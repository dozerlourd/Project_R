using UnityEngine;

namespace ProjectR.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerStats : MonoBehaviour
    {
        /// <summary>Maximum health point value the player can have.</summary>
        [Header("Health")]
        [SerializeField, Min(1f)] private float maxHP = 100f;

        /// <summary>Current health point value remaining for the player.</summary>
        [SerializeField, Min(0f)] private float currentHP = 100f;

        /// <summary>Health recovery amount restored on each health recovery tick.</summary>
        [SerializeField, Min(0f)] private float hrRate = 1;

        /// <summary>Time interval between each health recovery tick.</summary>
        [Header("Health Cooltime")]
        [SerializeField, Min(0f)] private float hrTickTime = 1;

        /// <summary>Delay before health recovery starts after the player takes damage.</summary>
        [SerializeField, Min(0f)] private float hrDelayAfterDamaged = 1;

        /// <summary>Maximum shield value the player can have.</summary>
        [Header("Shield")]
        [SerializeField, Min(0f)] private float maxShield = 50f;

        /// <summary>Current shield value remaining for the player.</summary>
        [SerializeField, Min(0f)] private float currentShield = 50f;

        /// <summary>Shield recovery amount restored on each shield recovery tick.</summary>
        [SerializeField, Min(0f)] private float srRate = 1;

        /// <summary>Time interval between each shield recovery tick.</summary>
        [Header("Shield Cooltime")]
        [SerializeField, Min(0f)] private float srTickTime = 1;

        /// <summary>Delay before shield recovery starts after the player takes damage.</summary>
        [SerializeField, Min(0f)] private float srDelayAfterDamaged = 1;

        /// <summary>Base movement speed used by the player movement component.</summary>
        [Header("Movement")]
        [SerializeField, Min(0f)] private float moveSpeed = 5f;

        /// <summary>Movement speed applied while the player is dashing.</summary>
        [SerializeField, Min(0f)] private float dashSpeed = 15f;

        /// <summary>Maximum number of dash charges the player can store.</summary>
        [SerializeField, Min(0f)] private byte maxDashCount = 1;

        /// <summary>Current number of dash charges available to the player.</summary>
        [SerializeField, Min(0f)] private byte currentDashCount = 1;

        /// <summary>Cooldown time before a dash charge can be recovered.</summary>
        [Header("Movement Cooltime")]
        [SerializeField, Min(0f)] private float dashCooltime = 3f;

        /// <summary>Base damage dealt by the player's attack.</summary>
        [Header("Combat")]
        [SerializeField, Min(0f)] private float attackDamage = 10f;

        /// <summary>Cooldown time between player attacks.</summary>
        [Header("Combat Cooltime")]
        [SerializeField, Min(0f)] private float attackCooltime = 0.5f;

        /// <summary>Radius where an item is always considered picked up if it is close enough to the player.</summary>
        [Header("Utility")]
        [SerializeField, Min(0f)] private float pickupRadius = 1.2f;

        /// <summary>Radius where an item starts being pulled toward the player.</summary>
        [SerializeField, Min(0f)] private float pickupMagnetRadius = 3.5f;

        /// <summary>Minimum speed used when an item starts being pulled from the edge of pickupMagnetRadius.</summary>
        [SerializeField, Min(0f)] private float minPickupMagnetSpeed = 1f;

        /// <summary>Maximum speed used as an item gets closer to pickupRadius.</summary>
        [SerializeField, Min(0f)] private float maxPickupMagnetSpeed = 5f;

        // Add new player stat variables here while the prototype is still evolving.

        public float MaxHP => maxHP;

        public float CurrentHP => currentHP;

        public float HrRate => hrRate;

        public float HrTickTime => hrTickTime;

        public float HrDelayAfterDamaged => hrDelayAfterDamaged;

        public float MaxShield => maxShield;

        public float CurrentShield => currentShield;

        public float SrRate => srRate;

        public float SrTickTime => srTickTime;

        public float SrDelayAfterDamaged => srDelayAfterDamaged;

        public float MoveSpeed => moveSpeed * temporaryMoveSpeedMultiplier;

        public float DashSpeed => dashSpeed;

        public byte MaxDashCount => maxDashCount;

        public byte CurrentDashCount => currentDashCount;

        public float DashCooltime => dashCooltime;

        public float AttackDamage => attackDamage;

        public float AttackCooltime => attackCooltime;

        public float PickupRadius => pickupRadius;

        public float PickupMagnetRadius => pickupMagnetRadius;

        public float MinPickupMagnetSpeed => minPickupMagnetSpeed;

        public float MaxPickupMagnetSpeed => maxPickupMagnetSpeed;

        public bool IsDead => currentHP <= 0f;

        public bool CanDash => currentDashCount > 0;

        public bool CanAttack => attackCooldownTimer <= 0f;

        private float healthRecoveryTimer;
        private float healthRecoveryDelayTimer;
        private float shieldRecoveryTimer;
        private float shieldRecoveryDelayTimer;
        private float dashRecoveryTimer;
        private float attackCooldownTimer;
        private float temporaryMoveSpeedMultiplier = 1f;
        private float temporaryMoveSpeedTimer;

        private void Awake()
        {
            ClampRuntimeValues();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            TickHealthRecovery(deltaTime);
            TickShieldRecovery(deltaTime);
            TickDashRecovery(deltaTime);
            TickAttackCooldown(deltaTime);
            TickTemporaryMoveSpeed(deltaTime);
        }

        public void SetCurrentHealth(float value)
        {
            currentHP = Mathf.Clamp(value, 0f, maxHP);
        }

        public void RestoreToFullHealth()
        {
            currentHP = maxHP;
        }

        public void SetCurrentShield(float value)
        {
            currentShield = Mathf.Clamp(value, 0f, maxShield);
        }

        public void RestoreToFullShield()
        {
            currentShield = maxShield;
        }

        public void RestoreAll()
        {
            RestoreToFullHealth();
            RestoreToFullShield();
            currentDashCount = maxDashCount;
        }

        public float TakeDamage(float damage)
        {
            if (damage <= 0f || IsDead)
            {
                return 0f;
            }

            float remainingDamage = damage;
            if (currentShield > 0f)
            {
                float shieldDamage = Mathf.Min(currentShield, remainingDamage);
                currentShield -= shieldDamage;
                remainingDamage -= shieldDamage;
                shieldRecoveryDelayTimer = srDelayAfterDamaged;
                shieldRecoveryTimer = 0f;
            }

            float healthDamage = 0f;
            if (remainingDamage > 0f)
            {
                healthDamage = Mathf.Min(currentHP, remainingDamage);
                currentHP -= healthDamage;
                healthRecoveryDelayTimer = hrDelayAfterDamaged;
                healthRecoveryTimer = 0f;
            }

            return healthDamage;
        }

        public void RecoverHealth(float amount)
        {
            if (amount <= 0f || IsDead)
            {
                return;
            }

            currentHP = Mathf.Min(maxHP, currentHP + amount);
        }

        public void RecoverShield(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            currentShield = Mathf.Min(maxShield, currentShield + amount);
        }

        public bool TryConsumeDash()
        {
            if (currentDashCount == 0)
            {
                return false;
            }

            currentDashCount--;
            if (currentDashCount < maxDashCount && dashRecoveryTimer <= 0f)
            {
                dashRecoveryTimer = dashCooltime;
            }

            return true;
        }

        public void RecoverDashCharge()
        {
            if (currentDashCount >= maxDashCount)
            {
                currentDashCount = maxDashCount;
                dashRecoveryTimer = 0f;
                return;
            }

            currentDashCount++;
            dashRecoveryTimer = currentDashCount < maxDashCount ? dashCooltime : 0f;
        }

        public bool TryStartAttackCooldown()
        {
            if (!CanAttack)
            {
                return false;
            }

            attackCooldownTimer = attackCooltime;
            return true;
        }

        public void ApplyTemporaryMoveSpeedMultiplier(float multiplier, float duration)
        {
            if (multiplier <= 0f || duration <= 0f)
            {
                return;
            }

            temporaryMoveSpeedMultiplier = Mathf.Max(temporaryMoveSpeedMultiplier, multiplier);
            temporaryMoveSpeedTimer = Mathf.Max(temporaryMoveSpeedTimer, duration);
        }

        public void ClearTemporaryMoveSpeedMultiplier()
        {
            temporaryMoveSpeedMultiplier = 1f;
            temporaryMoveSpeedTimer = 0f;
        }

        private void OnValidate()
        {
            ClampRuntimeValues();
            maxPickupMagnetSpeed = Mathf.Max(minPickupMagnetSpeed, maxPickupMagnetSpeed);
        }

        private void TickHealthRecovery(float deltaTime)
        {
            if (currentHP >= maxHP || hrRate <= 0f || IsDead)
            {
                return;
            }

            if (healthRecoveryDelayTimer > 0f)
            {
                healthRecoveryDelayTimer -= deltaTime;
                return;
            }

            RecoverHealth(CalculateRecoveryAmount(ref healthRecoveryTimer, hrTickTime, hrRate, deltaTime));
        }

        private void TickShieldRecovery(float deltaTime)
        {
            if (currentShield >= maxShield || srRate <= 0f)
            {
                return;
            }

            if (shieldRecoveryDelayTimer > 0f)
            {
                shieldRecoveryDelayTimer -= deltaTime;
                return;
            }

            RecoverShield(CalculateRecoveryAmount(ref shieldRecoveryTimer, srTickTime, srRate, deltaTime));
        }

        private void TickDashRecovery(float deltaTime)
        {
            if (currentDashCount >= maxDashCount)
            {
                return;
            }

            if (dashCooltime <= 0f)
            {
                currentDashCount = maxDashCount;
                dashRecoveryTimer = 0f;
                return;
            }

            dashRecoveryTimer -= deltaTime;
            if (dashRecoveryTimer <= 0f)
            {
                RecoverDashCharge();
            }
        }

        private void TickAttackCooldown(float deltaTime)
        {
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= deltaTime;
            }
        }

        private void TickTemporaryMoveSpeed(float deltaTime)
        {
            if (temporaryMoveSpeedTimer <= 0f)
            {
                return;
            }

            temporaryMoveSpeedTimer -= deltaTime;
            if (temporaryMoveSpeedTimer <= 0f)
            {
                ClearTemporaryMoveSpeedMultiplier();
            }
        }

        private static float CalculateRecoveryAmount(ref float timer, float tickTime, float amount, float deltaTime)
        {
            if (tickTime <= 0f)
            {
                return amount * deltaTime;
            }

            timer -= deltaTime;
            if (timer <= 0f)
            {
                timer = tickTime;
                return amount;
            }

            return 0f;
        }

        private void ClampRuntimeValues()
        {
            maxHP = Mathf.Max(1f, maxHP);
            currentHP = Mathf.Clamp(currentHP, 0f, maxHP);
            currentShield = Mathf.Clamp(currentShield, 0f, maxShield);
            currentDashCount = (byte)Mathf.Clamp(currentDashCount, 0, maxDashCount);
            temporaryMoveSpeedMultiplier = Mathf.Max(1f, temporaryMoveSpeedMultiplier);
        }
    }
}
