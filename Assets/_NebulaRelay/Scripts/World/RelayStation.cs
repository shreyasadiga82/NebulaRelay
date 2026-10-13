using System;
using System.Collections;
using UnityEngine;
using NebulaRelay.Player;
using NebulaRelay.Core;
using NebulaRelay.Effects;

namespace NebulaRelay.World
{
    public enum RelayState
    {
        Inactive,
        EnergyDelivered,
        ReadyForDroneActivation,
        Activated
    }

    /// <summary>
    /// RelayStation represents the mission destination exit goal.
    /// Strictly requires all mission relays to be synchronized AND energy capsule delivered
    /// before safe docking (<= 1.5 m/s) can complete the mission.
    /// </summary>
    [SelectionBase]
    public class RelayStation : MonoBehaviour
    {
        [Header("Relay Identity")]
        [Tooltip("The relay identifier (e.g. 1 for Sector 01).")]
        [SerializeField] private int relayIndex = 1;

        [Header("Docking Requirements")]
        [Tooltip("Maximum allowed drone speed to safely initiate activation (Default: 1.5).")]
        [SerializeField] private float maxSafeSpeed = 1.5f;

        [Tooltip("Radius of the drone activation zone in world units.")]
        [SerializeField] private float activationZoneRadius = 2.2f;

        [Header("Receiver Link")]
        [Tooltip("The EnergyReceiver linked to this relay.")]
        [SerializeField] private EnergyReceiver receiver;

        [Header("Visual Elements")]
        [SerializeField] private SpriteRenderer stationRenderer;
        [SerializeField] private SpriteRenderer beaconRenderer;
        [SerializeField] private Color lockedColor = new Color(0.85f, 0.15f, 0.15f, 1f); // Red locked glow
        [SerializeField] private Color inactiveColor = new Color(0.2f, 0.3f, 0.45f, 1f); // Dark industrial blue
        [SerializeField] private Color deliveredColor = new Color(1f, 0.75f, 0.1f, 1f); // Amber / charging
        [SerializeField] private Color readyUnlockedColor = new Color(0f, 0.95f, 0.5f, 1f); // Unlocked neon green
        [SerializeField] private Color activatedColor = new Color(0f, 0.9f, 1f, 1f); // Restored neon cyan

        // State
        public int RelayIndex => relayIndex;
        public RelayState State { get; private set; } = RelayState.Inactive;
        public float MaxSafeSpeed => maxSafeSpeed;
        public float ActivationZoneRadius => activationZoneRadius;
        public bool IsActivated => State == RelayState.Activated;

        private float lastLockWarningTime = -5f;
        private CircleCollider2D activationTrigger;

        // Events
        public event Action<RelayStation> OnRelayActivated;

        public bool IsDroneInActivationRadius(Vector3 dronePosition)
        {
            return Vector2.Distance(transform.position, dronePosition) <= activationZoneRadius;
        }

        private void Awake()
        {
            SetupActivationTrigger();
            EnsureVisibleVisuals();
        }

        private void Start()
        {
            if (receiver == null)
            {
                receiver = GetComponentInChildren<EnergyReceiver>();
            }

            if (receiver != null)
            {
                receiver.OnDeliverySuccess += HandleEnergyDelivered;
            }

            UpdateVisuals();
        }

        private void OnDestroy()
        {
            if (receiver != null)
            {
                receiver.OnDeliverySuccess -= HandleEnergyDelivered;
            }
        }

        private void SetupActivationTrigger()
        {
            CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();
            foreach (var c in colliders)
            {
                if (c.isTrigger)
                {
                    activationTrigger = c;
                    break;
                }
            }

            if (activationTrigger == null)
            {
                activationTrigger = gameObject.AddComponent<CircleCollider2D>();
                activationTrigger.isTrigger = true;
            }

            activationTrigger.radius = activationZoneRadius;
        }

