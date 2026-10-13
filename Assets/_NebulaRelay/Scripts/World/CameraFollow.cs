using UnityEngine;

namespace NebulaRelay.World
{
    /// <summary>
    /// Smooth camera follow script for 2D space flight.
    /// Follows the player drone with smooth damping, velocity look-ahead,
    /// and clamps to the sector flight corridor boundaries.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float smoothTime = 0.25f;

        [Header("Velocity Look-Ahead")]
        [SerializeField] private float lookAheadFactor = 0.4f;
        [SerializeField] private float maxLookAhead = 3.0f;

        [Header("Flight Corridor Bounds")]
        [SerializeField] private bool clampToBounds = true;
        [SerializeField] private Vector2 minBounds = new Vector2(-40f, -8f);
        [SerializeField] private Vector2 maxBounds = new Vector2(40f, 8f);

        [Header("Aspect Ratio Adaptation")]
        [SerializeField] private float baseOrthoSize = 7.5f;

        private Vector3 currentVelocity = Vector3.zero;
        private Rigidbody2D targetRb;
        private Camera cam;

        public void SetTarget(Transform newTarget, Vector2 minB, Vector2 maxB)
        {
            target = newTarget;
            minBounds = minB;
            maxBounds = maxB;
            if (target != null)
            {
                targetRb = target.GetComponent<Rigidbody2D>();
                transform.position = new Vector3(target.position.x, target.position.y, offset.z);
            }
        }

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam == null) cam = Camera.main;
            if (cam != null && cam.orthographic)
            {
                baseOrthoSize = cam.orthographicSize;
            }
            UpdateOrthoSize();
        }

        private void Start()
        {
            if (cam == null) cam = GetComponent<Camera>();
            UpdateOrthoSize();

            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                    targetRb = player.GetComponent<Rigidbody2D>();
                }
            }
        }

        public void UpdateOrthoSize()
        {
            if (cam == null || !cam.orthographic) return;

            // On screens narrower than 16:9 (such as 16:10 or 4:3), raise effective ortho size
            // so the horizontal view is at least 16:9 width and gameplay objects are never cropped.
            float targetAspect = 16f / 9f;
            float currentAspect = cam.aspect;
            if (currentAspect < targetAspect && currentAspect > 0.05f)
            {
                cam.orthographicSize = baseOrthoSize * (targetAspect / currentAspect);
            }
            else
            {
                cam.orthographicSize = baseOrthoSize;
            }
        }

        private void LateUpdate()
        {
            UpdateOrthoSize();
            if (target == null) return;

            Vector3 targetPos = target.position + offset;

            // Velocity look-ahead so player can see where they are heading
            if (targetRb != null)
            {
                Vector2 vel = targetRb.linearVelocity;
                Vector2 lookAhead = Vector2.ClampMagnitude(vel * lookAheadFactor, maxLookAhead);
                targetPos.x += lookAhead.x;
                targetPos.y += lookAhead.y;
            }

            // Clamp camera viewport center to level flight corridor
            if (clampToBounds)
            {
                targetPos.x = Mathf.Clamp(targetPos.x, minBounds.x, maxBounds.x);
                targetPos.y = Mathf.Clamp(targetPos.y, minBounds.y, maxBounds.y);
            }

            targetPos.z = offset.z;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref currentVelocity, smoothTime);
        }
    }
}
