using System;
using UnityEngine;

namespace ProjectR.Items
{
    [RequireComponent(typeof(Collider2D))]
    [DisallowMultipleComponent]
    public sealed class LootingItem2D : MonoBehaviour
    {
        [SerializeField] private bool deactivateOnCollected = true;

        private Transform cachedTransform;
        private bool isCollected;

        public event Action<LootingItem2D, GameObject> Collected;

        public bool IsCollected => isCollected;

        private void Awake()
        {
            cachedTransform = transform;
            Collider2D itemCollider = GetComponent<Collider2D>();
            itemCollider.isTrigger = true;
        }

        public void MoveToward(Vector2 targetPosition, float speed, float deltaTime)
        {
            if (isCollected)
            {
                return;
            }

            Vector2 currentPosition = cachedTransform.position;
            Vector2 nextPosition = Vector2.MoveTowards(currentPosition, targetPosition, speed * deltaTime);
            cachedTransform.position = new Vector3(nextPosition.x, nextPosition.y, cachedTransform.position.z);
        }

        public void Collect(GameObject collector)
        {
            if (isCollected)
            {
                return;
            }

            isCollected = true;
            Collected?.Invoke(this, collector);

            if (deactivateOnCollected)
            {
                gameObject.SetActive(false);
            }
        }
    }
}
