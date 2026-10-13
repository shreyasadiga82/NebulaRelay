using System;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaRelay.World
{
    /// <summary>
    /// RingManager manages the sequential orbital ring checkpoint progression.
    /// Tracks which ring must be crossed next and prevents out-of-order advances.
    /// </summary>
    [SelectionBase]
    public class RingManager : MonoBehaviour
    {
        [Header("Ring Progression")]
        [Tooltip("Ordered list of RingGates in the level. If left empty, auto-populates from scene by RingOrder.")]
        [SerializeField] private List<RingGate> rings = new List<RingGate>();

        [Header("Feedback Settings")]
        [Tooltip("Display built-in on-screen status text and wrong order alerts.")]
        [SerializeField] private bool showOnScreenHUD = true;

        [Tooltip("Duration in seconds that wrong-order alerts remain visible.")]
        [SerializeField] private float alertDuration = 2.5f;

        // State
        private int currentExpectedOrder = 1;
        private string currentAlertMessage = "";
        private float alertExpireTime = 0f;

        // Public Interface & Events
        public int CurrentExpectedOrder => currentExpectedOrder;
        public int TotalRings => rings.Count;
        public bool AllRingsCompleted { get; private set; } = false;

        public event Action<RingGate> OnRingPassed;
        public event Action<RingGate, int> OnWrongRingAttempted;
        public event Action OnAllRingsPassed;

        private void Awake()
        {
            InitializeRings();
        }

        private void InitializeRings()
        {
            if (rings != null)
            {
                rings.RemoveAll(r => r == null);
            }

            if (rings == null || rings.Count == 0)
            {
                // Auto-discover all RingGate objects in scene and sort by ringOrder
                RingGate[] foundRings = FindObjectsByType<RingGate>(FindObjectsSortMode.None);
                rings = new List<RingGate>(foundRings);
                rings.Sort((a, b) => a.RingOrder.CompareTo(b.RingOrder));
            }

            // Set initial ring states: Ring 1 is Active, others are Inactive
            for (int i = 0; i < rings.Count; i++)
            {
                if (rings[i] != null)
                {
                    rings[i].SetState(i == 0 ? RingState.Active : RingState.Inactive);
                }
            }
        }

        /// <summary>
        /// Called when the player drone passes through any RingGate's trigger collider.
        /// </summary>
        public void OnPlayerCrossedRing(RingGate ring)
        {
            if (AllRingsCompleted || ring == null) return;

            // 1. Correct sequence check
            if (ring.RingOrder == currentExpectedOrder)
            {
                // Success: mark completed
                ring.SetState(RingState.Completed);
                OnRingPassed?.Invoke(ring);

                currentExpectedOrder++;

                // Activate the next ring in sequence
                int nextIndex = rings.FindIndex(r => r != null && r.RingOrder == currentExpectedOrder);
                if (nextIndex >= 0 && nextIndex < rings.Count)
                {
                    rings[nextIndex].SetState(RingState.Active);
                    SetAlert($"Ring {ring.RingOrder}/{TotalRings} Passed! Next: Ring {currentExpectedOrder}", 2.0f, false);
                }
                else
                {
                    // All rings in the sequence completed!
                    AllRingsCompleted = true;
                    OnAllRingsPassed?.Invoke();
                    SetAlert("ALL ORBITAL RINGS COMPLETED!", 4.0f, false);
                }
            }
            // 2. Already completed ring
            else if (ring.RingOrder < currentExpectedOrder)
            {
                // Drone re-crossed an already completed ring; ignore without penalty
            }
            // 3. Out-of-order ring violation
            else
            {
                ring.FlashWrongOrder();
                OnWrongRingAttempted?.Invoke(ring, currentExpectedOrder);
                SetAlert($"WRONG RING! Cross Ring {currentExpectedOrder} first!", alertDuration, true);
            }
        }

        private void SetAlert(string message, float duration, bool isWarning)
        {
            currentAlertMessage = message;
            alertExpireTime = Time.time + duration;
        }

        private void OnGUI()
        {
            if (!showOnScreenHUD) return;

            // Top-left HUD box for Ring progression
            GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
            boxStyle.fontSize = 14;
            boxStyle.normal.textColor = Color.white;
            boxStyle.alignment = TextAnchor.MiddleLeft;

            int clearedCount = Mathf.Min(currentExpectedOrder - 1, TotalRings);
            string status = AllRingsCompleted 
                ? "<color=#00FF66><b>ORBITAL RINGS: COMPLETE (ALL PASS)</b></color>" 
                : $"<b>ORBITAL RINGS:</b> {clearedCount}/{TotalRings} (Target: Ring {currentExpectedOrder})";

            GUILayout.BeginArea(new Rect(20, 20, 340, 70), GUI.skin.box);
            GUILayout.Label(status, boxStyle);

            if (Time.time < alertExpireTime && !string.IsNullOrEmpty(currentAlertMessage))
            {
                bool isWarning = currentAlertMessage.Contains("WRONG");
                GUIStyle alertStyle = new GUIStyle(GUI.skin.label);
                alertStyle.fontSize = 13;
                alertStyle.fontStyle = FontStyle.Bold;
                alertStyle.normal.textColor = isWarning ? new Color(1f, 0.3f, 0.3f) : new Color(0.2f, 1f, 0.5f);
                GUILayout.Label(currentAlertMessage, alertStyle);
            }
            GUILayout.EndArea();
        }
    }
}
