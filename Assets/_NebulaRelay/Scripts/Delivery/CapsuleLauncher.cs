using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using NebulaRelay.Player;
using NebulaRelay.Core;
using NebulaRelay.Effects;
using NebulaRelay.World;

namespace NebulaRelay.Delivery
{
    /// <summary>
    /// CapsuleLauncher handles mouse aiming, trajectory calculation, and capsule firing.
    /// Strictly limits the player to exactly 2 capsules per mission.
    /// Tracks remaining capsules authoritatively and triggers mission failure if all capsules are exhausted
    /// without powering the objective receiver.
    /// </summary>
    [SelectionBase]
    public class CapsuleLauncher : MonoBehaviour
    {
        public const int MaxCapsulesPerMission = 2;

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

        [Header("Capsule Mission Allowance")]
        [SerializeField] private int remainingCapsules = MaxCapsulesPerMission;
        [SerializeField] private int capsulesUsed = 0;

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
        private Coroutine outOfCapsulesCheckRoutine;

        // Public Interface for Trajectory Preview, LevelManager & HUD
        public Vector2 AimDirection => aimDirection;
        public float LaunchSpeed => launchSpeed;
        public int RemainingCapsules => remainingCapsules;
        public int CapsulesUsed => capsulesUsed;
        public int MaxCapsules => SettingsManager.GetCapsuleLimit(SettingsManager.CurrentDifficulty, MaxCapsulesPerMission);

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
        public bool CanFire => remainingCapsules > 0 
                            && activeCapsulesCount < maxSimultaneousCapsules 
                            && Time.time >= nextFireTime 
                            && Time.timeScale > 0.001f
                            && (LevelManager.Instance == null || LevelManager.Instance.State == LevelState.InFlight);

        public int ActiveCapsulesCount => activeCapsulesCount;
        public int MaxSimultaneousCapsules => maxSimultaneousCapsules;

        public event Action<int, int> OnCapsuleCountChanged; // (remaining, max)

        private void Awake()
        {
            mainCamera = Camera.main;
            droneController = GetComponent<DroneController>();
            ResetCapsules();
        }

        public void ResetCapsules()
        {
            int limit = SettingsManager.GetCapsuleLimit(SettingsManager.CurrentDifficulty, MaxCapsulesPerMission);
            remainingCapsules = limit;
            capsulesUsed = 0;
            activeCapsulesCount = 0;
            nextFireTime = 0f;
            if (outOfCapsulesCheckRoutine != null)
            {
                StopCoroutine(outOfCapsulesCheckRoutine);
                outOfCapsulesCheckRoutine = null;
            }
            OnCapsuleCountChanged?.Invoke(remainingCapsules, limit);
        }

        private void Update()
        {
            // Block input while paused or game has ended
            if (Time.timeScale <= 0.001f) return;
            if (LevelManager.Instance != null && LevelManager.Instance.State != LevelState.InFlight) return;

            UpdateAimDirection();
            HandleFireInput();
        }

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

        private void HandleFireInput()
        {
            bool fireRequested = false;

            if (Keyboard.current != null)
            {
                if (fireKey == KeyCode.Space && Keyboard.current.spaceKey.wasPressedThisFrame) fireRequested = true;
                if (fireKey == KeyCode.E && Keyboard.current.eKey.wasPressedThisFrame) fireRequested = true;
            }
            if (allowMouseClickFire && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (!NebulaRelay.UI.HUDController.IsPointerOverUI(Mouse.current.position.ReadValue()))
                {
                    fireRequested = true;
                }
            }

            if (fireRequested)
            {
                TryFireCapsule();
            }
        }

        public bool TryFireCapsule()
        {
            if (!CanFire)
            {
                if (remainingCapsules <= 0)
                {
                    AudioManager.Instance?.PlayLaunchDeniedSound();
                    LevelManager.Instance?.SetHUDMessage("NO CAPSULES REMAINING!", 1.5f, true);
                }
                return false;
            }

            FireCapsule();
            return true;
        }

        private void FireCapsule()
        {
            // Strictly consume 1 capsule
            remainingCapsules = Mathf.Max(0, remainingCapsules - 1);
            capsulesUsed++;
            nextFireTime = Time.time + fireCooldown;

            int limit = SettingsManager.GetCapsuleLimit(SettingsManager.CurrentDifficulty, MaxCapsulesPerMission);
            OnCapsuleCountChanged?.Invoke(remainingCapsules, limit);
            AudioManager.Instance?.PlayLaunchSound();
            AudioManager.Instance?.SetCapsuleInFlight(true);

            Vector3 spawnPos = GetLaunchPosition();
            GameObject capsuleObj;

            if (capsulePrefab != null)
            {
                capsuleObj = Instantiate(capsulePrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                capsuleObj = new GameObject("EnergyCapsule_Instance");
                capsuleObj.transform.position = spawnPos;
                capsuleObj.AddComponent<EnergyCapsule>();
            }

            EnergyCapsule capsuleComponent = capsuleObj.GetComponent<EnergyCapsule>();
            if (capsuleComponent == null)
            {
                capsuleComponent = capsuleObj.AddComponent<EnergyCapsule>();
            }

            capsuleComponent.Launch(CalculatedLaunchVelocity);

            activeCapsulesCount++;
            capsuleComponent.OnDestroyed += HandleCapsuleDestroyed;
        }

        private void HandleCapsuleDestroyed(EnergyCapsule capsule)
        {
            capsule.OnDestroyed -= HandleCapsuleDestroyed;
            activeCapsulesCount = Mathf.Max(0, activeCapsulesCount - 1);

            if (activeCapsulesCount == 0)
            {
                AudioManager.Instance?.SetCapsuleInFlight(false);
            }

            // Check if player has exhausted all capsules with no viable way to complete objective
            if (remainingCapsules <= 0 && activeCapsulesCount == 0)
            {
                if (outOfCapsulesCheckRoutine != null) StopCoroutine(outOfCapsulesCheckRoutine);
                outOfCapsulesCheckRoutine = StartCoroutine(CheckOutOfCapsulesFailureRoutine());
            }
        }

        private IEnumerator CheckOutOfCapsulesFailureRoutine()
        {
            yield return new WaitForSeconds(1.5f);

            // If receiver still not delivered and level in flight, trigger mission failure
            RelayStation relay = LevelManager.Instance != null ? LevelManager.Instance.Relay : FindFirstObjectByType<RelayStation>();
            if (relay != null && relay.State == RelayState.Inactive)
            {
                if (LevelManager.Instance != null && LevelManager.Instance.State == LevelState.InFlight)
                {
                    LevelManager.Instance.OnMissionFailed("NO CAPSULES REMAINING");
                }
            }
            outOfCapsulesCheckRoutine = null;
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

            Gizmos.color = new Color(1f, 0.75f, 0.1f, 0.9f);
            Gizmos.DrawLine(origin, end);
            Gizmos.DrawWireSphere(end, 0.1f);
        }
    }
}
