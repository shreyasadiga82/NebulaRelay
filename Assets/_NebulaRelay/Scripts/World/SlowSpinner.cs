using UnityEngine;

namespace NebulaRelay.World
{
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
