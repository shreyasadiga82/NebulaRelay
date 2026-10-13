using UnityEngine;
using UnityEngine.InputSystem;

namespace NebulaRelay.Player
{
    /// <summary>
    /// DroneController manages the courier drone's physics-based movement using Rigidbody2D.
    /// Handles keyboard thrust, inertia/momentum, speed clamping, and linear damping.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [SelectionBase]
    public class DroneController : MonoBehaviour
    {
        [Header("Thrust & Speed Settings")]
        [Tooltip("Thrust acceleration applied in units/second^2.")]
        [SerializeField] private float thrustAcceleration = 6.0f;

        [Tooltip("Maximum allowed speed in units/second.")]
        [SerializeField] private float maxSpeed = 8.0f;

        [Tooltip("Linear damping / drag opposing motion in space.")]
        [SerializeField] private float linearDrag = 0.2f;

        [Header("Steering / Heading")]
        [Tooltip("Whether the drone rotates to face its movement/thrust direction.")]
        [SerializeField] private bool autoRotateToDirection = true;

        [Tooltip("Angular rotation speed when turning towards heading.")]
        [SerializeField] private float rotationSpeed = 10.0f;

        [Header("Flight Corridor Boundary")]
        [Tooltip("Enable corridor boundary clamping.")]
        [SerializeField] private bool clampToFlightCorridor = true;
        [SerializeField] private Vector2 minCorridorBounds = new Vector2(-20f, -6.5f);
        [SerializeField] private Vector2 maxCorridorBounds = new Vector2(20f, 6.5f);
        private float lastBoundaryWarningTime = -10f;

        [Header("Debug & Gizmos")]
        [Tooltip("Draw velocity and thrust vectors in Scene view.")]
        [SerializeField] private bool drawGizmoVectors = true;

        // Component References
        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        // Input & State
        private Vector2 inputDirection = Vector2.zero;

        // Public Properties for HUD, Battery, and Delivery systems
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public float Speed => Velocity.magnitude;
        public float MaxSpeed => maxSpeed;
        public float ThrustAcceleration => thrustAcceleration;
        public float LinearDrag => linearDrag;
        public bool IsThrusting => inputDirection.sqrMagnitude > 0.001f;
        public Vector2 ThrustDirection => inputDirection;
        public Vector2 MinCorridorBounds => minCorridorBounds;
        public Vector2 MaxCorridorBounds => maxCorridorBounds;

        public void SetFlightCorridor(float minX, float maxX, float maxY = 6.5f)
        {
            minCorridorBounds = new Vector2(minX, -maxY);
            maxCorridorBounds = new Vector2(maxX, maxY);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            ConfigureRigidbody();
            EnsureVisibleSprite();
        }

        private void ConfigureRigidbody()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Zero-gravity 2D space flight
            rb.linearDamping = linearDrag;
            rb.angularDamping = 1.0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate; // Smooth visuals
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        /// <summary>
        /// Ensures the player object has a visible sprite even if no art asset is assigned.
        /// </summary>
        private void EnsureVisibleSprite()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateProceduralDroneSprite();
                spriteRenderer.color = Color.white;
            }

