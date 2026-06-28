using ProjectR.Player;
using UnityEngine;

namespace ProjectR.Items
{
    [RequireComponent(typeof(PickupItem2D))]
    [DisallowMultipleComponent]
    public sealed class MoveSpeedBuffPickupItem2D : MonoBehaviour
    {
        [SerializeField, Min(0.01f)] private float moveSpeedMultiplier = 1.2f;
        [SerializeField, Min(0.01f)] private float duration = 3f;

        private PickupItem2D pickupItem;

        private void Awake()
        {
            pickupItem = GetComponent<PickupItem2D>();
        }

        private void OnEnable()
        {
            if (pickupItem == null)
            {
                pickupItem = GetComponent<PickupItem2D>();
            }

            pickupItem.Collected += ApplyMoveSpeedBuff;
        }

        private void OnDisable()
        {
            if (pickupItem != null)
            {
                pickupItem.Collected -= ApplyMoveSpeedBuff;
            }
        }

        private void ApplyMoveSpeedBuff(PickupItem2D item, GameObject collector)
        {
            if (collector == null || !collector.TryGetComponent(out PlayerStats playerStats))
            {
                return;
            }

            playerStats.ApplyTemporaryMoveSpeedMultiplier(moveSpeedMultiplier, duration);
        }
    }
}
