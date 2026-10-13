using UnityEngine;
using NebulaRelay.Effects;

namespace NebulaRelay.World
{
    /// <summary>
    /// RelayStationVisuals enhances the RelayStation with cinematic sci-fi visual effects:
    /// rotating concentric energy rings, pulsing core orb, vertical transmission beam,
    /// and dynamic state-based lighting and particle feedback.
    /// </summary>
    [RequireComponent(typeof(RelayStation))]
    public class RelayStationVisuals : MonoBehaviour
    {
        [Header("Sprite Elements")]
        [SerializeField] private SpriteRenderer towerRenderer;
        [SerializeField] private SpriteRenderer coreRenderer;
        [SerializeField] private SpriteRenderer innerRingRenderer;
        [SerializeField] private SpriteRenderer outerRingRenderer;

        [Header("Beam & Particle Effects")]
        [SerializeField] private LineRenderer transmissionBeam;
        [SerializeField] private ParticleSystem coreParticles;

        [Header("Animation Speeds")]
        [SerializeField] private float innerRingSpeed = 25f;
        [SerializeField] private float outerRingSpeed = -15f;
        [SerializeField] private float pulseFrequency = 2.5f;

        private RelayStation station;
        private RelayState lastState = RelayState.Inactive;
        private bool isBeamActive = false;

        private void Awake()
        {
            station = GetComponent<RelayStation>();
            SetupVisualComponents();
        }

        private void SetupVisualComponents()
        {
            // Auto-configure transmission beam if not present
            if (transmissionBeam == null)
            {
                GameObject beamObj = new GameObject("TransmissionBeam");
                beamObj.transform.SetParent(transform, false);
                beamObj.transform.localPosition = new Vector3(0f, 2.5f, 0f);

                transmissionBeam = beamObj.AddComponent<LineRenderer>();
                transmissionBeam.useWorldSpace = false;
                transmissionBeam.positionCount = 2;
                transmissionBeam.SetPosition(0, Vector3.zero);
                transmissionBeam.SetPosition(1, new Vector3(0f, 30f, 0f)); // shoots high into space
                transmissionBeam.startWidth = 0.35f;
                transmissionBeam.endWidth = 0.05f;

                // Glowing cyan gradient
                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0f, 0.9f, 1f), 0f), new GradientColorKey(new Color(0.2f, 0.5f, 1f), 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.9f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                transmissionBeam.colorGradient = grad;

                transmissionBeam.sortingOrder = -5;
                transmissionBeam.enabled = false;
            }
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Rotate concentric holographic rings
            if (innerRingRenderer != null)
            {
                innerRingRenderer.transform.Rotate(0f, 0f, innerRingSpeed * dt);
            }

            if (outerRingRenderer != null)
            {
                outerRingRenderer.transform.Rotate(0f, 0f, outerRingSpeed * dt);
            }

            // State changes
            if (station != null && station.State != lastState)
            {
                OnStateChanged(station.State);
                lastState = station.State;
            }

            // Core pulsing logic
            UpdateCorePulsing();

            // Beam oscillation when activated
            if (isBeamActive && transmissionBeam != null)
            {
                float beamFlicker = 0.35f + Mathf.PingPong(Time.time * 4f, 0.15f);
                transmissionBeam.startWidth = beamFlicker;
            }
        }

        private void UpdateCorePulsing()
        {
            if (coreRenderer == null) return;

            float pulse = (Mathf.Sin(Time.time * pulseFrequency) + 1f) * 0.5f;

            if (station == null || station.State == RelayState.Inactive)
            {
                coreRenderer.color = Color.Lerp(new Color(0f, 0.5f, 0.8f, 0.6f), new Color(0f, 0.9f, 1f, 0.9f), pulse);
            }
            else if (station.State == RelayState.EnergyDelivered || station.State == RelayState.ReadyForDroneActivation)
            {
                // Amber / Charging power pulse
                coreRenderer.color = Color.Lerp(new Color(1f, 0.5f, 0f, 0.8f), new Color(1f, 0.9f, 0.2f, 1f), pulse);
            }
            else if (station.State == RelayState.Activated)
            {
                // Full electric cyan radiance
                coreRenderer.color = Color.Lerp(new Color(0.1f, 1f, 0.9f, 0.95f), Color.white, pulse * 0.6f);
            }
        }

        private void OnStateChanged(RelayState newState)
        {
            switch (newState)
            {
                case RelayState.EnergyDelivered:
                    innerRingSpeed = 50f;
                    outerRingSpeed = -35f;
                    pulseFrequency = 4f;
                    break;

                case RelayState.Activated:
                    innerRingSpeed = 80f;
                    outerRingSpeed = -60f;
                    pulseFrequency = 6f;
                    isBeamActive = true;
                    if (transmissionBeam != null) transmissionBeam.enabled = true;
                    break;
            }
        }
    }
}
