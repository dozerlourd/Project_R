using System;
using UnityEngine;

namespace ProjectR.Player
{
    [DisallowMultipleComponent]
    public sealed class PlayerStats : MonoBehaviour
    {
        [Flags]
        public enum StatBuffTarget
        {
            None = 0,
            MaxHP = 1 << 0,
            HrRate = 1 << 1,
            HrTickTime = 1 << 2,
            HrDelayAfterDamaged = 1 << 3,
            MaxShield = 1 << 4,
            SrRate = 1 << 5,
            SrTickTime = 1 << 6,
            SrDelayAfterDamaged = 1 << 7,
            MoveSpeed = 1 << 8,
            DashSpeed = 1 << 9,
            DashDuration = 1 << 10,
            MaxDashCount = 1 << 11,
            DashCooltime = 1 << 12,
            AttackDamage = 1 << 13,
            AttackCooltime = 1 << 14,
            PickupRadius = 1 << 15,
            PickupMagnetRadius = 1 << 16,
            MinPickupMagnetSpeed = 1 << 17,
            MaxPickupMagnetSpeed = 1 << 18
        }

        private const int StatBuffCount = 19;

        private static readonly StatBuffTarget[] AllStatBuffTargets =
        {
            StatBuffTarget.MaxHP,
            StatBuffTarget.HrRate,
            StatBuffTarget.HrTickTime,
            StatBuffTarget.HrDelayAfterDamaged,
            StatBuffTarget.MaxShield,
            StatBuffTarget.SrRate,
            StatBuffTarget.SrTickTime,
            StatBuffTarget.SrDelayAfterDamaged,
            StatBuffTarget.MoveSpeed,
            StatBuffTarget.DashSpeed,
            StatBuffTarget.DashDuration,
            StatBuffTarget.MaxDashCount,
            StatBuffTarget.DashCooltime,
            StatBuffTarget.AttackDamage,
            StatBuffTarget.AttackCooltime,
            StatBuffTarget.PickupRadius,
            StatBuffTarget.PickupMagnetRadius,
            StatBuffTarget.MinPickupMagnetSpeed,
            StatBuffTarget.MaxPickupMagnetSpeed
        };

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

        /// <summary>Duration the player keeps moving with dashSpeed after starting a dash.</summary>
        [SerializeField, Min(0.01f)] private float dashDuration = 0.12f;

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

        public float MaxHP => GetBuffedStat(StatBuffTarget.MaxHP, maxHP, 1f);

        public float CurrentHP => currentHP;

        public float HrRate => GetBuffedStat(StatBuffTarget.HrRate, hrRate, 0f);

        public float HrTickTime => GetBuffedStat(StatBuffTarget.HrTickTime, hrTickTime, 0f);

        public float HrDelayAfterDamaged => GetBuffedStat(
            StatBuffTarget.HrDelayAfterDamaged,
            hrDelayAfterDamaged,
            0f);

        public float MaxShield => GetBuffedStat(StatBuffTarget.MaxShield, maxShield, 0f);

        public float CurrentShield => currentShield;

        public float SrRate => GetBuffedStat(StatBuffTarget.SrRate, srRate, 0f);

        public float SrTickTime => GetBuffedStat(StatBuffTarget.SrTickTime, srTickTime, 0f);

        public float SrDelayAfterDamaged => GetBuffedStat(
            StatBuffTarget.SrDelayAfterDamaged,
            srDelayAfterDamaged,
            0f);

        public float MoveSpeed => GetBuffedStat(StatBuffTarget.MoveSpeed, moveSpeed, 0f);

        public float DashSpeed => GetBuffedStat(StatBuffTarget.DashSpeed, dashSpeed, 0f);

        public float DashDuration => GetBuffedStat(StatBuffTarget.DashDuration, dashDuration, 0.01f);

        public byte MaxDashCount => (byte)Mathf.Clamp(
            Mathf.RoundToInt(GetBuffedStat(StatBuffTarget.MaxDashCount, maxDashCount, 0f)),
            0,
            byte.MaxValue);

        public byte CurrentDashCount => currentDashCount;

        public float DashCooltime => GetBuffedStat(StatBuffTarget.DashCooltime, dashCooltime, 0f);

