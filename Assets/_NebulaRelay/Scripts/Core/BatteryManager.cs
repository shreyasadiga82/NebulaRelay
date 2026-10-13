using System;
using UnityEngine;
using NebulaRelay.Player;
using NebulaRelay.Delivery;

namespace NebulaRelay.Core
{
    /// <summary>
    /// BatteryManager tracks drone energy reserves.
    /// Drains power during thrust and projectile firing, and triggers mission failure upon depletion.
    /// </summary>
    [SelectionBase]
    public class BatteryManager : MonoBehaviour
    {
        [Header("Battery Capacity")]
        [Tooltip("Maximum energy capacity (Default: 100).")]
        [SerializeField] private float maxBattery = 100.0f;

        [Tooltip("Starting battery value upon level load.")]
        [SerializeField] private float startingBattery = 100.0f;

        [Header("Discharge Rates")]
        [Tooltip("Battery percentage drained per second while firing thrusters (Default: 0.8%/s).")]
        [SerializeField] private float thrustDrainRate = 0.8f;

        [Tooltip("Battery percentage consumed immediately per capsule launched (Default: 4.0%).")]
        [SerializeField] private float capsuleLaunchCost = 4.0f;

        [Tooltip("Threshold percentage below which low-battery warnings trigger (Default: 20%).")]
        [SerializeField] private float lowBatteryThreshold = 20.0f;

        [Header("Components")]
        [SerializeField] private DroneController drone;
        [SerializeField] private CapsuleLauncher launcher;

        // Current Battery
        public float CurrentBattery { get; private set; }
        public float MaxBattery => maxBattery;
        public float BatteryPercentage => Mathf.Clamp01(CurrentBattery / maxBattery);
        public bool IsDepleted => CurrentBattery <= 0.001f;
        public bool IsLowBattery => !IsDepleted && CurrentBattery <= lowBatteryThreshold;

        // Events
        public event Action<float> OnBatteryChanged;
        public event Action OnBatteryDepleted;
        public event Action OnLowBatteryWarning;

        private bool lowBatteryFired = false;
        private int lastRecordedCapsuleCount = 0;

        private void Awake()
        {
            CurrentBattery = startingBattery;
            if (drone == null) drone = GetComponent<DroneController>();
            if (launcher == null) launcher = GetComponent<CapsuleLauncher>();
        }

        private void Start()
        {
            OnBatteryChanged?.Invoke(CurrentBattery);
        }

        private void Update()
        {
            if (IsDepleted) return;

            // 1. Continuous thrust discharge
            if (drone != null && drone.IsThrusting)
            {
                ConsumeBattery(thrustDrainRate * Time.deltaTime);
            }

            // 2. Discrete capsule launch discharge
            if (launcher != null)
            {
                if (launcher.ActiveCapsulesCount > lastRecordedCapsuleCount)
                {
                    int newlyFired = launcher.ActiveCapsulesCount - lastRecordedCapsuleCount;
                    ConsumeBattery(capsuleLaunchCost * newlyFired);
                }
                lastRecordedCapsuleCount = launcher.ActiveCapsulesCount;
            }

            // 3. Low-battery warning trigger
            if (IsLowBattery && !lowBatteryFired)
            {
                lowBatteryFired = true;
                OnLowBatteryWarning?.Invoke();
                LevelManager.Instance?.SetHUDMessage("WARNING: LOW BATTERY (<20%)!", 2.5f, true);
            }
            else if (!IsLowBattery && lowBatteryFired)
            {
                lowBatteryFired = false;
            }
        }

        /// <summary>
        /// Deducts energy from the battery, ensuring it never drops below 0.
        /// </summary>
        public void ConsumeBattery(float amount)
        {
            if (IsDepleted || amount <= 0f) return;

            CurrentBattery = Mathf.Max(0f, CurrentBattery - amount);
            OnBatteryChanged?.Invoke(CurrentBattery);

            if (CurrentBattery <= 0f)
            {
                TriggerBatteryFailure();
            }
        }

        /// <summary>
        /// Restores battery energy up to max capacity.
        /// </summary>
        public void Recharge(float amount)
        {
            CurrentBattery = Mathf.Min(maxBattery, CurrentBattery + amount);
            OnBatteryChanged?.Invoke(CurrentBattery);
        }

        private void TriggerBatteryFailure()
        {
            CurrentBattery = 0f;
            OnBatteryDepleted?.Invoke();

            // Disable player actions
            if (drone != null) drone.enabled = false;
            if (launcher != null) launcher.enabled = false;

            LevelManager.Instance?.OnMissionFailed("BATTERY DEPLETED — Out of power.");
        }

        /// <summary>
        /// Resets battery to initial level upon level retry.
        /// </summary>
        public void ResetBattery()
        {
            CurrentBattery = startingBattery;
            lowBatteryFired = false;
            if (drone != null) drone.enabled = true;
            if (launcher != null) launcher.enabled = true;
            OnBatteryChanged?.Invoke(CurrentBattery);
        }
    }
}
