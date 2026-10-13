using System.Collections;
using UnityEngine;
using NebulaRelay.Player;
using NebulaRelay.Delivery;

namespace NebulaRelay.World
{
    public enum RingState
    {
        Inactive,
        Active,
        Completed
    }

    /// <summary>
    /// RingGate represents an individual orbital relay checkpoint.
    /// Uses a trigger collider to detect when the player drone or launched capsule passes through.
    /// Each relay activates only once per mission.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    [SelectionBase]
    public class RingGate : MonoBehaviour
    {
        [Header("Ring Identity")]
        [Tooltip("The 1-based order index of this ring in the sequence (1, 2, 3...).")]
        [SerializeField] private int ringOrder = 1;

        [Header("Visual Feedback Colors")]
        [Tooltip("Color when this ring is the immediate target to cross.")]
        [SerializeField] private Color activeColor = new Color(0.1f, 0.9f, 1f, 1f); // Bright neon cyan

        [Tooltip("Color when waiting for prior rings to be cleared.")]
        [SerializeField] private Color inactiveColor = new Color(0.1f, 0.4f, 0.6f, 0.4f); // Dimmed blue

        [Tooltip("Color once successfully passed in correct sequence.")]
        [SerializeField] private Color completedColor = new Color(0.2f, 1f, 0.5f, 1f); // Bright neon green

        [Tooltip("Flash color when crossed out of order.")]
        [SerializeField] private Color wrongOrderColor = new Color(1f, 0.2f, 0.2f, 1f); // Warning red

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        // State
        public int RingOrder => ringOrder;
        public RingState State { get; private set; } = RingState.Inactive;
        public bool IsActivated => State == RingState.Completed;

        private Collider2D col;
        private Coroutine flashRoutine;
        private Coroutine activationGlowRoutine;
        private Vector3 initialScale;

        private void Awake()
        {
            col = GetComponent<Collider2D>();
            col.isTrigger = true;
            initialScale = transform.localScale;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            EnsureVisibleSprite();
        }

        private void Start()
        {
            UpdateVisuals();
        }

        public void SetState(RingState newState)
        {
            State = newState;
            UpdateVisuals();
        }

        /// <summary>
        /// Plays an activation pulse animation and glowing surge when passed.
        /// </summary>
        public void PlayActivationGlow()
        {
            if (activationGlowRoutine != null) StopCoroutine(activationGlowRoutine);
            activationGlowRoutine = StartCoroutine(ActivationGlowRoutine());
        }

        private IEnumerator ActivationGlowRoutine()
        {
            float elapsed = 0f;
            float duration = 0.45f;
            Vector3 peakScale = initialScale * 1.35f;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.white; // Flash white core
            }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scaleT = Mathf.Sin(t * Mathf.PI);
                transform.localScale = Vector3.Lerp(initialScale, peakScale, scaleT);
                yield return null;
            }

            transform.localScale = initialScale;
            UpdateVisuals();
            activationGlowRoutine = null;
        }

        /// <summary>
        /// Flashes the ring red when the player violates the required sequence.
        /// </summary>
        public void FlashWrongOrder()
        {
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(WrongOrderFlashRoutine());
        }

        private IEnumerator WrongOrderFlashRoutine()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = wrongOrderColor;
                transform.localScale = initialScale * 1.15f;
            }

            yield return new WaitForSeconds(0.4f);

            transform.localScale = initialScale;
            UpdateVisuals();
            flashRoutine = null;
        }

        private void UpdateVisuals()
        {
            if (spriteRenderer == null) return;

            switch (State)
            {
                case RingState.Inactive:
                    spriteRenderer.color = inactiveColor;
                    break;
                case RingState.Active:
                    spriteRenderer.color = activeColor;
                    break;
                case RingState.Completed:
                    spriteRenderer.color = completedColor;
                    break;
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Do not react if already completed
            if (State == RingState.Completed) return;

            bool isPlayerOrCapsule = false;

            // 1. Drone passage
            DroneController drone = other.GetComponent<DroneController>();
            if (drone == null) drone = other.GetComponentInParent<DroneController>();
            if (drone != null || other.CompareTag("Player"))
            {
                isPlayerOrCapsule = true;
            }

            // 2. Launched Energy Capsule passage
            if (!isPlayerOrCapsule)
            {
                EnergyCapsule capsule = other.GetComponent<EnergyCapsule>();
                if (capsule == null) capsule = other.GetComponentInParent<EnergyCapsule>();
                if (capsule != null && !capsule.IsDelivered)
                {
                    isPlayerOrCapsule = true;
                }
            }

            if (isPlayerOrCapsule)
            {
                RingManager manager = FindFirstObjectByType<RingManager>();
                if (manager != null)
                {
                    manager.OnPlayerCrossedRing(this);
                }
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
                spriteRenderer.CreateProceduralRingSprite();
            }

            spriteRenderer.sortingOrder = 3;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = State == RingState.Completed ? Color.green : (State == RingState.Active ? Color.cyan : Color.gray);
            Gizmos.DrawWireSphere(transform.position, 1.0f);
        }
    }

    internal static class RingGateExtensions
    {
        public static void CreateProceduralRingSprite(this SpriteRenderer sr)
        {
            int size = 64;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;

            Color transparent = new Color(0, 0, 0, 0);
            Color white = Color.white;
            Vector2 center = new Vector2(size / 2f, size / 2f);

            float outerR = size / 2f - 2f;
            float innerR = outerR - 6f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= outerR && d >= innerR)
                    {
                        tex.SetPixel(x, y, white);
                    }
                    else
                    {
                        tex.SetPixel(x, y, transparent);
                    }
                }
            }

            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
        }
    }
}
