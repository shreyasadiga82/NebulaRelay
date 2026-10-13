using UnityEngine;

namespace NebulaRelay.Delivery
{
    /// <summary>
    /// Core projectile mathematics used identically by the trajectory preview and real capsule.
    /// Follows Newtonian equations: p(t) = p0 + v0*t + 0.5*a*t^2.
    /// </summary>
    public static class ProjectileMath
    {
        /// <summary>
        /// Calculates projectile position at time t given initial position, velocity, and acceleration.
        /// </summary>
        public static Vector2 Position(Vector2 p0, Vector2 v0, Vector2 acceleration, float time)
        {
            return p0 + (v0 * time) + (0.5f * acceleration * time * time);
        }

        /// <summary>
        /// Calculates projectile velocity at time t given initial velocity and acceleration.
        /// </summary>
        public static Vector2 Velocity(Vector2 v0, Vector2 acceleration, float time)
        {
            return v0 + (acceleration * time);
        }
    }

    /// <summary>
    /// TrajectoryPreview renders the predicted flight path of an energy capsule using a LineRenderer.
    /// Shares the identical origin, velocity, and zero-gravity acceleration as the real capsule.
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    [SelectionBase]
    public class TrajectoryPreview : MonoBehaviour
    {
        [Header("Launcher Source")]
        [Tooltip("Reference to the CapsuleLauncher. If unassigned, finds one on this GameObject or in parent.")]
        [SerializeField] private CapsuleLauncher launcher;

        [Header("Rendering")]
        [SerializeField] private Material trajectoryMaterial;

        [Header("Preview Sampling (20–40 points)")]
        [Tooltip("Number of positions sampled along the predicted trajectory (between 20 and 40).")]
        [Range(20, 40)]
        [SerializeField] private int samplePoints = 30;

        [Tooltip("Total simulated flight duration in seconds represented by the preview line.")]
        [SerializeField] private float previewDuration = 1.5f;

        [Header("Line Styling")]
        [Tooltip("Width of the trajectory line at the launch point.")]
        [SerializeField] private float startWidth = 0.08f;

        [Tooltip("Width of the trajectory line at the farthest preview point.")]
        [SerializeField] private float endWidth = 0.02f;

        [Tooltip("Neon color at launch origin.")]
        [SerializeField] private Color startColor = new Color(0.1f, 0.9f, 1.0f, 0.9f);

        [Tooltip("Faded color at preview tip.")]
        [SerializeField] private Color endColor = new Color(0.1f, 0.9f, 1.0f, 0.15f);

        [Header("Display Settings")]
        [Tooltip("If checked, hides preview line while launcher is cooling down or at capsule limit.")]
        [SerializeField] private bool onlyShowWhenCanFire = false;

        // Components
        private LineRenderer lineRenderer;

        // Cached positions buffer to avoid GC allocations
        private Vector3[] positionsBuffer;

        private void Awake()
        {
            lineRenderer = GetComponent<LineRenderer>();

            if (launcher == null)
            {
                launcher = GetComponent<CapsuleLauncher>();
                if (launcher == null)
                {
                    launcher = GetComponentInParent<CapsuleLauncher>();
                }
            }

            ConfigureLineRenderer();
            AllocateBuffer();
        }

        private void OnValidate()
        {
            // Keep buffer in sync when sample count is adjusted in Inspector
            if (lineRenderer != null)
            {
                ConfigureLineRenderer();
                AllocateBuffer();
            }
        }

        private void AllocateBuffer()
        {
            positionsBuffer = new Vector3[samplePoints];
        }

        private void ConfigureLineRenderer()
        {
            if (lineRenderer == null) lineRenderer = GetComponent<LineRenderer>();

            lineRenderer.useWorldSpace = true;
            lineRenderer.startWidth = startWidth;
            lineRenderer.endWidth = endWidth;
            lineRenderer.positionCount = samplePoints;
            lineRenderer.sortingOrder = 4; // Renders under drone & capsule, above background

            // Smooth gradient fading towards trajectory tip
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(startColor, 0.0f),
                    new GradientColorKey(endColor, 1.0f)
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(startColor.a, 0.0f),
                    new GradientAlphaKey(endColor.a, 1.0f)
                }
            );
            lineRenderer.colorGradient = gradient;

            // Ensure valid unlit 2D material in URP without crashing if shader is stripped in standalone build
            if (trajectoryMaterial != null)
            {
                lineRenderer.material = trajectoryMaterial;
            }
            else if (lineRenderer.sharedMaterial == null || lineRenderer.sharedMaterial.name == "Default-Line")
            {
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                             ?? Shader.Find("Sprites/Default");
                if (shader != null)
                {
                    lineRenderer.material = new Material(shader);
                }
            }
        }

        private void LateUpdate()
        {
            UpdateTrajectory();
        }

        /// <summary>
        /// Recalculates all sampled points along the predicted flight path.
        /// </summary>
        private void UpdateTrajectory()
        {
            if (launcher == null) return;

            // Visibility check
            if (onlyShowWhenCanFire && !launcher.CanFire)
            {
                if (lineRenderer.enabled) lineRenderer.enabled = false;
                return;
            }

            if (!lineRenderer.enabled)
            {
                lineRenderer.enabled = true;
            }

            // Shared physics assumptions matching real capsule launch
            Vector2 p0 = launcher.LaunchPosition;
            Vector2 v0 = launcher.CalculatedLaunchVelocity;
            Vector2 acceleration = Vector2.zero; // Zero gravity space vacuum

            float timeStep = previewDuration / (samplePoints - 1);

            if (positionsBuffer == null || positionsBuffer.Length != samplePoints)
            {
                AllocateBuffer();
                lineRenderer.positionCount = samplePoints;
            }

            for (int i = 0; i < samplePoints; i++)
            {
                float t = i * timeStep;
                Vector2 pos = ProjectileMath.Position(p0, v0, acceleration, t);
                positionsBuffer[i] = new Vector3(pos.x, pos.y, 0f);
            }

            lineRenderer.SetPositions(positionsBuffer);
        }

        /// <summary>
        /// Programmatically sets sample point count within the 20–40 range.
        /// </summary>
        public void SetSamplePoints(int count)
        {
            samplePoints = Mathf.Clamp(count, 20, 40);
            AllocateBuffer();
            lineRenderer.positionCount = samplePoints;
        }
    }
}