        public float AttackDamage => GetBuffedStat(StatBuffTarget.AttackDamage, attackDamage, 0f);

        public float AttackCooltime => GetBuffedStat(StatBuffTarget.AttackCooltime, attackCooltime, 0f);

        public float PickupRadius => GetBuffedStat(StatBuffTarget.PickupRadius, pickupRadius, 0f);

        public float PickupMagnetRadius => GetBuffedStat(StatBuffTarget.PickupMagnetRadius, pickupMagnetRadius, 0f);

        public float MinPickupMagnetSpeed => GetBuffedStat(
            StatBuffTarget.MinPickupMagnetSpeed,
            minPickupMagnetSpeed,
            0f);

        public float MaxPickupMagnetSpeed => Mathf.Max(
            MinPickupMagnetSpeed,
            GetBuffedStat(StatBuffTarget.MaxPickupMagnetSpeed, maxPickupMagnetSpeed, 0f));

        public bool IsDead => currentHP <= 0f;

        public bool CanDash => currentDashCount > 0;

        public bool CanAttack => attackCooldownTimer <= 0f;

        private float healthRecoveryTimer;
        private float healthRecoveryDelayTimer;
        private float shieldRecoveryTimer;
        private float shieldRecoveryDelayTimer;
        private float dashRecoveryTimer;
        private float attackCooldownTimer;
        private readonly float[] temporaryStatBuffMultipliers = new float[StatBuffCount];
        private readonly float[] temporaryStatBuffAddValues = new float[StatBuffCount];
        private readonly float[] temporaryStatBuffTimers = new float[StatBuffCount];

        private void Awake()
        {
            ClampRuntimeValues();
            ResetTemporaryStatBuffs();
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            TickHealthRecovery(deltaTime);
            TickShieldRecovery(deltaTime);
            TickDashRecovery(deltaTime);
            TickAttackCooldown(deltaTime);
            TickTemporaryStatBuffs(deltaTime);
            ClampRuntimeStateValues();
        }

        public void SetCurrentHealth(float value)
        {
            currentHP = Mathf.Clamp(value, 0f, MaxHP);
        }

        public void RestoreToFullHealth()
        {
            currentHP = MaxHP;
        }

        public void SetCurrentShield(float value)
        {
            currentShield = Mathf.Clamp(value, 0f, MaxShield);
        }

        public void RestoreToFullShield()
        {
            currentShield = MaxShield;
        }

        public void RestoreAll()
        {
            RestoreToFullHealth();
            RestoreToFullShield();
            currentDashCount = MaxDashCount;
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
                shieldRecoveryDelayTimer = SrDelayAfterDamaged;
                shieldRecoveryTimer = 0f;
            }

            float healthDamage = 0f;
            if (remainingDamage > 0f)
            {
                healthDamage = Mathf.Min(currentHP, remainingDamage);
                currentHP -= healthDamage;
                healthRecoveryDelayTimer = HrDelayAfterDamaged;
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

            currentHP = Mathf.Min(MaxHP, currentHP + amount);
        }

        public void RecoverShield(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            currentShield = Mathf.Min(MaxShield, currentShield + amount);
        }

        public bool TryConsumeDash()
        {
            if (currentDashCount == 0)
            {
                return false;
            }

            currentDashCount--;
            if (currentDashCount < MaxDashCount && dashRecoveryTimer <= 0f)
            {
                dashRecoveryTimer = DashCooltime;
            }

            return true;
        }

        public void RecoverDashCharge()
        {
            if (currentDashCount >= MaxDashCount)
            {
                currentDashCount = MaxDashCount;
                dashRecoveryTimer = 0f;
                return;
            }

            currentDashCount++;
            dashRecoveryTimer = currentDashCount < MaxDashCount ? DashCooltime : 0f;
        }

        public bool TryStartAttackCooldown()
        {
            if (!CanAttack)
            {
                return false;
            }

            attackCooldownTimer = AttackCooltime;
            return true;
        }

