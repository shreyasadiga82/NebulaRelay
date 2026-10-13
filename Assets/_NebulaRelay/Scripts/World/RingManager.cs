using System;
using System.Collections.Generic;
using UnityEngine;
using NebulaRelay.Core;
using NebulaRelay.Effects;

namespace NebulaRelay.World
{
    /// <summary>
    /// RingManager manages the sequential orbital relay checkpoints in each mission.
    /// Tracks compulsory relay count, awards score on activation, and signals exit unlock when all relays are synchronized.
    /// </summary>
    [SelectionBase]
    public class RingManager : MonoBehaviour
    {
        [Header("Relay Progression")]
        [Tooltip("Ordered list of RingGates in the level. If left empty, auto-populates from scene by RingOrder.")]
        [SerializeField] private List<RingGate> rings = new List<RingGate>();

        // State
        private int currentExpectedOrder = 1;
        private int activatedRelaysCount = 0;
        private bool allRelaysCompleted = false;

        // Public Interface & Properties
        public int CurrentExpectedOrder => currentExpectedOrder;
        public int TotalRings => rings != null ? rings.Count : 0;
        public int TotalRelays => TotalRings;
        public int ActivatedRelaysCount => activatedRelaysCount;
        public int RemainingRelays => Mathf.Max(0, TotalRelays - activatedRelaysCount);
        public bool AllRingsCompleted => allRelaysCompleted;
        public bool AllRelaysSynchronized => allRelaysCompleted;

        // Events
        public event Action<RingGate> OnRingPassed;
        public event Action<RingGate, int> OnWrongRingAttempted;
        public event Action OnAllRingsPassed;
        public event Action OnAllRelaysSynchronized;

        private void Awake()
        {
            InitializeRings();
        }

        public void InitializeRings()
        {
            currentExpectedOrder = 1;
            activatedRelaysCount = 0;
            allRelaysCompleted = false;

            if (rings != null)
            {
                rings.RemoveAll(r => r == null);
            }

            if (rings == null || rings.Count == 0)
            {
                RingGate[] foundRings = FindObjectsByType<RingGate>(FindObjectsSortMode.None);
                rings = new List<RingGate>(foundRings);
                rings.Sort((a, b) => a.RingOrder.CompareTo(b.RingOrder));
            }

            // Set initial relay states: Relay 1 is Active, others are Inactive
            for (int i = 0; i < rings.Count; i++)
            {
                if (rings[i] != null)
                {
                    rings[i].SetState(i == 0 ? RingState.Active : RingState.Inactive);
                }
            }
        }

        /// <summary>
        /// Called when the player drone or capsule passes through any RingGate's trigger area.
        /// </summary>
        public void OnPlayerCrossedRing(RingGate ring)
        {
            if (allRelaysCompleted || ring == null) return;

            // 1. Correct sequence check
            if (ring.RingOrder == currentExpectedOrder)
            {
                ring.SetState(RingState.Completed);
                ring.PlayActivationGlow();
                activatedRelaysCount++;

                // Award points & floating text
                ScoreManager.Instance?.AwardRelayPoints(ring.GetInstanceID(), ring.transform.position, ScoreManager.PointsPerRelay);
                AudioManager.Instance?.PlayRelaySound(ring.RingOrder);
                AudioManager.Instance?.PlayScoreRewardSound();

                OnRingPassed?.Invoke(ring);
                currentExpectedOrder++;

                // Activate the next ring in sequence if any
                int nextIndex = rings.FindIndex(r => r != null && r.RingOrder == currentExpectedOrder);
                if (nextIndex >= 0 && nextIndex < rings.Count)
                {
                    rings[nextIndex].SetState(RingState.Active);
                    LevelManager.Instance?.SetHUDMessage($"RELAYS: {activatedRelaysCount}/{TotalRelays} SYNCHRONIZED", 2.0f, false);
                }
                else
                {
                    // Every compulsory relay in the mission is synchronized!
                    allRelaysCompleted = true;
                    OnAllRingsPassed?.Invoke();
                    OnAllRelaysSynchronized?.Invoke();

                    LevelManager.Instance?.SetHUDMessage("ALL RELAYS SYNCHRONIZED — EXIT UNLOCKED!", 5.0f, false);
                    AudioManager.Instance?.PlayExitUnlockedSound();
                }
            }
            // 2. Already completed relay (ignore gracefully, no duplicate scoring)
            else if (ring.RingOrder < currentExpectedOrder)
            {
                // Already passed
            }
            // 3. Out-of-order relay violation
            else
            {
                ring.FlashWrongOrder();
                AudioManager.Instance?.PlayExitLockedDenialSound();
                OnWrongRingAttempted?.Invoke(ring, currentExpectedOrder);
                LevelManager.Instance?.SetHUDMessage($"WRONG SEQUENCE! Synchronize Relay {currentExpectedOrder} first!", 2.5f, true);
            }
        }
    }
}
