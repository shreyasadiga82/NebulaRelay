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

        private Vector3 currentVelocity = Vector3.zero;
        private Rigidbody2D targetRb;

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

        private void Start()
        {
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

        private void LateUpdate()
        {
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
