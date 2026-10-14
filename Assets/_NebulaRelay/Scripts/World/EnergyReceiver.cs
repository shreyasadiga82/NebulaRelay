using System;
using UnityEngine;
using NebulaRelay.Delivery;
using NebulaRelay.Core;

namespace NebulaRelay.World
{
    /// <summary>
    /// EnergyReceiver is a stationary target that receives energy capsules.
    /// Uses a trigger collider and counts successful deliveries exactly once.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [SelectionBase]
    public class EnergyReceiver : MonoBehaviour
    {
        [Header("State Colors")]
        [Tooltip("Color when unpowered / waiting for delivery.")]
        [SerializeField] private Color waitingColor = new Color(1f, 0.7f, 0.1f, 1f); // Warm amber

        [Tooltip("Color when energy capsule is successfully received.")]
        [SerializeField] private Color poweredColor = new Color(0.1f, 1f, 0.5f, 1f); // Neon green

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // Delivery State
        public bool IsDelivered { get; private set; } = false;

        // Events
        public event Action<EnergyReceiver> OnDeliverySuccess;

        private Collider2D col;
        private Vector3 initialScale;
        private Coroutine pulseRoutine;
        private float baseTriggerRadius = -1f;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            col.isTrigger = true; // Must be trigger
            initialScale = transform.localScale;

            ApplyDifficultyTolerance();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            EnsureVisibleSprite();
            UpdateVisuals();
        }

        public void ApplyDifficultyTolerance()
        {
            if (col is CircleCollider2D circleCol)
            {
                if (baseTriggerRadius < 0f) baseTriggerRadius = circleCol.radius;
                circleCol.radius = baseTriggerRadius * SettingsManager.GetReceiverToleranceMultiplier();
            }
        }

        public void ResetReceiver()
        {
            IsDelivered = false;
            if (pulseRoutine != null)
            {
                StopCoroutine(pulseRoutine);
                pulseRoutine = null;
            }
            transform.localScale = initialScale;
            ApplyDifficultyTolerance();
            UpdateVisuals();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // If already delivered, ignore any subsequent triggers (strictly counted once)
            if (IsDelivered) return;

            EnergyCapsule capsule = other.GetComponent<EnergyCapsule>();
            if (capsule == null)
            {
                capsule = other.GetComponentInParent<EnergyCapsule>();
            }

            if (capsule != null && !capsule.IsDelivered)
            {
                ReceiveCapsule(capsule);
            }
        }

        private void ReceiveCapsule(EnergyCapsule capsule)
        {
            // Strictly set delivered state
            IsDelivered = true;

            // Notify capsule to absorb/destroy cleanly
            capsule.MarkDelivered();

            // Update visuals & pulse
            UpdateVisuals();
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = StartCoroutine(PulseDeliveredRoutine());

            // Fire event
            OnDeliverySuccess?.Invoke(this);
        }

        private System.Collections.IEnumerator PulseDeliveredRoutine()
        {
            float elapsed = 0f;
            float duration = 0.5f;
            Vector3 peakScale = initialScale * 1.25f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Sin((elapsed / duration) * Mathf.PI);
                transform.localScale = Vector3.Lerp(initialScale, peakScale, t);
                yield return null;
            }

            transform.localScale = initialScale;
            pulseRoutine = null;
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = IsDelivered ? poweredColor : waitingColor;
            }
        }

        private void EnsureVisibleSprite()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateProceduralReceiverSprite();
            }

            spriteRenderer.sortingOrder = 2;
        }

        private static Sprite CreateProceduralReceiverSprite()
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);
            Color dark = new Color(0.1f, 0.15f, 0.25f, 1f);
            Color rim = Color.white;
            Vector2 center = new Vector2(size / 2f, size / 2f);

            float rOuter = size / 2f - 2f;
            float rInner = rOuter - 8f;
            float rCore = 8f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d > rOuter)
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                    else if (d >= rInner || d <= rCore)
                    {
                        tex.SetPixel(x, y, rim);
                    }
                    else
                    {
                        tex.SetPixel(x, y, dark);
                    }
                }
            }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }



        private void OnDrawGizmos()
        {
            Gizmos.color = IsDelivered ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1.2f);
        }
    }
}
