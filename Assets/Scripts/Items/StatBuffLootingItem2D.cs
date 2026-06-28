using ProjectR.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectR.Items
{
    [RequireComponent(typeof(LootingItem2D))]
    [DisallowMultipleComponent]
    public sealed class StatBuffLootingItem2D : MonoBehaviour
    {
        [SerializeField] private bool isTemporaryBuff = true;

        [Header("Buff Value")]
        [FormerlySerializedAs("moveSpeedMultiplier")]
        [SerializeField, Min(0.01f)] private float buffMultiplier = 1.2f;
        [SerializeField, Min(0f)] private float buffAddValue;
        [SerializeField, Min(0.01f)] private float duration = 3f;

        [Header("Health")]
        [SerializeField] private bool buffMaxHP;
        [SerializeField] private bool buffHrRate;
        [SerializeField] private bool buffHrTickTime;
        [SerializeField] private bool buffHrDelayAfterDamaged;

        [Header("Shield")]
        [SerializeField] private bool buffMaxShield;
        [SerializeField] private bool buffSrRate;
        [SerializeField] private bool buffSrTickTime;
        [SerializeField] private bool buffSrDelayAfterDamaged;

        [Header("Movement")]
        [SerializeField] private bool buffMoveSpeed = true;
        [SerializeField] private bool buffDashSpeed;
        [SerializeField] private bool buffDashDuration;
        [SerializeField] private bool buffMaxDashCount;
        [SerializeField] private bool buffDashCooltime;

        [Header("Combat")]
        [SerializeField] private bool buffAttackDamage;
        [SerializeField] private bool buffAttackCooltime;

        [Header("Utility")]
        [SerializeField] private bool buffPickupRadius;
        [SerializeField] private bool buffPickupMagnetRadius;
        [SerializeField] private bool buffMinPickupMagnetSpeed;
        [SerializeField] private bool buffMaxPickupMagnetSpeed;

        private LootingItem2D lootingItem;

        private void Awake()
        {
            lootingItem = GetComponent<LootingItem2D>();
        }

        private void OnEnable()
        {
            if (lootingItem == null)
            {
                lootingItem = GetComponent<LootingItem2D>();
            }

            lootingItem.Collected += ApplyStatBuffMethod;
        }

        private void OnDisable()
        {
            if (lootingItem != null)
            {
                lootingItem.Collected -= ApplyStatBuffMethod;
            }
        }

        private void ApplyStatBuffMethod(LootingItem2D item, GameObject collector)
        {
            if (collector == null || !collector.TryGetComponent(out PlayerStats playerStats))
            {
                return;
            }

            if(isTemporaryBuff)
            {
                playerStats.ApplyTemporaryStatBuff(BuildBuffTargets(), buffMultiplier, buffAddValue, duration);
            }
            else
            {
                playerStats.ApplyStatBuff(BuildBuffTargets(), buffMultiplier, buffAddValue);
            }
        }

        private PlayerStats.StatBuffTarget BuildBuffTargets()
        {
            PlayerStats.StatBuffTarget targets = PlayerStats.StatBuffTarget.None;

            AddTarget(ref targets, buffMaxHP, PlayerStats.StatBuffTarget.MaxHP);
            AddTarget(ref targets, buffHrRate, PlayerStats.StatBuffTarget.HrRate);
            AddTarget(ref targets, buffHrTickTime, PlayerStats.StatBuffTarget.HrTickTime);
            AddTarget(ref targets, buffHrDelayAfterDamaged, PlayerStats.StatBuffTarget.HrDelayAfterDamaged);
            AddTarget(ref targets, buffMaxShield, PlayerStats.StatBuffTarget.MaxShield);
            AddTarget(ref targets, buffSrRate, PlayerStats.StatBuffTarget.SrRate);
            AddTarget(ref targets, buffSrTickTime, PlayerStats.StatBuffTarget.SrTickTime);
            AddTarget(ref targets, buffSrDelayAfterDamaged, PlayerStats.StatBuffTarget.SrDelayAfterDamaged);
            AddTarget(ref targets, buffMoveSpeed, PlayerStats.StatBuffTarget.MoveSpeed);
            AddTarget(ref targets, buffDashSpeed, PlayerStats.StatBuffTarget.DashSpeed);
            AddTarget(ref targets, buffDashDuration, PlayerStats.StatBuffTarget.DashDuration);
            AddTarget(ref targets, buffMaxDashCount, PlayerStats.StatBuffTarget.MaxDashCount);
            AddTarget(ref targets, buffDashCooltime, PlayerStats.StatBuffTarget.DashCooltime);
            AddTarget(ref targets, buffAttackDamage, PlayerStats.StatBuffTarget.AttackDamage);
            AddTarget(ref targets, buffAttackCooltime, PlayerStats.StatBuffTarget.AttackCooltime);
            AddTarget(ref targets, buffPickupRadius, PlayerStats.StatBuffTarget.PickupRadius);
            AddTarget(ref targets, buffPickupMagnetRadius, PlayerStats.StatBuffTarget.PickupMagnetRadius);
            AddTarget(ref targets, buffMinPickupMagnetSpeed, PlayerStats.StatBuffTarget.MinPickupMagnetSpeed);
            AddTarget(ref targets, buffMaxPickupMagnetSpeed, PlayerStats.StatBuffTarget.MaxPickupMagnetSpeed);

            return targets;
        }

        private static void AddTarget(
            ref PlayerStats.StatBuffTarget targets,
            bool shouldBuff,
            PlayerStats.StatBuffTarget target)
        {
            if (shouldBuff)
            {
                targets |= target;
            }
        }
    }
}
