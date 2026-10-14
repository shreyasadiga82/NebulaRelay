using System;
using UnityEngine;

namespace NebulaRelay.Core
{
    public enum GameDifficulty
    {
        Easy = 0,    // EXPLORER
        Medium = 1,  // PILOT (Normal)
        Hard = 2,    // VETERAN
        Expert = 3   // VOIDWALKER
    }

    /// <summary>
    /// SettingsManager handles user preferences, audio volume channels, display options,
    /// difficulty modes (Explorer, Pilot/Normal, Veteran, Voidwalker), and gameplay parameters.
    /// Persists all settings to PlayerPrefs.
    /// </summary>
    public static class SettingsManager
    {
        private const string PrefMasterVol = "NR_MasterVolume";
        private const string PrefSFXVol = "NR_SFXVolume";
        private const string PrefMusicVol = "NR_MusicVolume";
        private const string PrefMuted = "NR_AudioMuted";
        private const string PrefScreenShake = "NR_ScreenShake";
        private const string PrefVFX = "NR_VisualEffects";
        private const string PrefCountdownAudio = "NR_CountdownAudio";
        private const string PrefDifficulty = "NR_Difficulty";

        public static float MasterVolume { get; private set; } = 1.0f;
        public static float SFXVolume { get; private set; } = 0.9f;
        public static float MusicVolume { get; private set; } = 0.5f;
        public static bool IsMuted { get; private set; } = false;
        public static bool ScreenShakeEnabled { get; private set; } = true;
        public static bool VisualEffectsEnabled { get; private set; } = true;
        public static bool CountdownAudioEnabled { get; private set; } = true;
        public static GameDifficulty CurrentDifficulty { get; private set; } = GameDifficulty.Medium;
        public static bool IsFullscreen => Screen.fullScreen;

        public static event Action OnSettingsChanged;
        public static event Action<GameDifficulty> OnDifficultyChanged;

        static SettingsManager()
        {
            LoadSettings();
        }

        public static void LoadSettings()
        {
            MasterVolume = PlayerPrefs.GetFloat(PrefMasterVol, 1.0f);
            SFXVolume = PlayerPrefs.GetFloat(PrefSFXVol, 0.9f);
            MusicVolume = PlayerPrefs.GetFloat(PrefMusicVol, 0.5f);
            IsMuted = PlayerPrefs.GetInt(PrefMuted, 0) == 1;
            ScreenShakeEnabled = PlayerPrefs.GetInt(PrefScreenShake, 1) == 1;
            VisualEffectsEnabled = PlayerPrefs.GetInt(PrefVFX, 1) == 1;
            CountdownAudioEnabled = PlayerPrefs.GetInt(PrefCountdownAudio, 1) == 1;
            CurrentDifficulty = (GameDifficulty)PlayerPrefs.GetInt(PrefDifficulty, (int)GameDifficulty.Medium);
        }

        public static void SetDifficulty(GameDifficulty difficulty)
        {
            CurrentDifficulty = difficulty;
            PlayerPrefs.SetInt(PrefDifficulty, (int)difficulty);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
            OnDifficultyChanged?.Invoke(difficulty);
        }

        public static string GetDifficultyName(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return "EASY - EXPLORER";
                case GameDifficulty.Hard: return "HARD - VETERAN";
                case GameDifficulty.Expert: return "EXPERT - VOIDWALKER";
                case GameDifficulty.Medium:
                default: return "NORMAL - PILOT";
            }
        }

