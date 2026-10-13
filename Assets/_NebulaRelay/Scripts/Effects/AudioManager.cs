using System;
using UnityEngine;
using NebulaRelay.Core;

namespace NebulaRelay.Effects
{
    /// <summary>
    /// AudioManager provides zero-dependency procedural audio synthesis for all gameplay actions.
    /// Manages master, SFX, and ambient music channels with immediate volume response and muting.
    /// Generates sci-fi tones procedurally on Awake so no external audio files are required.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource thrustSource;
        private AudioSource ambientSource;
        private AudioSource projectileSource;
        private AudioSource uiSource;

        private AudioClip launchClip;
        private AudioClip ringPassClip;
        private AudioClip impactClip;
        private AudioClip relayClip;
        private AudioClip scoreClip;
        private AudioClip unlockClip;
        private AudioClip lockBuzzClip;
        private AudioClip warningClip;
        private AudioClip urgentCountdownClip;
        private AudioClip failureClip;
        private AudioClip victoryClip;
        private AudioClip clickClip;
        private AudioClip pauseClip;
        private AudioClip resumeClip;
        private AudioClip thrustClip;
        private AudioClip ambientClip;
        private AudioClip projectileHumClip;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (GetComponents<Component>().Length > 2)
                {
                    Destroy(this);
                }
                else
                {
                    Destroy(gameObject);
                }
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitAudioSources();
            GenerateProceduralAudioClips();
            ApplySettings();

