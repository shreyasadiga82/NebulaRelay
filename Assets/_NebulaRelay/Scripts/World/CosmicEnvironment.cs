using UnityEngine;

namespace NebulaRelay.World
{
    /// <summary>
    /// CosmicEnvironment configures the multi-layer cosmic space scene for each level.
    /// Spawns the nebula backdrop, distant celestial bodies, floating rotating decorative asteroids,
    /// and configures the live ParallaxBackground controller.
    /// </summary>
    public class CosmicEnvironment : MonoBehaviour
    {
        [Header("Sector Configuration")]
        [Range(1, 5)] public int sectorIndex = 1;

        [Header("Sprites")]
        [SerializeField] private Sprite nebulaSprite;
        [SerializeField] private Sprite planetSprite;
        [SerializeField] private Sprite asteroidSprite;

        [Header("Visual Tuning")]
        [SerializeField] private Color spaceAmbientColor = new Color(0.02f, 0.035f, 0.07f, 1f);

        private void Awake()
        {
            SetupEnvironment();
        }

        public void SetupEnvironment()
        {
            // Camera background color to deep space navy black (#050816) without failing if Camera.main is null at edit-time
            Camera cam = Camera.main;
            if (cam == null) cam = FindFirstObjectByType<Camera>();
            if (cam != null)
            {
                cam.backgroundColor = new Color(0.02f, 0.031f, 0.086f, 1f); // #050816
            }

            // Ensure Parallax Background component exists
            ParallaxBackground parallax = GetComponent<ParallaxBackground>();
            if (parallax == null)
            {
                parallax = gameObject.AddComponent<ParallaxBackground>();
            }

            // 1. Nebula Layer (Layer 1, SortingOrder -80) - Live moving cosmic cloud
            if (nebulaSprite != null)
            {
                GameObject nebulaObj = new GameObject("Layer_Nebula");
                nebulaObj.transform.SetParent(transform, false);
                nebulaObj.transform.position = new Vector3(0f, 0f, 0f);

                SpriteRenderer sr = nebulaObj.AddComponent<SpriteRenderer>();
                sr.sprite = nebulaSprite;
                sr.sortingOrder = -80;
                sr.color = new Color(1f, 1f, 1f, 0.95f);

                // Scale to comfortably cover camera view frustum across wide flight corridors (6.0 x 5.0 = 61.4 x 51.2 units)
                nebulaObj.transform.localScale = new Vector3(6.0f, 5.0f, 1f);

                // Drift continuously at (-0.10, -0.04) u/s, anchored to camera
                parallax.AddLayer(nebulaObj.transform, 0.04f, new Vector2(-0.10f, -0.04f), true, "Nebula");
            }

            // 2. Distant Celestial Body Layer (Layer 2, SortingOrder -60)
            if (planetSprite != null && (sectorIndex == 1 || sectorIndex == 4))
            {
                GameObject planetObj = new GameObject("Layer_Planet_Moon");
                planetObj.transform.SetParent(transform, false);
                Vector3 planetPos = (sectorIndex == 1) ? new Vector3(-14f, -4f, 0f) : new Vector3(18f, 6f, 0f);
                planetObj.transform.position = planetPos;
                // Calibrated planet scale ~0.45u to sit distinctly in the background
                planetObj.transform.localScale = new Vector3(0.45f, 0.45f, 1f);

                SpriteRenderer sr = planetObj.AddComponent<SpriteRenderer>();
                sr.sprite = planetSprite;
                sr.sortingOrder = -60;
                sr.color = (sectorIndex == 1) ? new Color(0.75f, 0.85f, 0.95f, 0.75f) : new Color(0.9f, 0.75f, 0.45f, 0.75f);

                // Slow axial rotation
                SlowSpinner spinner = planetObj.AddComponent<SlowSpinner>();
                spinner.spinSpeed = 2f;

                parallax.AddLayer(planetObj.transform, 0.07f, new Vector2(0.03f, 0.01f), false, "Planet");
            }

            // 3. Decorative Live Background Asteroids (Layer 3, SortingOrder -40)
            if (asteroidSprite != null)
            {
                GameObject clusterObj = new GameObject("Layer_DistantAsteroids");
                clusterObj.transform.SetParent(transform, false);

                // Seed Random state deterministically for repeatable scene generation
                Random.InitState(1000 + sectorIndex * 37);

                int count = 12;
                for (int i = 0; i < count; i++)
                {
                    GameObject ast = new GameObject($"BgAsteroid_{i}");
                    ast.transform.SetParent(clusterObj.transform, false);

                    float x = Random.Range(-35f, 35f);
                    float y = Random.Range(-12f, 12f);
                    ast.transform.localPosition = new Vector3(x, y, 0f);

                    // Calibrated background asteroid scale 0.05 - 0.12 (clearly smaller than gameplay 1.0-1.8u)
                    float s = Random.Range(0.05f, 0.12f);
                    ast.transform.localScale = new Vector3(s, s, 1f);

                    SpriteRenderer sr = ast.AddComponent<SpriteRenderer>();
                    sr.sprite = asteroidSprite;
                    sr.sortingOrder = -40;
                    // Dim, blue-shifted background shading
                    sr.color = new Color(0.25f, 0.32f, 0.48f, 0.45f);

                    // Continuous rotation for live feeling
                    SlowSpinner spinner = ast.AddComponent<SlowSpinner>();
                    spinner.spinSpeed = Random.Range(-12f, 12f);
                }

                // Continuous drifting asteroid field
                parallax.AddLayer(clusterObj.transform, 0.14f, new Vector2(-0.06f, 0.02f), false, "AsteroidBelt");
            }
        }
    }

    /// <summary>
    /// Helper component for slow continuous rotation of background celestial bodies.
    /// </summary>
    public class SlowSpinner : MonoBehaviour
    {
        public float spinSpeed = 5f;

        private void Update()
        {
            transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }
    }
}
