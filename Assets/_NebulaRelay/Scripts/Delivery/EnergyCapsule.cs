using System;
using UnityEngine;

namespace NebulaRelay.Delivery
{
    /// <summary>
    /// EnergyCapsule represents the delivered projectile in Nebula Relay.
    /// Operates under zero-gravity physics and triggers delivery or hazard interactions.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(CircleCollider2D))]
    [SelectionBase]
    public class EnergyCapsule : MonoBehaviour
    {
        [Header("Capsule Settings")]
        [Tooltip("Lifetime in seconds before an undelivered capsule expires and cleans up.")]
        [SerializeField] private float lifetime = 6.0f;

        [Tooltip("Radius of the collision sphere in world units.")]
        [SerializeField] private float colliderRadius = 0.25f;

        // Components
        private Rigidbody2D rb;
        private CircleCollider2D col;
        private SpriteRenderer spriteRenderer;

        // State
        private float spawnTime;
        public bool IsDelivered { get; private set; }
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;

        // Events
        public event Action<EnergyCapsule> OnDestroyed;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<CircleCollider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            ConfigurePhysics();
            EnsureVisibleSprite();
        }

        private void Start()
        {
            spawnTime = Time.time;
        }

        private void Update()
        {
            // Auto cleanup expired capsules that drifted out of bounds
            if (!IsDelivered && Time.time - spawnTime >= lifetime)
            {
                Expire();
            }
        }

        private void ConfigurePhysics()
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f; // Zero-gravity orbital flight
            rb.linearDamping = 0f; // Constant velocity coast in vacuum
            rb.angularDamping = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

            col.isTrigger = true; // Use triggers for ring & receiver detection
            col.radius = colliderRadius;
        }

        /// <summary>
        /// Launches the capsule with an initial linear velocity vector.
        /// </summary>
        public void Launch(Vector2 initialVelocity)
        {
            if (rb == null) rb = GetComponent<Rigidbody2D>();
            rb.linearVelocity = initialVelocity;

            // Orient sprite towards travel direction if not circular
            if (initialVelocity.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(initialVelocity.y, initialVelocity.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>
        /// Called when the capsule successfully strikes a relay receiver.
        /// </summary>
        public void MarkDelivered()
        {
            if (IsDelivered) return;
            IsDelivered = true;
            rb.linearVelocity = Vector2.zero;
            DestroyCapsule();
        }

        /// <summary>
        /// Cleans up the capsule when expired or destroyed by a hazard.
        /// </summary>
        public void Expire()
        {
            DestroyCapsule();
        }

        private void DestroyCapsule()
        {
            OnDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        private void EnsureVisibleSprite()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateProceduralCapsuleSprite();
            }

            spriteRenderer.sortingOrder = 6;
        }

        /// <summary>
        /// Generates a procedural glowing cyan energy orb sprite if no asset is assigned.
        /// </summary>
        private static Sprite CreateProceduralCapsuleSprite()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);
            Color cyanGlow = new Color(0.1f, 0.85f, 1f, 0.8f);
            Color whiteCore = new Color(1f, 1f, 1f, 1f);

            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    if (dist > radius)
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                    else
                    {
                        float t = dist / radius;
                        Color col = Color.Lerp(whiteCore, cyanGlow, t);
                        col.a = Mathf.Clamp01(1f - (t * 0.3f));
                        tex.SetPixel(x, y, col);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
