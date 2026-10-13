using System;
using UnityEngine;

namespace NebulaRelay.Core
{
    /// <summary>
    /// SettingsManager handles user preferences, audio volume channels, display options,
    /// and gameplay toggles. Persists all settings to PlayerPrefs.
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

        public static float MasterVolume { get; private set; } = 1.0f;
        public static float SFXVolume { get; private set; } = 0.9f;
        public static float MusicVolume { get; private set; } = 0.5f;
        public static bool IsMuted { get; private set; } = false;
        public static bool ScreenShakeEnabled { get; private set; } = true;
        public static bool VisualEffectsEnabled { get; private set; } = true;
        public static bool CountdownAudioEnabled { get; private set; } = true;
        public static bool IsFullscreen => Screen.fullScreen;

        public static event Action OnSettingsChanged;

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
        }

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
