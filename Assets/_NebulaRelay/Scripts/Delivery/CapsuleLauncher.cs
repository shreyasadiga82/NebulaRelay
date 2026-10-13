using UnityEngine;
using UnityEngine.InputSystem;
using NebulaRelay.Player;

namespace NebulaRelay.Delivery
{
    /// <summary>
    /// CapsuleLauncher handles mouse aiming, trajectory calculation, and capsule firing.
    /// Can be attached directly to the PlayerDrone GameObject.
    /// </summary>
    [SelectionBase]
    public class CapsuleLauncher : MonoBehaviour
    {
        [Header("Launch Configuration")]
        [Tooltip("The Energy Capsule prefab to instantiate when firing.")]
        [SerializeField] private GameObject capsulePrefab;

        [Tooltip("Launch speed in units/second (Default: 12).")]
        [SerializeField] private float launchSpeed = 12.0f;

        [Tooltip("Optional transform for spawn origin. If unassigned, uses player position + launchOffset.")]
        [SerializeField] private Transform launchPoint;

        [Tooltip("Distance in front of drone to spawn capsule (avoids overlapping player collider).")]
        [SerializeField] private float launchOffset = 0.85f;

        [Tooltip("Inherit player drone's current velocity in the launch vector.")]
        [SerializeField] private bool inheritDroneVelocity = false;

        [Header("Controls & Rate Limiting")]
        [Tooltip("Keyboard key to launch capsule (Default: Space).")]
        [SerializeField] private KeyCode fireKey = KeyCode.Space;

        [Tooltip("Also allow Left Mouse Click to fire.")]
        [SerializeField] private bool allowMouseClickFire = true;

        [Tooltip("Cooldown delay in seconds between consecutive shots.")]
        [SerializeField] private float fireCooldown = 0.5f;

        [Tooltip("Maximum active capsules permitted in flight simultaneously (1 ensures puzzle focus).")]
        [SerializeField] private int maxSimultaneousCapsules = 1;

        [Header("Debug & Gizmos")]
        [Tooltip("Draw aim direction and predicted launch vector in Scene view.")]
        [SerializeField] private bool drawAimGizmo = true;

        // References
        private Camera mainCamera;
        private DroneController droneController;

        // State
        private float nextFireTime = 0f;
        private int activeCapsulesCount = 0;
        private Vector2 aimDirection = Vector2.up;

        // Public Interface for Trajectory Preview & HUD
        public Vector2 AimDirection => aimDirection;
        public float LaunchSpeed => launchSpeed;
        public Vector2 CalculatedLaunchVelocity
        {
            get
            {
                Vector2 vel = aimDirection * launchSpeed;
                if (inheritDroneVelocity && droneController != null)
                {
                    vel += droneController.Velocity;
                }
                return vel;
            }
        }
        public Vector3 LaunchPosition => GetLaunchPosition();
        public bool CanFire => Time.time >= nextFireTime && activeCapsulesCount < maxSimultaneousCapsules;
        public int ActiveCapsulesCount => activeCapsulesCount;
        public int MaxSimultaneousCapsules => maxSimultaneousCapsules;

        private void Awake()
        {
            mainCamera = Camera.main;
            droneController = GetComponent<DroneController>();
        }

        private void Update()
        {
            UpdateAimDirection();
            HandleFireInput();
        }

        /// <summary>
        /// Calculates normalized aim vector pointing from the launch point to the mouse cursor.
        /// </summary>
        private void UpdateAimDirection()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null) return;
            }

            Vector3 mouseScreenPos = Vector3.zero;

            if (Mouse.current != null)
            {
                mouseScreenPos = Mouse.current.position.ReadValue();
            }

            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
            mouseWorldPos.z = 0f;

            Vector3 origin = transform.position;
            Vector2 dir = (mouseWorldPos - origin);

            if (dir.sqrMagnitude > 0.001f)
            {
                aimDirection = dir.normalized;
            }
        }

        /// <summary>
        /// Checks for fire input and rate limits firing.
        /// </summary>
        private void HandleFireInput()
        {
            bool fireRequested = false;

            // Check Keyboard key (Space by default)
            if (Keyboard.current != null)
            {
                if (fireKey == KeyCode.Space && Keyboard.current.spaceKey.wasPressedThisFrame) fireRequested = true;
                if (fireKey == KeyCode.E && Keyboard.current.eKey.wasPressedThisFrame) fireRequested = true;
            }
            if (allowMouseClickFire && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                fireRequested = true;
            }

            if (fireRequested)
            {
                TryFireCapsule();
            }
        }

        /// <summary>
        /// Validates firing constraints and spawns the capsule.
        /// </summary>
        public bool TryFireCapsule()
        {
            if (!CanFire)
            {
                return false;
            }

            FireCapsule();
            return true;
        }

        private void FireCapsule()
        {
            nextFireTime = Time.time + fireCooldown;
            Vector3 spawnPos = GetLaunchPosition();

            GameObject capsuleObj;

            if (capsulePrefab != null)
            {
                capsuleObj = Instantiate(capsulePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                // Fallback: Dynamically generate an EnergyCapsule GameObject if prefab is unassigned
                capsuleObj = new GameObject("EnergyCapsule_Instance");
                capsuleObj.transform.position = spawnPos;
                capsuleObj.AddComponent<EnergyCapsule>();
            }

            EnergyCapsule capsuleComponent = capsuleObj.GetComponent<EnergyCapsule>();
            if (capsuleComponent == null)
            {
                capsuleComponent = capsuleObj.AddComponent<EnergyCapsule>();
            }

            // Launch with calculated velocity
            capsuleComponent.Launch(CalculatedLaunchVelocity);

            // Track active count & hook destruction cleanup
            activeCapsulesCount++;
            capsuleComponent.OnDestroyed += HandleCapsuleDestroyed;
        }

        private void HandleCapsuleDestroyed(EnergyCapsule capsule)
        {
            capsule.OnDestroyed -= HandleCapsuleDestroyed;
            activeCapsulesCount = Mathf.Max(0, activeCapsulesCount - 1);
        }

        private Vector3 GetLaunchPosition()
        {
            if (launchPoint != null)
            {
                return launchPoint.position;
            }
            return transform.position + (Vector3)(aimDirection * launchOffset);
        }

        private void OnDrawGizmos()
        {
            if (!drawAimGizmo) return;

            Vector3 origin = Application.isPlaying ? GetLaunchPosition() : transform.position;
            Vector3 end = origin + (Vector3)(aimDirection * 2.0f);

            // Aim direction line (Orange/Gold)
            Gizmos.color = new Color(1f, 0.75f, 0.1f, 0.9f);
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, 0.1f);
        }
    }
}