        public void ApplyTemporaryStatBuff(
            StatBuffTarget targets,
            float multiplier,
            float addValue,
            float duration)
        {
            if (targets == StatBuffTarget.None || multiplier <= 0f || duration <= 0f)
            {
                return;
            }

            foreach (StatBuffTarget target in AllStatBuffTargets)
            {
                if ((targets & target) == 0)
                {
                    continue;
                }

                int index = GetStatBuffIndex(target);
                temporaryStatBuffMultipliers[index] = Mathf.Max(temporaryStatBuffMultipliers[index], multiplier);
                temporaryStatBuffAddValues[index] = Mathf.Max(temporaryStatBuffAddValues[index], addValue);
                temporaryStatBuffTimers[index] = Mathf.Max(temporaryStatBuffTimers[index], duration);
            }
        }

        public void ApplyStatBuff(
            StatBuffTarget targets,
            float multiplier,
            float addValue)
        {
            if (targets == StatBuffTarget.None || multiplier <= 0f)
            {
                return;
            }

            foreach (StatBuffTarget target in AllStatBuffTargets)
            {
                if ((targets & target) == 0)
                {
                    continue;
                }

                ApplyPermanentStatBuff(target, multiplier, addValue);
            }

            maxPickupMagnetSpeed = Mathf.Max(minPickupMagnetSpeed, maxPickupMagnetSpeed);
            ClampRuntimeStateValues();
        }

        private void OnValidate()
        {
            ClampRuntimeValues();
            maxPickupMagnetSpeed = Mathf.Max(minPickupMagnetSpeed, maxPickupMagnetSpeed);
        }

        private void TickHealthRecovery(float deltaTime)
        {
            if (currentHP >= MaxHP || HrRate <= 0f || IsDead)
            {
                return;
            }

            if (healthRecoveryDelayTimer > 0f)
            {
                healthRecoveryDelayTimer -= deltaTime;
                return;
            }

            RecoverHealth(CalculateRecoveryAmount(ref healthRecoveryTimer, HrTickTime, HrRate, deltaTime));
        }

        private void TickShieldRecovery(float deltaTime)
        {
            if (currentShield >= MaxShield || SrRate <= 0f)
            {
                return;
            }

            if (shieldRecoveryDelayTimer > 0f)
            {
                shieldRecoveryDelayTimer -= deltaTime;
                return;
            }

            RecoverShield(CalculateRecoveryAmount(ref shieldRecoveryTimer, SrTickTime, SrRate, deltaTime));
        }

