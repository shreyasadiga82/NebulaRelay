using UnityEngine;

namespace NebulaRelay.World
{
    /// <summary>
    /// CosmicDustParticles creates a continuous live ambient starfield / space dust effect
    /// attached to the camera, giving the universe constant movement, depth and life.
    /// </summary>
    public class CosmicDustParticles : MonoBehaviour
    {
        [Header("Particle Properties")]
        [SerializeField] private int maxParticles = 120;
        [SerializeField] private float emissionRate = 25f;
        [SerializeField] private float minSize = 0.04f;
        [SerializeField] private float maxSize = 0.14f;
        [SerializeField] private float minLifetime = 4f;
        [SerializeField] private float maxLifetime = 8f;
        [SerializeField] private Vector2 driftSpeed = new Vector2(-0.8f, -0.3f);

        private ParticleSystem ps;

        private void Awake()
        {
            SetupDustSystem();
        }

        private void SetupDustSystem()
        {
            ps = GetComponent<ParticleSystem>();
            if (ps == null)
            {
                ps = gameObject.AddComponent<ParticleSystem>();
            }

            var main = ps.main;
            main.loop = true;
            main.maxParticles = maxParticles;
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.4f, 1.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(minSize, maxSize);
            main.startLifetime = new ParticleSystem.MinMaxCurve(minLifetime, maxLifetime);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Shape;

            // Neon cosmic palette (cyan, soft violet, bright star white)
            var grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(new Color(0.1f, 0.9f, 1f), 0f),      // Cyan
                    new GradientColorKey(new Color(1f, 1f, 1f), 0.5f),        // White star
                    new GradientColorKey(new Color(0.85f, 0.4f, 1f), 1f)      // Violet
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.85f, 0.3f),
                    new GradientAlphaKey(0.85f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(grad);

            var emission = ps.emission;
            emission.enabled = true;
            emission.rateOverTime = emissionRate;

            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(32f, 20f, 0.1f);
            shape.position = new Vector3(8f, 4f, 0f);

            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.x = new ParticleSystem.MinMaxCurve(driftSpeed.x - 0.3f, driftSpeed.x + 0.3f);
            vel.y = new ParticleSystem.MinMaxCurve(driftSpeed.y - 0.2f, driftSpeed.y + 0.2f);
            vel.z = new ParticleSystem.MinMaxCurve(0f, 0f);

            var renderer = GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = -20; // In front of background, behind gameplay
            }
        }
    }
}