        public static string GetDifficultyShortBadge(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return "EXPLORER";
                case GameDifficulty.Hard: return "VETERAN";
                case GameDifficulty.Expert: return "VOIDWALKER";
                case GameDifficulty.Medium:
                default: return "PILOT";
            }
        }

        public static string GetDifficultyDescription(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return "Explore the relay network and learn to drift.";
                case GameDifficulty.Hard: return "Navigate dangerous routes with limited room for error.";
                case GameDifficulty.Expert: return "Master every trajectory. Survive the impossible-looking.";
                case GameDifficulty.Medium:
                default: return "A balanced test of momentum and precision.";
            }
        }

        public static float GetMissionDuration(GameDifficulty diff, int missionIndex = 1)
        {
            // Base mission timers: Easy 360s, Normal 300s, Hard 270s, Expert 240s
            // Ensures timer >= 1.25 x estimated par time for every mission
            float baseDuration;
            switch (diff)
            {
                case GameDifficulty.Easy:
                    baseDuration = 360.0f; // 6m 00s
                    break;
                case GameDifficulty.Hard:
                    baseDuration = 270.0f; // 4m 30s
                    break;
                case GameDifficulty.Expert:
                    baseDuration = 240.0f; // 4m 00s
                    break;
                case GameDifficulty.Medium:
                default:
                    baseDuration = 300.0f; // 5m 00s
                    break;
            }

            // Estimate par time for mission if index provided (1..10)
            // If par time requires higher timer to satisfy timer >= 1.25 x par, raise timer cleanly
            float par = EstimateParTime(missionIndex);
            float minRequired = par * 1.25f;

            if (baseDuration < minRequired)
            {
                // Round up to nearest 5 seconds above minRequired
                return Mathf.Ceil(minRequired / 5.0f) * 5.0f;
            }

            return baseDuration;
        }

        public static float GetMissionDuration() => GetMissionDuration(CurrentDifficulty, LevelManager.Instance != null ? LevelManager.Instance.LevelNumber : 1);

        public static float EstimateParTime(int missionIndex)
        {
            // Mathematical par time formula from Section 7: (pathLength / 4.0) + 15s + (gateCount * 1.5s)
            // Path lengths: ~450u in M1 rising to ~675u in M10; Gates: 16 in M1 rising to 34 in M10
            int gates = 16 + (Mathf.Clamp(missionIndex, 1, 10) - 1) * 2;
            float pathLength = 450.0f + (Mathf.Clamp(missionIndex, 1, 10) - 1) * 25.0f;
            return (pathLength / 4.0f) + 15.0f + (gates * 1.5f);
        }

        public static float GetMaxSafeDockingSpeed(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return 1.8f;
                case GameDifficulty.Hard: return 1.2f;
                case GameDifficulty.Expert: return 1.0f;
                case GameDifficulty.Medium:
                default: return 1.5f;
            }
        }
        public static float GetMaxSafeDockingSpeed() => GetMaxSafeDockingSpeed(CurrentDifficulty);

        public static float GetHazardDamageMultiplier(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return 0.67f; // ~10% damage
                case GameDifficulty.Hard: return 1.50f; // ~22.5% damage
                case GameDifficulty.Expert: return 1.75f; // ~26.25% damage
                case GameDifficulty.Medium:
                default: return 1.0f; // 15% damage
            }
        }
        public static float GetHazardDamageMultiplier() => GetHazardDamageMultiplier(CurrentDifficulty);

        public static float GetBatteryDrainMultiplier(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return 0.75f;
                case GameDifficulty.Hard: return 1.35f;
                case GameDifficulty.Expert: return 1.60f;
                case GameDifficulty.Medium:
                default: return 1.0f;
            }
        }
        public static float GetBatteryDrainMultiplier() => GetBatteryDrainMultiplier(CurrentDifficulty);

        public static float GetScoreMultiplier(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Hard: return 1.5f; // 50% score bonus
                case GameDifficulty.Expert: return 2.0f; // 100% score bonus
                case GameDifficulty.Easy:
                case GameDifficulty.Medium:
                default: return 1.0f;
            }
        }
        public static float GetScoreMultiplier() => GetScoreMultiplier(CurrentDifficulty);

        public static float GetHazardSpeedMultiplier(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return 0.7f;
                case GameDifficulty.Hard: return 1.3f;
                case GameDifficulty.Expert: return 1.5f;
                case GameDifficulty.Medium:
                default: return 1.0f;
            }
        }
        public static float GetHazardSpeedMultiplier() => GetHazardSpeedMultiplier(CurrentDifficulty);

        public static float GetObstacleClearance(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return 3.2f;
                case GameDifficulty.Hard: return 2.2f;
                case GameDifficulty.Expert: return 1.9f;
                case GameDifficulty.Medium:
                default: return 2.6f;
            }
        }
        public static float GetObstacleClearance() => GetObstacleClearance(CurrentDifficulty);

        public static float GetReceiverToleranceMultiplier(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return 1.25f;
                case GameDifficulty.Hard: return 0.90f;
                case GameDifficulty.Expert: return 0.80f;
                case GameDifficulty.Medium:
                default: return 1.00f;
            }
        }
        public static float GetReceiverToleranceMultiplier() => GetReceiverToleranceMultiplier(CurrentDifficulty);

        public static int GetObstaclesPer100u(GameDifficulty diff)
        {
            switch (diff)
            {
                case GameDifficulty.Easy: return 2;
                case GameDifficulty.Hard: return 6;
                case GameDifficulty.Expert: return 8;
                case GameDifficulty.Medium:
                default: return 4;
            }
        }
        public static int GetObstaclesPer100u() => GetObstaclesPer100u(CurrentDifficulty);

        public static int GetCapsuleLimit(GameDifficulty diff, int baseline = 2)
        {
            // Easy +2, Normal baseline, Hard -1, Expert -2, but never less than 2 total
            int mod = 0;
            switch (diff)
            {
                case GameDifficulty.Easy: mod = 2; break;
                case GameDifficulty.Hard: mod = -1; break;
                case GameDifficulty.Expert: mod = -2; break;
                case GameDifficulty.Medium:
                default: mod = 0; break;
            }
            return Mathf.Max(2, baseline + mod);
        }
        public static int GetCapsuleLimit() => GetCapsuleLimit(CurrentDifficulty);

        public static void SetMasterVolume(float val)
        {
            MasterVolume = Mathf.Clamp01(val);
            PlayerPrefs.SetFloat(PrefMasterVol, MasterVolume);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetSFXVolume(float val)
        {
            SFXVolume = Mathf.Clamp01(val);
            PlayerPrefs.SetFloat(PrefSFXVol, SFXVolume);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetMusicVolume(float val)
        {
            MusicVolume = Mathf.Clamp01(val);
            PlayerPrefs.SetFloat(PrefMusicVol, MusicVolume);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetMuted(bool muted)
        {
            IsMuted = muted;
            PlayerPrefs.SetInt(PrefMuted, IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetScreenShake(bool enabled)
        {
            ScreenShakeEnabled = enabled;
            PlayerPrefs.SetInt(PrefScreenShake, ScreenShakeEnabled ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetVisualEffects(bool enabled)
        {
            VisualEffectsEnabled = enabled;
            PlayerPrefs.SetInt(PrefVFX, VisualEffectsEnabled ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void SetCountdownAudio(bool enabled)
        {
            CountdownAudioEnabled = enabled;
            PlayerPrefs.SetInt(PrefCountdownAudio, CountdownAudioEnabled ? 1 : 0);
            PlayerPrefs.Save();
            OnSettingsChanged?.Invoke();
        }

        public static void ToggleFullscreen()
        {
            Screen.fullScreen = !Screen.fullScreen;
            OnSettingsChanged?.Invoke();
        }

        public static void SetFullscreen(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
            OnSettingsChanged?.Invoke();
        }
    }
}
