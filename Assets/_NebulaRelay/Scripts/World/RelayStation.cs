using System;
using System.Collections;
using UnityEngine;
using NebulaRelay.Player;
using NebulaRelay.Core;

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
    /// RelayStation represents an orbital energy relay station.
    /// Progresses through 4 states: Inactive -> Energy Delivered -> Ready -> Activated.
    /// Requires an EnergyCapsule hit first, then safe drone docking speed (<= 1.5 units/s).
    /// </summary>
    [SelectionBase]
    public class RelayStation : MonoBehaviour
    {
        [Header("Relay Identity")]
        [Tooltip("The relay identifier (e.g. 1 for Relay 01).")]
        [SerializeField] private int relayIndex = 1;

        [Header("Docking Requirements")]
        [Tooltip("Maximum allowed drone speed to safely initiate activation (Default: 1.5).")]
        [SerializeField] private float maxSafeSpeed = 1.5f;

        [Tooltip("Radius of the drone activation zone in world units.")]
        [SerializeField] private float activationZoneRadius = 2.2f;

        [Header("Receiver Link")]
        [Tooltip("The EnergyReceiver linked to this relay. If unassigned, finds one in children or nearby.")]
        [SerializeField] private EnergyReceiver receiver;

        [Header("Visual Elements")]
        [SerializeField] private SpriteRenderer stationRenderer;
        [SerializeField] private SpriteRenderer beaconRenderer;
        [SerializeField] private Color inactiveColor = new Color(0.2f, 0.3f, 0.45f, 1f); // Dark industrial blue
        [SerializeField] private Color deliveredColor = new Color(1f, 0.75f, 0.1f, 1f); // Amber / Power charging
        [SerializeField] private Color activatedColor = new Color(0f, 0.9f, 1f, 1f); // Restored neon cyan/green

        // State
        public int RelayIndex => relayIndex;
        public RelayState State { get; private set; } = RelayState.Inactive;
        public float MaxSafeSpeed => maxSafeSpeed;
        public bool IsActivated => State == RelayState.Activated;

        // Feedback message
        private string speedWarningMessage = "";
        private float warningExpireTime = 0f;

        // Events
        public event Action<RelayStation> OnRelayActivated;

        private CircleCollider2D activationTrigger;

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
            // Ensure a trigger collider dedicated to the drone activation zone exists
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
            if (State != RelayState.Inactive) return;

            State = RelayState.ReadyForDroneActivation;
            UpdateVisuals();

            // Set global banner via LevelManager if available
            LevelManager.Instance?.SetHUDMessage("ENERGY DELIVERED! Approach relay below 1.5 speed to activate.", 3.5f, false);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (State != RelayState.ReadyForDroneActivation) return;

            // Check if player drone is inside activation zone
            DroneController drone = other.GetComponent<DroneController>();
            if (drone == null)
            {
                drone = other.GetComponentInParent<DroneController>();
            }

            if (drone != null || other.CompareTag("Player"))
            {
                float currentSpeed = drone != null ? drone.Speed : 0f;

                if (currentSpeed <= maxSafeSpeed)
                {
                    // Successful activation!
                    ActivateRelay();
                }
                else
                {
                    // Moving too fast: show warning
                    ShowSpeedWarning(currentSpeed);
                }
            }
        }

        private void ShowSpeedWarning(float currentSpeed)
        {
            speedWarningMessage = $"SLOW DOWN! Speed: {currentSpeed:F1} / Safe: ≤ {maxSafeSpeed:F1}";
            warningExpireTime = Time.time + 1.5f;

            LevelManager.Instance?.SetHUDMessage(speedWarningMessage, 1.5f, true);
        }

        private void ActivateRelay()
        {
            if (State == RelayState.Activated) return;

            State = RelayState.Activated;
            UpdateVisuals();

            LevelManager.Instance?.SetHUDMessage($"RELAY {relayIndex} RESTORED AND ONLINE!", 4.0f, false);
            LevelManager.Instance?.OnRelayActivated(this);
            OnRelayActivated?.Invoke(this);

            StartCoroutine(ActivationCelebrationRoutine());
        }

        private IEnumerator ActivationCelebrationRoutine()
        {
            Vector3 originalScale = transform.localScale;
            for (float t = 0; t < 0.6f; t += Time.deltaTime)
            {
                float scaleMod = 1f + Mathf.Sin(t * Mathf.PI * 4f) * 0.15f;
                transform.localScale = originalScale * scaleMod;
                yield return null;
            }
            transform.localScale = originalScale;
        }

        private void UpdateVisuals()
        {
            Color targetColor = inactiveColor;
            switch (State)
            {
                case RelayState.Inactive:
                    targetColor = inactiveColor;
                    break;
                case RelayState.EnergyDelivered:
                case RelayState.ReadyForDroneActivation:
                    targetColor = deliveredColor;
                    break;
                case RelayState.Activated:
                    targetColor = activatedColor;
                    break;
            }

            if (stationRenderer != null) stationRenderer.color = targetColor;
            if (beaconRenderer != null)
            {
                beaconRenderer.color = State == RelayState.Activated ? activatedColor : targetColor;
                beaconRenderer.enabled = State != RelayState.Inactive;
            }
        }

        private void EnsureVisibleVisuals()
        {
            if (stationRenderer == null)
            {
                stationRenderer = GetComponent<SpriteRenderer>();
                if (stationRenderer == null) stationRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (stationRenderer.sprite == null)
            {
                stationRenderer.sprite = CreateProceduralRelaySprite();
            }

            stationRenderer.sortingOrder = 1;
        }

        private static Sprite CreateProceduralRelaySprite()
        {
            int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);
            Color body = new Color(0.2f, 0.35f, 0.5f, 1f);
            Color solar = new Color(0.05f, 0.6f, 0.8f, 1f);
            Color core = Color.white;

            // Clear
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                    tex.SetPixel(x, y, transparent);

            // Satellite body & solar wings
            for (int y = 30; y < 66; y++)
            {
                for (int x = 12; x < 84; x++)
                {
                    if (x < 36 || x > 60)
                        tex.SetPixel(x, y, solar);
                    else
                        tex.SetPixel(x, y, body);
                }
            }

            // Central beacon orb
            Vector2 center = new Vector2(48, 48);
            for (int y = 38; y < 58; y++)
                for (int x = 38; x < 58; x++)
                    if (Vector2.Distance(new Vector2(x, y), center) <= 8f)
                        tex.SetPixel(x, y, core);

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }

        private void OnDrawGizmos()
        {
            // Draw activation zone in Scene view
            Gizmos.color = State == RelayState.Activated ? Color.cyan : (State == RelayState.ReadyForDroneActivation ? Color.yellow : Color.gray);
            Gizmos.DrawWireSphere(transform.position, activationZoneRadius);
        }
    }
}
