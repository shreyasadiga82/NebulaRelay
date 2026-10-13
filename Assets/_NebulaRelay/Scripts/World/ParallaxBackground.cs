using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaRelay.World
{
    /// <summary>
    /// ParallaxBackground provides a multi-layer cinematic cosmic parallax background.
    /// Moves deep space stars, volumetric nebulae, distant planets, and asteroid belts
    /// at distinct fractional speeds relative to camera movement to convey vast cosmic depth.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public string layerName = "Layer";
            public Transform layerTransform;
            [Tooltip("Parallax factor (0.0 = static deep space, 0.05 = nebula, 0.12 = planet, 0.25 = asteroids)")]
            [Range(0f, 1f)] public float parallaxFactor = 0.05f;
            [Tooltip("Optional subtle autonomous cosmic drift speed in units/sec")]
            public Vector2 driftVelocity = Vector2.zero;

            [HideInInspector] public Vector3 initialPosition;
        }

        [Header("Target Tracking")]
        [SerializeField] private Camera targetCamera;

        [Header("Parallax Layers")]
        [SerializeField] private List<ParallaxLayer> layers = new List<ParallaxLayer>();

        private Vector3 lastCameraPosition;
        private bool isInitialized = false;

        private void Start()
        {
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }

            if (targetCamera != null)
            {
                lastCameraPosition = targetCamera.transform.position;
            }

            foreach (var layer in layers)
            {
                if (layer.layerTransform != null)
                {
                    layer.initialPosition = layer.layerTransform.position;
                }
            }

            isInitialized = true;
        }

        private void LateUpdate()
        {
            if (!isInitialized || targetCamera == null) return;

            Vector3 currentCamPos = targetCamera.transform.position;
            Vector3 camDelta = currentCamPos - lastCameraPosition;

            float dt = Time.deltaTime;

            foreach (var layer in layers)
            {
                if (layer.layerTransform == null) continue;

                // Parallax shift (moves in same direction as camera by factor, creating illusion of distance)
                Vector3 newPos = layer.layerTransform.position;
                newPos.x += camDelta.x * layer.parallaxFactor + (layer.driftVelocity.x * dt);
                newPos.y += camDelta.y * layer.parallaxFactor + (layer.driftVelocity.y * dt);

                layer.layerTransform.position = newPos;
            }

            lastCameraPosition = currentCamPos;
        }

        public void AddLayer(Transform trans, float factor, Vector2 drift, string name = "Layer")
        {
            ParallaxLayer layer = new ParallaxLayer
            {
                layerName = name,
                layerTransform = trans,
                parallaxFactor = factor,
                driftVelocity = drift,
                initialPosition = trans.position
            };
            layers.Add(layer);
        }
    }
}