        private void TickDashRecovery(float deltaTime)
        {
            if (currentDashCount >= MaxDashCount)
            {
                return;
            }

            if (DashCooltime <= 0f)
            {
                currentDashCount = MaxDashCount;
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

        private void TickTemporaryStatBuffs(float deltaTime)
        {
            for (int i = 0; i < StatBuffCount; i++)
            {
                if (temporaryStatBuffTimers[i] <= 0f)
                {
                    continue;
                }

                temporaryStatBuffTimers[i] -= deltaTime;
                if (temporaryStatBuffTimers[i] <= 0f)
                {
                    ClearTemporaryStatBuff(i);
                }
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

        private void ApplyPermanentStatBuff(StatBuffTarget target, float multiplier, float addValue)
        {
            switch (target)
            {
                case StatBuffTarget.MaxHP:
                    maxHP = ApplyStatValue(maxHP, multiplier, addValue, 1f);
                    break;
                case StatBuffTarget.HrRate:
                    hrRate = ApplyStatValue(hrRate, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.HrTickTime:
                    hrTickTime = ApplyStatValue(hrTickTime, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.HrDelayAfterDamaged:
                    hrDelayAfterDamaged = ApplyStatValue(hrDelayAfterDamaged, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.MaxShield:
                    maxShield = ApplyStatValue(maxShield, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.SrRate:
                    srRate = ApplyStatValue(srRate, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.SrTickTime:
                    srTickTime = ApplyStatValue(srTickTime, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.SrDelayAfterDamaged:
                    srDelayAfterDamaged = ApplyStatValue(srDelayAfterDamaged, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.MoveSpeed:
                    moveSpeed = ApplyStatValue(moveSpeed, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.DashSpeed:
                    dashSpeed = ApplyStatValue(dashSpeed, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.DashDuration:
                    dashDuration = ApplyStatValue(dashDuration, multiplier, addValue, 0.01f);
                    break;
                case StatBuffTarget.MaxDashCount:
                    maxDashCount = ApplyByteStatValue(maxDashCount, multiplier, addValue);
                    break;
                case StatBuffTarget.DashCooltime:
                    dashCooltime = ApplyStatValue(dashCooltime, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.AttackDamage:
                    attackDamage = ApplyStatValue(attackDamage, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.AttackCooltime:
                    attackCooltime = ApplyStatValue(attackCooltime, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.PickupRadius:
                    pickupRadius = ApplyStatValue(pickupRadius, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.PickupMagnetRadius:
                    pickupMagnetRadius = ApplyStatValue(pickupMagnetRadius, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.MinPickupMagnetSpeed:
                    minPickupMagnetSpeed = ApplyStatValue(minPickupMagnetSpeed, multiplier, addValue, 0f);
                    break;
                case StatBuffTarget.MaxPickupMagnetSpeed:
                    maxPickupMagnetSpeed = ApplyStatValue(maxPickupMagnetSpeed, multiplier, addValue, 0f);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        private static float ApplyStatValue(float value, float multiplier, float addValue, float minValue)
        {
            return Mathf.Max(minValue, value * multiplier + addValue);
        }

        private static byte ApplyByteStatValue(byte value, float multiplier, float addValue)
        {
            return (byte)Mathf.Clamp(
                Mathf.RoundToInt(ApplyStatValue(value, multiplier, addValue, 0f)),
                0,
                byte.MaxValue);
        }

        private void ClampRuntimeValues()
        {
            maxHP = Mathf.Max(1f, maxHP);
            dashDuration = Mathf.Max(0.01f, dashDuration);
            currentHP = Mathf.Clamp(currentHP, 0f, MaxHP);
            currentShield = Mathf.Clamp(currentShield, 0f, MaxShield);
            currentDashCount = (byte)Mathf.Clamp(currentDashCount, 0, MaxDashCount);
        }

        private void ClampRuntimeStateValues()
        {
            currentHP = Mathf.Min(currentHP, MaxHP);
            currentShield = Mathf.Min(currentShield, MaxShield);
            currentDashCount = (byte)Mathf.Clamp(currentDashCount, 0, MaxDashCount);
        }

        private float GetBuffedStat(StatBuffTarget target, float baseValue, float minValue)
        {
            int index = GetStatBuffIndex(target);
            float multiplier = temporaryStatBuffMultipliers[index] > 0f ? temporaryStatBuffMultipliers[index] : 1f;
            return Mathf.Max(minValue, baseValue * multiplier + temporaryStatBuffAddValues[index]);
        }

        private static int GetStatBuffIndex(StatBuffTarget target)
        {
            return target switch
            {
                StatBuffTarget.MaxHP => 0,
                StatBuffTarget.HrRate => 1,
                StatBuffTarget.HrTickTime => 2,
                StatBuffTarget.HrDelayAfterDamaged => 3,
                StatBuffTarget.MaxShield => 4,
                StatBuffTarget.SrRate => 5,
                StatBuffTarget.SrTickTime => 6,
                StatBuffTarget.SrDelayAfterDamaged => 7,
                StatBuffTarget.MoveSpeed => 8,
                StatBuffTarget.DashSpeed => 9,
                StatBuffTarget.DashDuration => 10,
                StatBuffTarget.MaxDashCount => 11,
                StatBuffTarget.DashCooltime => 12,
                StatBuffTarget.AttackDamage => 13,
                StatBuffTarget.AttackCooltime => 14,
                StatBuffTarget.PickupRadius => 15,
                StatBuffTarget.PickupMagnetRadius => 16,
                StatBuffTarget.MinPickupMagnetSpeed => 17,
                StatBuffTarget.MaxPickupMagnetSpeed => 18,
                _ => throw new ArgumentOutOfRangeException(nameof(target), target, null)
            };
        }

        private void ResetTemporaryStatBuffs()
        {
            for (int i = 0; i < StatBuffCount; i++)
            {
                ClearTemporaryStatBuff(i);
            }
        }

        private void ClearTemporaryStatBuff(int index)
        {
            temporaryStatBuffMultipliers[index] = 1f;
            temporaryStatBuffAddValues[index] = 0f;
            temporaryStatBuffTimers[index] = 0f;
        }
    }
}
