using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaRelay.World
{
    /// <summary>
    /// ParallaxBackground provides a live, continuously moving multi-layer cosmic background.
    /// Deep space stars, volumetric nebulae, distant planets, and asteroid belts move
    /// both from camera parallax AND autonomous continuous space drift.
    /// Anchors deep cosmic layers to the camera view so the scene never runs out of background.
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        [System.Serializable]
        public class ParallaxLayer
        {
            public string layerName = "Layer";
            public Transform layerTransform;
            [Tooltip("Parallax factor (0.02 = distant nebula, 0.08 = planet, 0.20 = asteroids)")]
            [Range(0f, 1f)] public float parallaxFactor = 0.04f;
            [Tooltip("Continuous autonomous drift speed in units/sec")]
            public Vector2 driftVelocity = new Vector2(-0.08f, -0.03f);
            [Tooltip("If true, layer stays centered with the camera plus parallax offset and continuous drift.")]
            public bool anchorToCamera = true;

            [HideInInspector] public Vector3 initialPosition;
            [HideInInspector] public Vector2 accumulatedDrift;
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
                    layer.accumulatedDrift = Vector2.zero;
                }
            }

            isInitialized = true;
        }

        private void LateUpdate()
        {
            if (!isInitialized || targetCamera == null) return;

            Vector3 currentCamPos = targetCamera.transform.position;
            float dt = Time.deltaTime;

            foreach (var layer in layers)
            {
                if (layer.layerTransform == null) continue;

                // Accumulate autonomous drift
                layer.accumulatedDrift += layer.driftVelocity * dt;

                // Wrap subtle drift so it never flies away infinitely
                if (Mathf.Abs(layer.accumulatedDrift.x) > 20f) layer.accumulatedDrift.x %= 20f;
                if (Mathf.Abs(layer.accumulatedDrift.y) > 20f) layer.accumulatedDrift.y %= 20f;

                if (layer.anchorToCamera)
                {
                    // Nebula & deep space: moves with camera by parallax factor + autonomous drifting
                    float x = currentCamPos.x * layer.parallaxFactor + layer.initialPosition.x + layer.accumulatedDrift.x;
                    float y = currentCamPos.y * layer.parallaxFactor + layer.initialPosition.y + layer.accumulatedDrift.y;
                    layer.layerTransform.position = new Vector3(x, y, layer.initialPosition.z);
                }
                else
                {
                    // Floating elements: relative delta movement
                    Vector3 camDelta = currentCamPos - lastCameraPosition;
                    Vector3 newPos = layer.layerTransform.position;
                    newPos.x += camDelta.x * layer.parallaxFactor + (layer.driftVelocity.x * dt);
                    newPos.y += camDelta.y * layer.parallaxFactor + (layer.driftVelocity.y * dt);
                    layer.layerTransform.position = newPos;
                }
            }

            lastCameraPosition = currentCamPos;
        }

        public void AddLayer(Transform trans, float factor, Vector2 drift, bool anchorToCam = true, string name = "Layer")
        {
            ParallaxLayer layer = new ParallaxLayer
            {
                layerName = name,
                layerTransform = trans,
                parallaxFactor = factor,
                driftVelocity = drift,
                anchorToCamera = anchorToCam,
                initialPosition = trans.position,
                accumulatedDrift = Vector2.zero
            };
            layers.Add(layer);
        }
    }
}
