using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using NebulaRelay.Player;
using NebulaRelay.World;
using NebulaRelay.Delivery;
using NebulaRelay.Effects;

namespace NebulaRelay.Core
{
    public enum LevelState
    {
        InFlight,
        Completed,
        Failed
    }

    /// <summary>
    /// LevelManager coordinates all mission objectives, 120-second countdown timer,
    /// compulsory relay checks, scoring, and one-time win/fail state transitions.
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        public const float MissionDurationSeconds = 120.0f; // 2 minutes

        [Header("Level Information")]
        [Tooltip("The level index (1, 2, 3, 4, 5).")]
        [SerializeField] private int levelNumber = 1;

        [Tooltip("Display name for the level.")]
        [SerializeField] private string levelTitle = "First Signal";

        [Header("Scene References")]
        [SerializeField] private DroneController drone;
        [SerializeField] private BatteryManager battery;
        [SerializeField] private RingManager ringManager;
        [SerializeField] private RelayStation relayStation;
        [SerializeField] private CapsuleLauncher launcher;

        [Header("Flight Corridor Boundaries")]
        [Tooltip("Corridor start X coordinate.")]
        [SerializeField] private float minCorridorX = -18f;
        [Tooltip("Corridor end X coordinate (Relay station location).")]
        [SerializeField] private float maxCorridorX = 18f;
        [Tooltip("Safety margin beyond start/end.")]
        [SerializeField] private float marginX = 2.0f;
        [Tooltip("Half-height of corridor.")]
        [SerializeField] private float corridorMarginY = 6.5f;

        public float FlightMinX => minCorridorX - marginX;
        public float FlightMaxX => maxCorridorX + marginX;
        public float FlightMinY => -corridorMarginY;
        public float FlightMaxY => corridorMarginY;

        // Level State & Mission Timer
        public LevelState State { get; private set; } = LevelState.InFlight;
        public int LevelNumber => levelNumber;
        public string LevelTitle => levelTitle;
        public float RemainingTime { get; private set; } = 120.0f;
        public float MissionDuration => SettingsManager.GetMissionDuration();
        public float ElapsedTime => Mathf.Max(0f, MissionDuration - RemainingTime);
        public GameDifficulty Difficulty => SettingsManager.CurrentDifficulty;

        public DroneController Drone => drone;
        public BatteryManager Battery => battery;
        public RingManager Rings => ringManager;
        public RelayStation Relay => relayStation;
        public CapsuleLauncher Launcher => launcher;
        public string CurrentObjectiveText => GetObjectiveText();

        // Banner Alert System
        public string CurrentAlertMessage { get; private set; } = "";
        public bool IsCurrentAlertWarning { get; private set; } = false;
        private float alertExpireTime = 0f;

        // Failure Reason
        public string FailureReason { get; private set; } = "";

        // Audio countdown state
        private bool warning10sPlayed = false;
        private int lastSecondsWarningPlayed = -1;

        // Events
        public event Action OnLevelCompleted;
        public event Action<string> OnLevelFailed;
        public event Action<float> OnTimerTick;

        private void Awake()
        {
            Instance = this;

            if (drone == null) drone = FindFirstObjectByType<DroneController>();
            if (battery == null) battery = FindFirstObjectByType<BatteryManager>();
            if (ringManager == null) ringManager = FindFirstObjectByType<RingManager>();
            if (relayStation == null) relayStation = FindFirstObjectByType<RelayStation>();
            if (launcher == null && drone != null) launcher = drone.GetComponent<CapsuleLauncher>();
            if (launcher == null) launcher = FindFirstObjectByType<CapsuleLauncher>();

            // Ensure ScoreManager exists
            if (FindFirstObjectByType<ScoreManager>() == null)
            {
                gameObject.AddComponent<ScoreManager>();
            }

            RemainingTime = MissionDuration;
            State = LevelState.InFlight;
        }

        private void Start()
        {
            SetHUDMessage($"MISSION {levelNumber:D2}: {levelTitle.ToUpper()}", 3.0f, false);
            AudioManager.Instance?.PlayMissionStartupSound();
        }