        private void HandleEnergyDelivered(EnergyReceiver r)
        {
            if (State == RelayState.Activated) return;

            State = RelayState.ReadyForDroneActivation;
            UpdateVisuals();

            bool allRelaysSynced = IsAllRelaysSynchronized();
            if (!allRelaysSynced)
            {
                int remaining = GetRemainingRelaysCount();
                LevelManager.Instance?.SetHUDMessage($"ENERGY STORED! BUT MISSION LOCKED — {remaining} RELAYS REMAINING", 4.0f, true);
            }
            else
            {
                LevelManager.Instance?.SetHUDMessage("EXIT UNLOCKED! Approach dock below 1.5 m/s to complete mission.", 4.0f, false);
            }
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (State == RelayState.Activated) return;

            DroneController drone = other.GetComponent<DroneController>();
            if (drone == null) drone = other.GetComponentInParent<DroneController>();

            if (drone != null || other.CompareTag("Player"))
            {
                // RULE 1: STRICT COMPULSORY RELAY CHECK
                if (!IsAllRelaysSynchronized())
                {
                    int remaining = GetRemainingRelaysCount();
                    if (Time.time - lastLockWarningTime > 2.0f)
                    {
                        lastLockWarningTime = Time.time;
                        LevelManager.Instance?.SetHUDMessage($"MISSION LOCKED — {remaining} RELAYS REMAINING", 2.5f, true);
                        AudioManager.Instance?.PlayExitLockedDenialSound();
                    }
                    return; // EXIT LOCKED! Cannot complete!
                }

                // RULE 2: CAPSULE DELIVERY CHECK
                if (State == RelayState.Inactive)
                {
                    if (Time.time - lastLockWarningTime > 2.0f)
                    {
                        lastLockWarningTime = Time.time;
                        LevelManager.Instance?.SetHUDMessage("EXIT LOCKED — LAUNCH ENERGY CAPSULE INTO RECEIVER DISH!", 2.5f, true);
                        AudioManager.Instance?.PlayExitLockedDenialSound();
                    }
                    return;
                }

                // RULE 3: DOCKING SPEED CHECK (<= 1.5 m/s)
                float currentSpeed = drone != null ? drone.Speed : 0f;
                if (currentSpeed <= maxSafeSpeed)
                {
                    ActivateRelay();
                }
                else
                {
                    ShowSpeedWarning(currentSpeed);
                }
            }
        }

        private bool IsAllRelaysSynchronized()
        {
            if (LevelManager.Instance != null && LevelManager.Instance.Rings != null)
            {
                return LevelManager.Instance.Rings.AllRelaysSynchronized;
            }
            return true;
        }

        private int GetRemainingRelaysCount()
        {
            if (LevelManager.Instance != null && LevelManager.Instance.Rings != null)
            {
                return LevelManager.Instance.Rings.RemainingRelays;
            }
            return 0;
        }

        private void ShowSpeedWarning(float currentSpeed)
        {
            string msg = $"SLOW DOWN! Speed: {currentSpeed:F1} / Safe: ≤ {maxSafeSpeed:F1} m/s";
            LevelManager.Instance?.SetHUDMessage(msg, 1.5f, true);
        }

        private void ActivateRelay()
        {
            if (State == RelayState.Activated) return;

            // Extra safety verification: every relay MUST be synchronized
            if (!IsAllRelaysSynchronized())
            {
                return;
            }

            State = RelayState.Activated;
            UpdateVisuals();

            LevelManager.Instance?.SetHUDMessage($"RELAY {relayIndex} RESTORED AND SYNCHRONIZED!", 4.0f, false);
            LevelManager.Instance?.OnRelayActivated(this);
            OnRelayActivated?.Invoke(this);

            StartCoroutine(ActivationCelebrationRoutine());
        }

        private IEnumerator ActivationCelebrationRoutine()
        {
            Vector3 origScale = transform.localScale;
            float duration = 1.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float pulse = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.1f;
                transform.localScale = origScale * pulse;
                yield return null;
            }

            transform.localScale = origScale;
        }

        private void UpdateVisuals()
        {
            if (stationRenderer != null)
            {
                bool synced = IsAllRelaysSynchronized();
                if (State == RelayState.Activated)
                {
                    stationRenderer.color = activatedColor;
                }
                else if (State == RelayState.ReadyForDroneActivation)
                {
                    stationRenderer.color = synced ? readyUnlockedColor : deliveredColor;
                }
                else
                {
                    stationRenderer.color = inactiveColor;
                }
            }

            if (beaconRenderer != null)
            {
                bool synced = IsAllRelaysSynchronized();
                beaconRenderer.color = (State == RelayState.Activated) ? activatedColor
                    : ((State == RelayState.ReadyForDroneActivation && synced) ? readyUnlockedColor : (synced ? deliveredColor : lockedColor));
            }
        }

        private void EnsureVisibleVisuals()
        {
            if (stationRenderer == null) stationRenderer = GetComponent<SpriteRenderer>();
            if (stationRenderer == null) stationRenderer = gameObject.AddComponent<SpriteRenderer>();
            stationRenderer.sortingOrder = 2;
        }
    }
}
