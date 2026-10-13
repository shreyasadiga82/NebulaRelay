using System;
using UnityEngine;
using NebulaRelay.Core;

namespace NebulaRelay.Effects
{
    /// <summary>
    /// AudioManager is the centralized audio system for Nebula Relay.
    /// Provides dedicated audio channels, collision source pooling, volume channel controls,
    /// and clean playback methods for all gameplay and UI audio events.
    /// Supports serialized inspector clips, asset loading from Assets/Audio, and procedural fallbacks.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        private static AudioManager instance;
        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<AudioManager>();
                    if (instance != null && !instance.isInitialized)
                    {
                        instance.Initialize();
                    }
                }
                return instance;
            }
            private set => instance = value;
        }

        private bool isInitialized = false;

        public void Initialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            InitAudioSources();
            LoadOrGenerateAudioClips();
            ApplySettings();
        }

        [Header("Capsule Launch SFX")]
        [SerializeField] private AudioClip capsuleLaunchClip;
        [SerializeField] private AudioClip capsuleLaunchDeniedClip;
        [SerializeField] private AudioClip capsuleHumClip;

        [Header("Physics Collision SFX")]
        [SerializeField] private AudioClip collisionLightClip;
        [SerializeField] private AudioClip collisionHeavyClip;

        [Header("Relay & Objective SFX")]
        [SerializeField] private AudioClip relayActivateClip;
        [SerializeField] private AudioClip relayScoreClip;
        [SerializeField] private AudioClip exitUnlockedClip;
        [SerializeField] private AudioClip exitLockedDenialClip;

        [Header("Mission Flow SFX")]
        [SerializeField] private AudioClip missionStartupClip;
        [SerializeField] private AudioClip missionCompleteClip;
        [SerializeField] private AudioClip missionFailedClip;
        [SerializeField] private AudioClip missionResetClip;
        [SerializeField] private AudioClip countdownWarningClip;
        [SerializeField] private AudioClip countdownUrgentClip;

        [Header("UI & Interface SFX")]
        [SerializeField] private AudioClip uiClickClip;
        [SerializeField] private AudioClip uiPauseClip;
        [SerializeField] private AudioClip uiResumeClip;

        [Header("Cosmic Ambience")]
        [SerializeField] private AudioClip ambienceSpaceStationClip;

        // Dedicated AudioSources
        private AudioSource sfxSource;
        private AudioSource uiSource;
        private AudioSource thrustSource;
        private AudioSource projectileSource;
        private AudioSource ambientSource;

        // Collision AudioSource Pool (prevents GC allocations on impacts)
        private const int CollisionPoolSize = 4;
        private AudioSource[] collisionPool;
        private int collisionPoolIndex = 0;

        // Rate-limiting timestamps
        private float lastLaunchSoundTime = -10f;
        private float lastDeniedSoundTime = -10f;
        private float lastImpactSoundTime = -10f;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                if (GetComponents<Component>().Length > 2)
                {
                    Destroy(this);
                }
                else
                {
                    if (Application.isPlaying) Destroy(gameObject);
                    else DestroyImmediate(gameObject);
                }
                return;
            }

            instance = this;
            if (Application.isPlaying)
            {
                DontDestroyOnLoad(gameObject);
            }

            Initialize();

            SettingsManager.OnSettingsChanged += ApplySettings;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
            SettingsManager.OnSettingsChanged -= ApplySettings;
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (projectileSource != null && projectileSource.isPlaying) projectileSource.Stop();
            if (thrustSource != null && thrustSource.isPlaying) thrustSource.Stop();

            if (scene.name == "MainMenu")
            {
                if (ambientSource != null && ambientSource.isPlaying) ambientSource.Stop();
            }
            else
            {
                if (ambientSource != null && !ambientSource.isPlaying && !SettingsManager.IsMuted && SettingsManager.MusicVolume > 0.001f)
                {
                    ambientSource.Play();
                }
            }
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

            // Initialize Collision Pool
            collisionPool = new AudioSource[CollisionPoolSize];
            for (int i = 0; i < CollisionPoolSize; i++)
            {
                collisionPool[i] = gameObject.AddComponent<AudioSource>();
                collisionPool[i].playOnAwake = false;
            }
        }

        private void LoadOrGenerateAudioClips()
        {
#if UNITY_EDITOR
            // Try loading from Assets/Audio in Editor if not assigned
            if (capsuleLaunchClip == null) capsuleLaunchClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Capsule_Launch.wav");
            if (capsuleLaunchDeniedClip == null) capsuleLaunchDeniedClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Capsule_Launch_Denied.wav");
            if (capsuleHumClip == null) capsuleHumClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Capsule_Hum.wav");
            if (collisionLightClip == null) collisionLightClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Collision_Light.wav");
            if (collisionHeavyClip == null) collisionHeavyClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Collision_Heavy.wav");
            if (relayActivateClip == null) relayActivateClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Relay_Activate.wav");
            if (relayScoreClip == null) relayScoreClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Relay_Score.wav");
            if (exitUnlockedClip == null) exitUnlockedClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Exit_Unlocked.wav");
            if (exitLockedDenialClip == null) exitLockedDenialClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Exit_Locked_Denial.wav");
            if (missionStartupClip == null) missionStartupClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Startup.wav");
            if (missionCompleteClip == null) missionCompleteClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Complete.wav");
            if (missionFailedClip == null) missionFailedClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Failed.wav");
            if (missionResetClip == null) missionResetClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Mission_Reset.wav");
            if (countdownWarningClip == null) countdownWarningClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Countdown_Warning.wav");
            if (countdownUrgentClip == null) countdownUrgentClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/Countdown_Urgent.wav");
            if (uiClickClip == null) uiClickClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Click.wav");
            if (uiPauseClip == null) uiPauseClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Pause.wav");
            if (uiResumeClip == null) uiResumeClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/SFX/UI_Resume.wav");
            if (ambienceSpaceStationClip == null) ambienceSpaceStationClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Ambience/Ambience_SpaceStation.wav");
#endif

            // Procedural synthesis fallback ensures zero missing references even if files are missing
            if (capsuleLaunchClip == null) capsuleLaunchClip = CreateChirpClip("Fallback_Launch", 950f, 280f, 0.22f);
            if (capsuleLaunchDeniedClip == null) capsuleLaunchDeniedClip = CreateBuzzClip("Fallback_Denied", 110f, 0.18f);
            if (capsuleHumClip == null) capsuleHumClip = CreateRumbleClip("Fallback_CapsuleHum", 180f, 1.0f);
            if (collisionLightClip == null) collisionLightClip = CreateBellClip("Fallback_ImpactLight", 520f, 0.16f);
            if (collisionHeavyClip == null) collisionHeavyClip = CreateBellClip("Fallback_ImpactHeavy", 110f, 0.38f);
            if (relayActivateClip == null) relayActivateClip = CreateArpeggioClip("Fallback_RelayActive", new float[] { 523f, 659f, 784f, 1046f }, 0.5f);
            if (relayScoreClip == null) relayScoreClip = CreateTwoToneClip("Fallback_Score", 1046f, 1318f, 0.15f);
            if (exitUnlockedClip == null) exitUnlockedClip = CreateArpeggioClip("Fallback_ExitUnlock", new float[] { 440f, 554f, 659f, 880f }, 0.65f);
            if (exitLockedDenialClip == null) exitLockedDenialClip = CreateBuzzClip("Fallback_LockedDenial", 160f, 0.22f);
            if (missionStartupClip == null) missionStartupClip = CreateChirpClip("Fallback_Startup", 320f, 960f, 0.45f);
            if (missionCompleteClip == null) missionCompleteClip = CreateArpeggioClip("Fallback_Complete", new float[] { 523f, 659f, 784f, 1046f, 1318f }, 1.2f);
            if (missionFailedClip == null) missionFailedClip = CreateDescendingChirpClip("Fallback_Failed", 440f, 90f, 0.75f);
            if (missionResetClip == null) missionResetClip = CreateChirpClip("Fallback_Reset", 420f, 840f, 0.14f);
            if (countdownWarningClip == null) countdownWarningClip = CreateChirpClip("Fallback_Warning", 520f, 520f, 0.14f);
            if (countdownUrgentClip == null) countdownUrgentClip = CreateTwoToneClip("Fallback_Urgent", 880f, 880f, 0.10f);
            if (uiClickClip == null) uiClickClip = CreateChirpClip("Fallback_Click", 1200f, 900f, 0.04f);
            if (uiPauseClip == null) uiPauseClip = CreateTwoToneClip("Fallback_Pause", 600f, 400f, 0.12f);
            if (uiResumeClip == null) uiResumeClip = CreateTwoToneClip("Fallback_Resume", 400f, 600f, 0.12f);
            if (ambienceSpaceStationClip == null) ambienceSpaceStationClip = CreateAmbientPadClip("Fallback_Ambience", 110f, 4.0f);

            thrustSource.clip = CreateRumbleClip("SFX_Thrust", 75f, 1.0f);
            projectileSource.clip = capsuleHumClip;

            if (ambienceSpaceStationClip != null)
            {
                ambientSource.clip = ambienceSpaceStationClip;
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

            if (collisionPool != null)
            {
                for (int i = 0; i < collisionPool.Length; i++)
                {
                    if (collisionPool[i] != null) collisionPool[i].volume = sfx;
                }
            }

            if (ambientSource != null)
            {
                ambientSource.volume = music * 0.22f;
                if ((music <= 0.001f || muted) && ambientSource.isPlaying) ambientSource.Pause();
                else if (music > 0.001f && !muted && !ambientSource.isPlaying) ambientSource.Play();
            }
        }

        // ==========================================
        // 1. CAPSULE LAUNCHING AUDIO
        // ==========================================
        public void PlayLaunchSound()
        {
            if (Time.time - lastLaunchSoundTime < 0.15f) return;
            lastLaunchSoundTime = Time.time;
            PlaySFX(capsuleLaunchClip, 0.9f);
        }

        public void PlayLaunchDeniedSound()
        {
            if (Time.time - lastDeniedSoundTime < 0.2f) return;
            lastDeniedSoundTime = Time.time;
            PlaySFX(capsuleLaunchDeniedClip, 0.8f);
        }

        public void SetCapsuleInFlight(bool inFlight)
        {
            if (projectileSource == null) return;
            if (inFlight && !projectileSource.isPlaying && !SettingsManager.IsMuted)
            {
                projectileSource.Play();
            }
            else if (!inFlight && projectileSource.isPlaying)
            {
                projectileSource.Stop();
            }
        }

        // ==========================================
        // 2. PHYSICS COLLISION AUDIO
        // ==========================================
        public void PlayCollisionSound(float impactSpeed, bool heavy = false)
        {
            // Ignore tiny micro-collisions below velocity threshold
            if (impactSpeed < 0.6f) return;

            // Debounce rapid continuous collisions (e.g. dragging against a wall)
            if (Time.time - lastImpactSoundTime < 0.12f) return;
            lastImpactSoundTime = Time.time;

            if (SettingsManager.IsMuted || collisionPool == null) return;

            // Velocity-responsive dynamic volume (scaled between 0.25 and 1.0)
            float normSpeed = Mathf.Clamp01(impactSpeed / 7.5f);
            float volume = Mathf.Lerp(0.25f, 1.0f, normSpeed) * SettingsManager.SFXVolume;

            AudioClip clip = heavy ? collisionHeavyClip : collisionLightClip;
            if (clip == null) return;

            // Round-robin through temporary collision sources pool
            AudioSource src = collisionPool[collisionPoolIndex];
            collisionPoolIndex = (collisionPoolIndex + 1) % CollisionPoolSize;

            if (src != null)
            {
                // Subtle pitch variation (0.95 to 1.05) prevents repetitive mechanical feel
                src.pitch = UnityEngine.Random.Range(0.95f, 1.05f);
                src.PlayOneShot(clip, volume);
            }
        }

        // ==========================================
        // 3. RELAY & EXIT AUDIO
        // ==========================================
        public void PlayRelaySound(int sequenceOrder = 1)
        {
            if (SettingsManager.IsMuted || sfxSource == null) return;

            // Subtle rising-pitch effect per cleared relay in sequence
            float basePitch = 1.0f + Mathf.Clamp(sequenceOrder - 1, 0, 8) * 0.07f;
            sfxSource.pitch = basePitch;
            PlaySFX(relayActivateClip, 0.95f);
            sfxSource.pitch = 1.0f; // Reset pitch
        }

        public void PlayScoreRewardSound() => PlaySFX(relayScoreClip, 0.8f);
        public void PlayExitUnlockedSound() => PlaySFX(exitUnlockedClip, 1.0f);
        public void PlayExitLockedDenialSound() => PlaySFX(exitLockedDenialClip, 0.8f);

        // ==========================================
        // 4. MISSION & GAMEPLAY AUDIO
        // ==========================================
        public void PlayMissionStartupSound() => PlaySFX(missionStartupClip, 0.85f);
        public void PlayMissionCompleteSound() => PlaySFX(missionCompleteClip, 1.0f);
        public void PlayMissionFailedSound() => PlaySFX(missionFailedClip, 0.95f);
        public void PlayNoCapsulesSound() => PlaySFX(capsuleLaunchDeniedClip, 0.9f);
        public void PlayMissionResetSound() => PlaySFX(missionResetClip, 0.75f);

        public void PlayCountdownWarningSound(bool urgent = false)
        {
            if (!SettingsManager.CountdownAudioEnabled) return;
            PlaySFX(urgent ? countdownUrgentClip : countdownWarningClip, 0.9f);
        }

        // ==========================================
        // 5. UI & AMBIENCE AUDIO
        // ==========================================
        public void PlayUISound() => PlayUI(uiClickClip, 0.7f);
        public void PlayPauseSound() => PlayUI(uiPauseClip, 0.75f);
        public void PlayResumeSound() => PlayUI(uiResumeClip, 0.75f);

        public void SetThrustActive(bool active)
        {
            if (thrustSource == null) return;
            if (active && !thrustSource.isPlaying && !SettingsManager.IsMuted) thrustSource.Play();
            else if (!active && thrustSource.isPlaying) thrustSource.Stop();
        }

        // Backward compatibility forwarding aliases
        public void PlayLaunch() => PlayLaunchSound();
        public void PlayRingPass() => PlayRelaySound(1);
        public void PlayImpact() => PlayCollisionSound(5.0f, false);
        public void PlayRelayActivated() => PlayRelaySound(1);
        public void PlayScoreReward() => PlayScoreRewardSound();
        public void PlayExitUnlocked() => PlayExitUnlockedSound();
        public void PlayMissionLocked() => PlayExitLockedDenialSound();
        public void PlayCountdownWarning(bool urgent = false) => PlayCountdownWarningSound(urgent);
        public void PlayWarning() => PlayCountdownWarningSound(false);
        public void PlayFailure() => PlayMissionFailedSound();
        public void PlayVictory() => PlayMissionCompleteSound();
        public void PlayButtonClick() => PlayUISound();
        public void PlayPause() => PlayPauseSound();
        public void PlayResume() => PlayResumeSound();
        public void SetProjectileInFlight(bool inFlight) => SetCapsuleInFlight(inFlight);

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

        // ==========================================
        // PROCEDURAL SYNTHESIS FALLBACK UTILITIES
        // ==========================================
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
                float decay = Mathf.Exp(-t * 8f);
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
                float f2 = baseFreq * 1.5f;
                float f3 = baseFreq * 1.25f;
                float val = Mathf.Sin(2f * Mathf.PI * f1 * (i / (float)sampleRate)) * 0.4f
                          + Mathf.Sin(2f * Mathf.PI * f2 * (i / (float)sampleRate)) * 0.3f
                          + Mathf.Sin(2f * Mathf.PI * f3 * (i / (float)sampleRate)) * 0.3f;
                float fade = Mathf.Sin(t * Mathf.PI);
                data[i] = val * fade * 0.6f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