        private void Update()
        {
            // Update 120-second countdown timer during flight
            if (State == LevelState.InFlight && Time.timeScale > 0.001f)
            {
                RemainingTime = Mathf.Max(0f, RemainingTime - Time.deltaTime);
                OnTimerTick?.Invoke(RemainingTime);

                // Audio warnings
                CheckCountdownAudioWarnings();

                // Failure condition: Time Expired
                if (RemainingTime <= 0f)
                {
                    OnMissionFailed("TIME EXPIRED");
                }
            }

            // Quick keyboard shortcuts
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (kb.rKey.wasPressedThisFrame)
                {
                    RestartLevel();
                }

                if (State == LevelState.Completed && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame))
                {
                    AdvanceToNextLevel();
                }
            }
        }

        private void CheckCountdownAudioWarnings()
        {
            int wholeSec = Mathf.CeilToInt(RemainingTime);

            if (wholeSec <= 10 && !warning10sPlayed && wholeSec > 5)
            {
                warning10sPlayed = true;
                AudioManager.Instance?.PlayCountdownWarningSound(false);
            }
            else if (wholeSec <= 5 && wholeSec >= 1 && wholeSec != lastSecondsWarningPlayed)
            {
                lastSecondsWarningPlayed = wholeSec;
                AudioManager.Instance?.PlayCountdownWarningSound(true);
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

        public bool AreAllRelaysSynchronized()
        {
            if (ringManager != null)
            {
                return ringManager.AllRelaysSynchronized;
            }
            return true;
        }

        public void OnRelayActivated(RelayStation relay)
        {
            if (State != LevelState.InFlight) return;

            // MANDATORY CHECK: Every compulsory relay must be active!
            if (!AreAllRelaysSynchronized())
            {
                int remaining = ringManager != null ? ringManager.RemainingRelays : 0;
                SetHUDMessage($"MISSION LOCKED — {remaining} RELAYS REMAINING", 3.0f, true);
                AudioManager.Instance?.PlayExitLockedDenialSound();
                return;
            }

            State = LevelState.Completed;

            // Finalize Score
            int capsRemaining = launcher != null ? launcher.RemainingCapsules : 0;
            ScoreManager.Instance?.FinalizeMissionScore(RemainingTime, capsRemaining);

            AudioManager.Instance?.PlayMissionCompleteSound();
            SetHUDMessage($"SECTOR {levelNumber:D2} RESTORED! MISSION ACCOMPLISHED!", 10.0f, false);

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

            if (reason != null && reason.Contains("CAPSULE"))
            {
                AudioManager.Instance?.PlayNoCapsulesSound();
            }
            else
            {
                AudioManager.Instance?.PlayMissionFailedSound();
            }
            SetHUDMessage($"MISSION FAILED: {reason}\nPRESS [R] TO RETRY", 20.0f, true);

            OnLevelFailed?.Invoke(reason);
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            AudioManager.Instance?.PlayMissionResetSound();
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
            Time.timeScale = 1f;
            int maxSectors = GameManager.Instance != null ? GameManager.Instance.MaxRelays : 5;

            if (levelNumber < maxSectors)
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

        public string GetObjectiveText()
        {
            if (State == LevelState.Completed)
            {
                return levelNumber >= 5 
                    ? "MISSION ACCOMPLISHED! GALACTIC NETWORK FULLY RESTORED!" 
                    : "RELAY SYNCHRONIZED! Press [SPACE] for Next Sector.";
            }

            if (State == LevelState.Failed)
            {
                return $"FAILED: {FailureReason} — Press [R] to Restart.";
            }

            // Step 1: Compulsory Relays
            if (ringManager != null && !ringManager.AllRelaysSynchronized && ringManager.TotalRelays > 0)
            {
                return $"STEP 1: Synchronize Compulsory Relays ({ringManager.ActivatedRelaysCount}/{ringManager.TotalRelays})";
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
                return $"STEP 3: Approach Station Docking Bay at Safe Speed (≤ {safeSpeed:F1} m/s)";
            }

            return "RESTORE ORBITAL RELAY";
        }
    }
}