            // Ensure rendering sorting order is prominent
            spriteRenderer.sortingOrder = 5;
        }

        private void Update()
        {
            CaptureInput();
        }

        private void FixedUpdate()
        {
            ApplyThrustPhysics();
            ApplyOrientationPhysics();
            ApplyFlightBoundaryPhysics();
        }

        /// <summary>
        /// Captures player input from WASD / Arrow keys using New Input System.
        /// </summary>
        private void CaptureInput()
        {
            Vector2 rawInput = Vector2.zero;

            if (Keyboard.current != null)
            {
                var kb = Keyboard.current;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) rawInput.y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) rawInput.y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) rawInput.x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) rawInput.x += 1f;
            }

            // Normalize so diagonal movement does not accelerate faster
            inputDirection = rawInput.sqrMagnitude > 1f ? rawInput.normalized : rawInput;
        }

        /// <summary>
        /// Applies Newton physics thrust using Rigidbody2D forces and clamps maximum velocity.
        /// </summary>
        private void ApplyThrustPhysics()
        {
            // Sync drag in case it is adjusted in Inspector at runtime
            rb.linearDamping = linearDrag;

            // Apply continuous thrust force (F = m * a)
            if (IsThrusting)
            {
                Vector2 force = inputDirection * thrustAcceleration * rb.mass;
                rb.AddForce(force, ForceMode2D.Force);
            }

            // Clamp velocity to maximum speed
            if (rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }

        /// <summary>
        /// Soft-clamps the drone within the sector flight corridor, zeroes outward velocity on contact,
        /// and flashes the boundary HUD alert.
        /// </summary>
        private void ApplyFlightBoundaryPhysics()
        {
            if (!clampToFlightCorridor || rb == null) return;

            Vector2 pos = rb.position;
            Vector2 vel = rb.linearVelocity;
            bool clamped = false;

            float minX = minCorridorBounds.x;
            float maxX = maxCorridorBounds.x;
            float minY = minCorridorBounds.y;
            float maxY = maxCorridorBounds.y;

            if (NebulaRelay.Core.LevelManager.Instance != null)
            {
                minX = NebulaRelay.Core.LevelManager.Instance.FlightMinX;
                maxX = NebulaRelay.Core.LevelManager.Instance.FlightMaxX;
                minY = NebulaRelay.Core.LevelManager.Instance.FlightMinY;
                maxY = NebulaRelay.Core.LevelManager.Instance.FlightMaxY;
            }

            if (pos.x < minX)
            {
                pos.x = minX;
                if (vel.x < 0f) vel.x = 0f;
                clamped = true;
            }
            else if (pos.x > maxX)
            {
                pos.x = maxX;
                if (vel.x > 0f) vel.x = 0f;
                clamped = true;
            }

            if (pos.y < minY)
            {
                pos.y = minY;
                if (vel.y < 0f) vel.y = 0f;
                clamped = true;
            }
            else if (pos.y > maxY)
            {
                pos.y = maxY;
                if (vel.y > 0f) vel.y = 0f;
                clamped = true;
            }

            if (clamped)
            {
                rb.position = pos;
                rb.linearVelocity = vel;

                if (Time.time - lastBoundaryWarningTime > 1.2f)
                {
                    lastBoundaryWarningTime = Time.time;
                    NebulaRelay.Core.LevelManager.Instance?.SetHUDMessage("WARNING: FLIGHT CORRIDOR BOUNDARY!", 1.5f, true);
                    NebulaRelay.Effects.AudioManager.Instance?.PlayWarning();
                }
            }
        }

        /// <summary>
        /// Rotates the drone smoothly towards heading/thrust direction using Rigidbody2D.MoveRotation.
        /// </summary>
        private void ApplyOrientationPhysics()
        {
            if (!autoRotateToDirection) return;

            Vector2 targetHeading = IsThrusting ? inputDirection : rb.linearVelocity;

            if (targetHeading.sqrMagnitude > 0.05f)
            {
                // In 2D, Atan2(y, x) is 0 along +X. If sprite faces +Y (Up), subtract 90 degrees.
                float targetAngle = Mathf.Atan2(targetHeading.y, targetHeading.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = rb.rotation;
                float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, rotationSpeed * Time.fixedDeltaTime);
                rb.MoveRotation(newAngle);
            }
        }

        /// <summary>
        /// Creates a clean procedural 64x64 courier drone sprite if none is assigned in the Inspector.
        /// </summary>
        private static Sprite CreateProceduralDroneSprite()
        {
            int width = 64;
            int height = 64;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);
            Color cyan = new Color(0.1f, 0.85f, 1.0f, 1.0f);
            Color white = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            Color darkCore = new Color(0.05f, 0.2f, 0.3f, 0.9f);

            // Fill transparent
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    tex.SetPixel(x, y, transparent);

            // Draw arrow/chevron drone pointing UP (+Y)
            Vector2 tip = new Vector2(32, 58);
            Vector2 leftWing = new Vector2(10, 8);
            Vector2 notch = new Vector2(32, 18);
            Vector2 rightWing = new Vector2(54, 8);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    if (IsPointInTriangle(p, tip, leftWing, notch) || IsPointInTriangle(p, tip, notch, rightWing))
                    {
                        // Gradient from core to cyan edge
                        float distToTip = Vector2.Distance(p, tip) / 50f;
                        Color col = Color.Lerp(white, cyan, distToTip);
                        if (y < 25 && Mathf.Abs(x - 32) < 8) col = darkCore;
                        tex.SetPixel(x, y, col);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 32f);
        }

        private static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
            bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
            return !(hasNeg && hasPos);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
        }

        private void OnDrawGizmos()
        {
            if (!drawGizmoVectors || !Application.isPlaying || rb == null) return;

            Vector3 pos = transform.position;

            // Velocity Vector (Cyan)
            if (rb.linearVelocity.sqrMagnitude > 0.01f)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(pos, pos + (Vector3)rb.linearVelocity);
                Gizmos.DrawWireSphere(pos + (Vector3)rb.linearVelocity, 0.15f);
            }

            // Thrust Input Vector (Yellow)
            if (IsThrusting)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos, pos + (Vector3)inputDirection * 1.5f);
            }
        }
    }
}
