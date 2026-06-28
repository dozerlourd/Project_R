using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectR.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CapsuleCollider2D))]
    [RequireComponent(typeof(PlayerStats))]
    [DisallowMultipleComponent]
    public sealed class PlayerMovement2D : MonoBehaviour
    {
        [SerializeField] private bool requireWalkableSurface = true;
        [SerializeField] private LayerMask walkableLayerMask = 1 << 9;
        [SerializeField, Min(0.01f)] private float walkableCheckRadius = 0.08f;
        [SerializeField] private bool flipSpriteToMoveDirection = true;

        private PlayerStats stats;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Vector2 moveInput;
        private Vector2 lastMoveDirection = Vector2.down;
        private Vector2 dashDirection;
        private float dashTimeRemaining;
        private bool dashRequested;
        private ContactFilter2D walkableFilter;
        private readonly Collider2D[] walkableHits = new Collider2D[1];

        private void Awake()
        {
            stats = GetOrAddComponent<PlayerStats>();
            body = GetOrAddComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            ConfigureWalkableFilter();
            ConfigureBody();
            ConfigureCollider();
        }

        private void Update()
        {
            moveInput = ReadMoveInput();
            if (moveInput.sqrMagnitude > 0.0001f)
            {
                lastMoveDirection = moveInput.normalized;
            }

            dashRequested |= ReadDashInput();
            UpdateFacing(moveInput);
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime;
            TryStartDash();

            if (dashTimeRemaining > 0f)
            {
                dashTimeRemaining -= deltaTime;
                TryMove(dashDirection * (GetDashSpeed() * deltaTime));
                return;
            }

            TryMove(moveInput * (GetMoveSpeed() * deltaTime));
        }

        private static Vector2 ReadMoveInput()
        {
            Vector2 input = Vector2.zero;

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
                {
                    input.y += 1f;
                }

                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
                {
                    input.y -= 1f;
                }

                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
                {
                    input.x += 1f;
                }

                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
                {
                    input.x -= 1f;
                }
            }

            Gamepad gamepad = Gamepad.current;
            if (gamepad != null)
            {
                Vector2 stickInput = gamepad.leftStick.ReadValue();
                if (stickInput.sqrMagnitude > input.sqrMagnitude)
                {
                    input = stickInput;
                }
            }

            return input.sqrMagnitude > 1f ? input.normalized : input;
        }

        private static bool ReadDashInput()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.leftShiftKey.wasPressedThisFrame))
            {
                return true;
            }

            Gamepad gamepad = Gamepad.current;
            return gamepad != null && gamepad.buttonSouth.wasPressedThisFrame;
        }

        private void UpdateFacing(Vector2 input)
        {
            if (!flipSpriteToMoveDirection || spriteRenderer == null || Mathf.Abs(input.x) < 0.01f)
            {
                return;
            }

            spriteRenderer.flipX = input.x < 0f;
        }

        private void ConfigureBody()
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
        }

        private void ConfigureCollider()
        {
            CapsuleCollider2D playerCollider = GetOrAddComponent<CapsuleCollider2D>();
            playerCollider.size = new Vector2(0.55f, 0.8f);
            playerCollider.offset = new Vector2(0f, -0.05f);
            playerCollider.direction = CapsuleDirection2D.Vertical;
            playerCollider.isTrigger = false;
        }

        private float GetMoveSpeed()
        {
            return stats.MoveSpeed;
        }

        private float GetDashSpeed()
        {
            return stats.DashSpeed;
        }

        private void TryStartDash()
        {
            if (!dashRequested)
            {
                return;
            }

            dashRequested = false;
            if (!stats.TryConsumeDash())
            {
                return;
            }

            dashDirection = moveInput.sqrMagnitude > 0.0001f ? moveInput.normalized : lastMoveDirection;
            dashTimeRemaining = stats.DashDuration;
        }

        private void TryMove(Vector2 displacement)
        {
            if (displacement.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector2 nextPosition = body.position + displacement;
            if (!CanStandAt(nextPosition))
            {
                return;
            }

            body.MovePosition(nextPosition);
        }

        private bool CanStandAt(Vector2 position)
        {
            if (!requireWalkableSurface)
            {
                return true;
            }

            return Physics2D.OverlapCircle(position, walkableCheckRadius, walkableFilter, walkableHits) > 0;
        }

        private void OnValidate()
        {
            walkableCheckRadius = Mathf.Max(0.01f, walkableCheckRadius);
            ConfigureWalkableFilter();
        }

        private void ConfigureWalkableFilter()
        {
            walkableFilter = new ContactFilter2D
            {
                useTriggers = true,
                useLayerMask = true
            };
            walkableFilter.SetLayerMask(walkableLayerMask);
        }

        private T GetOrAddComponent<T>() where T : Component
        {
            T component = GetComponent<T>();
            return component != null ? component : gameObject.AddComponent<T>();
        }
    }
}
