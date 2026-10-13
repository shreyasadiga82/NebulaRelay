using UnityEngine;

namespace NebulaRelay.World
{
    /// <summary>
    /// CosmicEnvironment configures the multi-layer cosmic space scene for each level.
    /// Spawns the nebula backdrop, distant planets, floating decorative asteroids,
    /// and configures the ParallaxBackground controller.
    /// </summary>
    public class CosmicEnvironment : MonoBehaviour
    {
        [Header("Sector Configuration")]
        [Range(1, 3)] public int sectorIndex = 1;

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
            // Camera background color to deep space navy black (#050816)
            Camera cam = Camera.main;
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

            // 1. Nebula Layer (Layer 2, SortingOrder -80)
            if (nebulaSprite != null)
            {
                GameObject nebulaObj = new GameObject("Layer_Nebula");
                nebulaObj.transform.SetParent(transform, false);
                nebulaObj.transform.position = new Vector3(0f, 0f, 0f);

                SpriteRenderer sr = nebulaObj.AddComponent<SpriteRenderer>();
                sr.sprite = nebulaSprite;
                sr.sortingOrder = -80;
                sr.color = new Color(1f, 1f, 1f, 0.95f);

                // Scale to comfortably cover camera view frustum (ortho size 8-12)
                nebulaObj.transform.localScale = new Vector3(2.5f, 2.5f, 1f);

                parallax.AddLayer(nebulaObj.transform, 0.04f, new Vector2(0.05f, 0.02f), "Nebula");
            }

            // 2. Distant Planet Layer (Layer 3, SortingOrder -60)
            if (planetSprite != null && sectorIndex == 1)
            {
                GameObject planetObj = new GameObject("Layer_Planet_Moon");
                planetObj.transform.SetParent(transform, false);
                planetObj.transform.position = new Vector3(-12f, -5f, 0f); // Bottom-left like reference
                planetObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

                SpriteRenderer sr = planetObj.AddComponent<SpriteRenderer>();
                sr.sprite = planetSprite;
                sr.sortingOrder = -60;
                sr.color = new Color(0.85f, 0.9f, 1f, 0.9f);

                parallax.AddLayer(planetObj.transform, 0.08f, new Vector2(0.01f, 0f), "Planet");
            }

            // 3. Decorative Background Asteroid Cluster (Layer 4, SortingOrder -40)
            if (asteroidSprite != null)
            {
                GameObject clusterObj = new GameObject("Layer_DistantAsteroids");
                clusterObj.transform.SetParent(transform, false);

                // Spawn several decorative non-colliding background asteroids
                int count = sectorIndex == 2 ? 8 : 4;
                for (int i = 0; i < count; i++)
                {
                    GameObject ast = new GameObject($"BgAsteroid_{i}");
                    ast.transform.SetParent(clusterObj.transform, false);

                    float x = Random.Range(-18f, 18f);
                    float y = Random.Range(-10f, 10f);
                    ast.transform.localPosition = new Vector3(x, y, 0f);

                    float s = Random.Range(0.2f, 0.45f);
                    ast.transform.localScale = new Vector3(s, s, 1f);

                    SpriteRenderer sr = ast.AddComponent<SpriteRenderer>();
                    sr.sprite = asteroidSprite;
                    sr.sortingOrder = -40;
                    // Slightly dimmed/tinted so gameplay obstacles pop out clearly
                    sr.color = new Color(0.35f, 0.4f, 0.6f, 0.65f);
                }

                parallax.AddLayer(clusterObj.transform, 0.16f, new Vector2(-0.04f, 0.01f), "AsteroidBelt");
            }
        }
    }
}
