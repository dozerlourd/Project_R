using ProjectR.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectR.Player
{
    [RequireComponent(typeof(PlayerStats))]
    [DisallowMultipleComponent]
    public sealed class PlayerPickupCollector2D : MonoBehaviour
    {
        [FormerlySerializedAs("pickupLayerMask")]
        [SerializeField] private LayerMask autoLootingItemLayerMask = 1 << 8;
        [SerializeField, Min(1)] private int maxPickupChecks = 64;

        private PlayerStats stats;
        private Collider2D[] autoLootingItemBuffer;
        private ContactFilter2D autoLootingItemFilter;

        private void Awake()
        {
            stats = GetComponent<PlayerStats>();
            autoLootingItemBuffer = new Collider2D[maxPickupChecks];
            ConfigureAutoLootingItemFilter();
        }

        private void Update()
        {
            if (stats == null || stats.PickupMagnetRadius <= 0f)
            {
                return;
            }

            int pickupCount = Physics2D.OverlapCircle(
                transform.position,
                stats.PickupMagnetRadius,
                autoLootingItemFilter,
                autoLootingItemBuffer);

            for (int i = 0; i < pickupCount; i++)
            {
                TryProcessPickup(autoLootingItemBuffer[i]);
                autoLootingItemBuffer[i] = null;
            }
        }

        private void TryProcessPickup(Collider2D pickupCollider)
        {
            if (pickupCollider == null)
            {
                return;
            }

            LootingItem2D pickupItem = pickupCollider.GetComponentInParent<LootingItem2D>();
            if (pickupItem == null || pickupItem.IsCollected)
            {
                return;
            }

            Vector2 playerPosition = transform.position;
            Vector2 itemPosition = pickupItem.transform.position;
            Vector2 toPlayer = playerPosition - itemPosition;
            float pickupRadius = stats.PickupRadius;
            float sqrDistance = toPlayer.sqrMagnitude;

            if (sqrDistance <= pickupRadius * pickupRadius)
            {
                pickupItem.Collect(gameObject);
                return;
            }

            float distance = Mathf.Sqrt(sqrDistance);
            float magnetSpeed = CalculateMagnetSpeed(distance);
            pickupItem.MoveToward(playerPosition, magnetSpeed, Time.deltaTime);

            Vector2 afterMoveToPlayer = playerPosition - (Vector2)pickupItem.transform.position;
            if (afterMoveToPlayer.sqrMagnitude <= pickupRadius * pickupRadius)
            {
                pickupItem.Collect(gameObject);
            }
        }

        private float CalculateMagnetSpeed(float distance)
        {
            if (stats.PickupMagnetRadius <= stats.PickupRadius)
            {
                return stats.MaxPickupMagnetSpeed;
            }

            float speedRatio = 1f - Mathf.InverseLerp(stats.PickupRadius, stats.PickupMagnetRadius, distance);
            return Mathf.Lerp(stats.MinPickupMagnetSpeed, stats.MaxPickupMagnetSpeed, speedRatio);
        }

        private void OnValidate()
        {
            maxPickupChecks = Mathf.Max(1, maxPickupChecks);
            ConfigureAutoLootingItemFilter();
        }

        private void ConfigureAutoLootingItemFilter()
        {
            autoLootingItemFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true
            };
            autoLootingItemFilter.SetLayerMask(autoLootingItemLayerMask);
        }
    }
}
