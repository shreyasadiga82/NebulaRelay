using UnityEngine;
using NebulaRelay.Player;
using NebulaRelay.Delivery;
using NebulaRelay.Core;

namespace NebulaRelay.World
{
    /// <summary>
    /// AsteroidHazard represents a space hazard obstacle.
    /// Inflicts battery damage on drone collision and destroys intercepted capsules.
    /// Supports optional slow rotation and linear patrol movement.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [SelectionBase]
    public class AsteroidHazard : MonoBehaviour
    {
        [Header("Damage Settings")]
        [Tooltip("Battery percentage drained when the drone impacts this asteroid (Default: 15%).")]
        [SerializeField] private float collisionBatteryDamage = 15.0f;

        [Header("Motion & Dynamics")]
        [Tooltip("Degrees rotated per second around the Z axis.")]
        [SerializeField] private float rotationSpeed = 15.0f;

        [Tooltip("Linear movement patrol offset from initial position (set (0,0) for stationary).")]
        [SerializeField] private Vector2 patrolOffset = Vector2.zero;

        [Tooltip("Patrol oscillation cycle speed.")]
        [SerializeField] private float patrolSpeed = 1.0f;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Vector3 startPosition;
        private Collider2D col;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            startPosition = transform.position;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
                if (spriteRenderer == null) spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            EnsureVisibleSprite();
        }

        private void Update()
        {
            // Passive space tumble
            if (Mathf.Abs(rotationSpeed) > 0.01f)
            {
                transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);
            }

            // Patrol movement
            if (patrolOffset.sqrMagnitude > 0.01f)
            {
                float t = Mathf.Sin(Time.time * patrolSpeed);
                transform.position = startPosition + (Vector3)(patrolOffset * t);
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            HandleHit(collision.gameObject);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            HandleHit(other.gameObject);
        }

        private void HandleHit(GameObject other)
        {
            // 1. Drone collision
            DroneController drone = other.GetComponent<DroneController>();
            if (drone == null) drone = other.GetComponentInParent<DroneController>();

            if (drone != null || other.CompareTag("Player"))
            {
                BatteryManager battery = other.GetComponent<BatteryManager>();
                if (battery == null) battery = other.GetComponentInParent<BatteryManager>();

                if (battery != null)
                {
                    battery.ConsumeBattery(collisionBatteryDamage);
                    LevelManager.Instance?.SetHUDMessage($"COLLISION DAMAGE! -{collisionBatteryDamage:F0}% Battery", 2.0f, true);
                }
            }

            // 2. Capsule collision (intercepted shot)
            EnergyCapsule capsule = other.GetComponent<EnergyCapsule>();
            if (capsule == null) capsule = other.GetComponentInParent<EnergyCapsule>();

            if (capsule != null && !capsule.IsDelivered)
            {
                capsule.Expire();
                LevelManager.Instance?.SetHUDMessage("CAPSULE INTERCEPTED BY ASTEROID!", 2.0f, true);
            }
        }

        private void EnsureVisibleSprite()
        {
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateProceduralAsteroidSprite();
            }
            spriteRenderer.color = new Color(0.7f, 0.7f, 0.75f, 1f);
            spriteRenderer.sortingOrder = 2;
        }

        private static Sprite CreateProceduralAsteroidSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);
            Color rock = new Color(0.35f, 0.35f, 0.4f, 1f);
            Color highlight = new Color(0.5f, 0.5f, 0.55f, 1f);
            Vector2 center = new Vector2(size / 2f, size / 2f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    // Add subtle rocky roughness
                    float angle = Mathf.Atan2(y - center.y, x - center.x);
                    float roughR = 26f + Mathf.Sin(angle * 5f) * 3f + Mathf.Cos(angle * 3f) * 2f;

                    if (d <= roughR)
                    {
                        float t = (x + y) / (float)(size * 2);
                        tex.SetPixel(x, y, Color.Lerp(rock, highlight, t));
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
