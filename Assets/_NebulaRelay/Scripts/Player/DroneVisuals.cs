using UnityEngine;

namespace NebulaRelay.Player
{
    /// <summary>
    /// DroneVisuals enhances the Player Drone with sci-fi engine exhaust effects,
    /// dynamic thruster glow on acceleration, and sleek visual orientation.
    /// </summary>
    [RequireComponent(typeof(DroneController))]
    public class DroneVisuals : MonoBehaviour
    {
        [Header("Exhaust Effects")]
        [SerializeField] private TrailRenderer thrusterTrail;
        [SerializeField] private SpriteRenderer thrusterGlow;

        private DroneController drone;
        private Rigidbody2D rb;
        private float glowIntensity = 0f;

        private void Awake()
        {
            drone = GetComponent<DroneController>();
            rb = GetComponent<Rigidbody2D>();
            SetupExhaustTrail();
        }

        private void SetupExhaustTrail()
        {
            if (thrusterTrail == null)
            {
                GameObject trailObj = new GameObject("EngineTrail");
                trailObj.transform.SetParent(transform, false);
                trailObj.transform.localPosition = new Vector3(0f, -0.4f, 0f);

                thrusterTrail = trailObj.AddComponent<TrailRenderer>();
                thrusterTrail.time = 0.25f;
                thrusterTrail.startWidth = 0.2f;
                thrusterTrail.endWidth = 0.0f;

                Gradient grad = new Gradient();
                grad.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(new Color(0f, 0.95f, 1f), 0f), new GradientColorKey(new Color(0.1f, 0.3f, 0.8f), 1f) },
                    new GradientAlphaKey[] { new GradientAlphaKey(0.8f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                thrusterTrail.colorGradient = grad;
                thrusterTrail.sortingOrder = -1;
            }
        }

        private void Update()
        {
            // Detect if player is supplying thrust
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            bool isThrusting = (h != 0f || v != 0f);

            // Interpolate thruster glow
            float targetGlow = isThrusting ? 1.0f : 0.2f;
            glowIntensity = Mathf.MoveTowards(glowIntensity, targetGlow, Time.deltaTime * 5f);

            if (thrusterTrail != null)
            {
                thrusterTrail.emitting = isThrusting || (rb != null && rb.linearVelocity.sqrMagnitude > 4f);
            }

            if (thrusterGlow != null)
            {
                thrusterGlow.color = new Color(0f, 0.95f, 1f, glowIntensity);
            }
        }
    }
}
