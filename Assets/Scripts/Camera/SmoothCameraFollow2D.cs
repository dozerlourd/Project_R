using UnityEngine;

namespace ProjectR.CameraSystem
{
    [DisallowMultipleComponent]
    public sealed class SmoothCameraFollow2D : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 followOffset = new Vector3(0f, 0f, -10f);
        [SerializeField, Min(0f)] private float stopDistance = 0.5f;
        [SerializeField, Min(0.01f)] private float followSharpness = 10f;
        [SerializeField, Min(0f)] private float cameraMoveSpeed = 20f;
        [SerializeField] private AnimationCurve followLerpCurve = AnimationCurve.EaseInOut(0f, 0.25f, 1f, 1f);
        [SerializeField, Min(0f)] private float snapDistance = 20f;

        private void Awake()
        {
            EnsureFollowCurve();
            ResolveTarget();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + followOffset;
            Vector3 toDesiredPosition = desiredPosition - transform.position;
            float distanceToTarget = toDesiredPosition.magnitude;
            if (distanceToTarget <= stopDistance)
            {
                return;
            }

            if (snapDistance > 0f && distanceToTarget > snapDistance)
            {
                transform.position = desiredPosition;
                return;
            }

            Vector3 followPosition = desiredPosition - toDesiredPosition.normalized * stopDistance;
            float lerpAmount = EvaluateCurvedLerpAmount(Time.deltaTime, distanceToTarget);
            Vector3 lerpedPosition = Vector3.Lerp(transform.position, followPosition, lerpAmount);
            transform.position = MoveAtConfiguredSpeed(lerpedPosition, Time.deltaTime);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void ResolveTarget()
        {
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                target = player != null ? player.transform : null;
            }
        }

        private float EvaluateCurvedLerpAmount(float deltaTime, float distanceToTarget)
        {
            float maxCurveDistance = snapDistance > stopDistance ? snapDistance : stopDistance + 1f;
            float distanceRatio = Mathf.InverseLerp(stopDistance, maxCurveDistance, distanceToTarget);
            float curveMultiplier = followLerpCurve != null && followLerpCurve.length > 0
                ? Mathf.Max(0.01f, followLerpCurve.Evaluate(distanceRatio))
                : 1f;

            return 1f - Mathf.Exp(-followSharpness * curveMultiplier * deltaTime);
        }

        private Vector3 MoveAtConfiguredSpeed(Vector3 targetPosition, float deltaTime)
        {
            if (cameraMoveSpeed <= 0f)
            {
                return transform.position;
            }

            return Vector3.MoveTowards(transform.position, targetPosition, cameraMoveSpeed * deltaTime);
        }

        private void OnValidate()
        {
            followSharpness = Mathf.Max(0.01f, followSharpness);
            cameraMoveSpeed = Mathf.Max(0f, cameraMoveSpeed);
            stopDistance = Mathf.Max(0f, stopDistance);
            snapDistance = Mathf.Max(0f, snapDistance);

            EnsureFollowCurve();
        }

        private void EnsureFollowCurve()
        {
            if (followLerpCurve == null || followLerpCurve.length == 0)
            {
                followLerpCurve = AnimationCurve.EaseInOut(0f, 0.25f, 1f, 1f);
            }
        }
    }
}
