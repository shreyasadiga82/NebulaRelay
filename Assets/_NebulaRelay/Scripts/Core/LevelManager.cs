using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using NebulaRelay.Player;
using NebulaRelay.World;

namespace NebulaRelay.Core
{
    public enum LevelState
    {
        InFlight,
        Completed,
        Failed
    }

    /// <summary>
    /// LevelManager coordinates all objectives, UI messages, and win/fail states for the active scene.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Information")]
        [Tooltip("The level index (1, 2, or 3).")]
        [SerializeField] private int levelNumber = 1;

        [Tooltip("Display name for the level (e.g. First Signal, The Debris Field, The Last Transmission).")]
        [SerializeField] private string levelTitle = "First Signal";

        [Header("Scene References")]
        [SerializeField] private DroneController drone;
        [SerializeField] private BatteryManager battery;
        [SerializeField] private RingManager ringManager;
        [SerializeField] private RelayStation relayStation;

        // Level State
        public LevelState State { get; private set; } = LevelState.InFlight;
        public int LevelNumber => levelNumber;
        public string LevelTitle => levelTitle;
        public DroneController Drone => drone;
        public BatteryManager Battery => battery;
        public RingManager Rings => ringManager;
        public RelayStation Relay => relayStation;

        // Current Objective Text
        public string CurrentObjectiveText => GetObjectiveText();

        // Banner Alert System
        public string CurrentAlertMessage { get; private set; } = "";
        public bool IsCurrentAlertWarning { get; private set; } = false;
        private float alertExpireTime = 0f;

        // Failure Reason
        public string FailureReason { get; private set; } = "";

        // Events
        public event Action OnLevelCompleted;
        public event Action<string> OnLevelFailed;

        private void Awake()
        {
            Instance = this;

            // Auto-locate scene references if not wired in Inspector
            if (drone == null) drone = FindFirstObjectByType<DroneController>();
            if (battery == null) battery = FindFirstObjectByType<BatteryManager>();
            if (ringManager == null) ringManager = FindFirstObjectByType<RingManager>();
            if (relayStation == null) relayStation = FindFirstObjectByType<RelayStation>();
        }

        private void Start()
        {
            SetHUDMessage($"MISSION {levelNumber}: {levelTitle.ToUpper()}", 3.0f, false);
        }

        private void Update()
        {
            // Quick restart key (R)
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartLevel();
            }

            // Quick advance key on completion (Space or Return)
            if (State == LevelState.Completed && (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
            {
                AdvanceToNextLevel();
            }
        }

        public void SetHUDMessage(string message, float duration, bool isWarning)
        {
            CurrentAlertMessage = message;
            IsCurrentAlertWarning = isWarning;
            alertExpireTime = Time.time + duration;
        }

        public bool IsAlertActive()
        {
            return Time.time < alertExpireTime && !string.IsNullOrEmpty(CurrentAlertMessage);
        }

        public void OnRelayActivated(RelayStation relay)
        {
            if (State != LevelState.InFlight) return;

            State = LevelState.Completed;
            SetHUDMessage($"RELAY {levelNumber} RESTORED! PRESS [SPACE] TO CONTINUE", 10.0f, false);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterRelayRestored(levelNumber);
            }

            OnLevelCompleted?.Invoke();
        }

        public void OnMissionFailed(string reason)
        {
            if (State != LevelState.InFlight) return;

            State = LevelState.Failed;
            FailureReason = reason;
            SetHUDMessage($"MISSION FAILED: {reason}\nPRESS [R] TO RESTART", 20.0f, true);

            OnLevelFailed?.Invoke(reason);
        }

        public void RestartLevel()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartCurrentLevel();
            }
            else
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        public void AdvanceToNextLevel()
        {
            if (levelNumber < 3)
            {
                int nextLevel = levelNumber + 1;
                string nextSceneName = $"Level_{nextLevel:D2}";

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.LoadLevel(nextLevel);
                }
                else
                {
                    SceneManager.LoadScene(nextSceneName);
                }
            }
            else
            {
                // Grand Victory! Return to Main Menu or Victory Screen
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.SetState(GameState.Victory);
                    GameManager.Instance.LoadMainMenu();
                }
                else
                {
                    SceneManager.LoadScene("MainMenu");
                }
            }
        }

        private string GetObjectiveText()
        {
            if (State == LevelState.Completed)
            {
                return levelNumber >= 3 
                    ? "MISSION ACCOMPLISHED! ORBITAL NETWORK FULLY RESTORED!" 
                    : "RELAY ONLINE! Press [SPACE] for Next Sector.";
            }

            if (State == LevelState.Failed)
            {
                return $"FAILED: {FailureReason} — Press [R] to Restart.";
            }

            // Step 1: Rings
            if (ringManager != null && !ringManager.AllRingsCompleted && ringManager.TotalRings > 0)
            {
                return $"STEP 1: Navigate through Orbital Ring {ringManager.CurrentExpectedOrder}/{ringManager.TotalRings}";
            }

            // Step 2: Capsule Delivery
            if (relayStation != null && relayStation.State == RelayState.Inactive)
            {
                return "STEP 2: Aim & Launch Energy Capsule into the Relay Receiver";
            }

            // Step 3: Relay Docking at Safe Speed
            if (relayStation != null && (relayStation.State == RelayState.ReadyForDroneActivation || relayStation.State == RelayState.EnergyDelivered))
            {
                float safeSpeed = relayStation.MaxSafeSpeed;
                return $"STEP 3: Approach Relay Activation Zone at Safe Speed (≤ {safeSpeed:F1})";
            }

            return "RESTORE ORBITAL RELAY";
        }
    }
}