            SettingsManager.OnSettingsChanged += ApplySettings;
        }

        private void OnDestroy()
        {
            SettingsManager.OnSettingsChanged -= ApplySettings;
        }

        private void InitAudioSources()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            uiSource = gameObject.AddComponent<AudioSource>();
            uiSource.playOnAwake = false;

            thrustSource = gameObject.AddComponent<AudioSource>();
            thrustSource.loop = true;
            thrustSource.playOnAwake = false;

            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;

            projectileSource = gameObject.AddComponent<AudioSource>();
            projectileSource.loop = true;
            projectileSource.playOnAwake = false;
        }

        private void GenerateProceduralAudioClips()
        {
            launchClip = CreateChirpClip("SFX_Launch", 850f, 320f, 0.18f);
            ringPassClip = CreateTwoToneClip("SFX_RingPass", 784f, 1046f, 0.22f);
            impactClip = CreateBellClip("SFX_Impact", 220f, 0.35f);
            relayClip = CreateArpeggioClip("SFX_RelayActive", new float[] { 523f, 659f, 784f, 1046f }, 0.6f);
            scoreClip = CreateTwoToneClip("SFX_Score", 1046f, 1318f, 0.15f);
            unlockClip = CreateArpeggioClip("SFX_ExitUnlock", new float[] { 440f, 554f, 659f, 880f }, 0.5f);
            lockBuzzClip = CreateBuzzClip("SFX_LockedBuzz", 120f, 0.25f);
            warningClip = CreateChirpClip("SFX_Warning", 440f, 440f, 0.12f);
            urgentCountdownClip = CreateTwoToneClip("SFX_UrgentCountdown", 880f, 880f, 0.08f);
            failureClip = CreateDescendingChirpClip("SFX_Failure", 440f, 110f, 0.7f);
            victoryClip = CreateArpeggioClip("SFX_Victory", new float[] { 523f, 659f, 784f, 1046f, 1318f }, 0.8f);
            clickClip = CreateChirpClip("SFX_Click", 1200f, 900f, 0.04f);
            pauseClip = CreateTwoToneClip("SFX_Pause", 600f, 400f, 0.12f);
            resumeClip = CreateTwoToneClip("SFX_Resume", 400f, 600f, 0.12f);
            thrustClip = CreateRumbleClip("SFX_Thrust", 75f, 1.0f);
            projectileHumClip = CreateRumbleClip("SFX_ProjectileHum", 180f, 1.0f);
            ambientClip = CreateAmbientPadClip("SFX_CosmicAmbient", 110f, 4.0f);

            thrustSource.clip = thrustClip;
            projectileSource.clip = projectileHumClip;

            if (ambientClip != null)
            {
                ambientSource.clip = ambientClip;
                ambientSource.Play();
            }
        }

        public void ApplySettings()
        {
            bool muted = SettingsManager.IsMuted;
            float master = SettingsManager.MasterVolume;
            float sfx = SettingsManager.SFXVolume;
            float music = SettingsManager.MusicVolume;

            AudioListener.volume = muted ? 0f : master;

            if (sfxSource != null) sfxSource.volume = sfx;
            if (uiSource != null) uiSource.volume = sfx;
            if (thrustSource != null) thrustSource.volume = sfx * 0.35f;
            if (projectileSource != null) projectileSource.volume = sfx * 0.25f;
            if (ambientSource != null)
            {
                ambientSource.volume = music * 0.22f;
                if (music <= 0.001f && ambientSource.isPlaying) ambientSource.Pause();
                else if (music > 0.001f && !ambientSource.isPlaying) ambientSource.Play();
            }
        }

        // Gameplay Audio Triggers
        public void PlayLaunch() => PlaySFX(launchClip, 0.85f);
        public void PlayRingPass() => PlaySFX(ringPassClip, 0.9f);
        public void PlayImpact() => PlaySFX(impactClip, 0.9f);
        public void PlayRelayActivated() => PlaySFX(relayClip, 1.0f);
        public void PlayScoreReward() => PlaySFX(scoreClip, 0.8f);
        public void PlayExitUnlocked() => PlaySFX(unlockClip, 0.95f);
        public void PlayMissionLocked() => PlaySFX(lockBuzzClip, 0.85f);
        public void PlayCountdownWarning(bool urgent = false)
        {
            if (!SettingsManager.CountdownAudioEnabled) return;
            PlaySFX(urgent ? urgentCountdownClip : warningClip, 0.9f);
        }
        public void PlayWarning() => PlaySFX(warningClip, 0.75f);
        public void PlayFailure() => PlaySFX(failureClip, 0.95f);
        public void PlayVictory() => PlaySFX(victoryClip, 1.0f);
        public void PlayButtonClick() => PlayUI(clickClip, 0.7f);
        public void PlayPause() => PlayUI(pauseClip, 0.75f);
        public void PlayResume() => PlayUI(resumeClip, 0.75f);

        public void SetThrustActive(bool active)
        {
            if (thrustSource == null) return;
            if (active && !thrustSource.isPlaying) thrustSource.Play();
            else if (!active && thrustSource.isPlaying) thrustSource.Stop();
        }

        public void SetProjectileInFlight(bool inFlight)
        {
            if (projectileSource == null) return;
            if (inFlight && !projectileSource.isPlaying) projectileSource.Play();
            else if (!inFlight && projectileSource.isPlaying) projectileSource.Stop();
        }

        private void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null && !SettingsManager.IsMuted)
            {
                sfxSource.PlayOneShot(clip, volume * SettingsManager.SFXVolume);
            }
        }

        private void PlayUI(AudioClip clip, float volume = 1f)
        {
            if (clip != null && uiSource != null && !SettingsManager.IsMuted)
            {
                uiSource.PlayOneShot(clip, volume * SettingsManager.SFXVolume);
            }
        }

        // Procedural Audio Synthesizers
        private static AudioClip CreateChirpClip(string name, float freqStart, float freqEnd, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(freqStart, freqEnd, t);
                float envelope = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateDescendingChirpClip(string name, float freqStart, float freqEnd, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float freq = Mathf.Lerp(freqStart, freqEnd, t * t);
                float envelope = Mathf.Cos(t * Mathf.PI * 0.5f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateBuzzClip(string name, float freq, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float envelope = 1f - t;
                // Square-ish wave for harsh electric buzz
                float sin = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                data[i] = Mathf.Sign(sin) * 0.7f * envelope;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateTwoToneClip(string name, float freq1, float freq2, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            int mid = samples / 2;

            for (int i = 0; i < samples; i++)
            {
                float freq = i < mid ? freq1 : freq2;
                float t = (i % mid) / (float)mid;
                float envelope = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateBellClip(string name, float freq, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float decay = Mathf.Exp(-t * 5f);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * decay;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArpeggioClip(string name, float[] freqs, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];
            int noteCount = freqs.Length;
            int noteLength = samples / noteCount;

            for (int i = 0; i < samples; i++)
            {
                int noteIndex = Mathf.Clamp(i / noteLength, 0, noteCount - 1);
                float freq = freqs[noteIndex];
                float t = (i % noteLength) / (float)noteLength;
                float envelope = Mathf.Sin(t * Mathf.PI);
                data[i] = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * envelope;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateRumbleClip(string name, float freq, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float sin = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate));
                float noise = (UnityEngine.Random.value * 2f - 1f) * 0.25f;
                data[i] = Mathf.Clamp((sin + noise) * 0.5f, -1f, 1f);
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateAmbientPadClip(string name, float baseFreq, float duration)
        {
            int sampleRate = 44100;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = i / (float)samples;
                float f1 = baseFreq;
                float f2 = baseFreq * 1.5f; // Perfect fifth
                float f3 = baseFreq * 1.25f; // Major third
                float val = Mathf.Sin(2f * Mathf.PI * f1 * (i / (float)sampleRate)) * 0.4f
                          + Mathf.Sin(2f * Mathf.PI * f2 * (i / (float)sampleRate)) * 0.3f
                          + Mathf.Sin(2f * Mathf.PI * f3 * (i / (float)sampleRate)) * 0.3f;
                // Soft cyclic crossfade loop
                float fade = Mathf.Sin(t * Mathf.PI);
                data[i] = val * fade * 0.6f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
