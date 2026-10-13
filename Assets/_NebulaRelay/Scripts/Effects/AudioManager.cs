using UnityEngine;

namespace NebulaRelay.Effects
{
    /// <summary>
    /// AudioManager provides zero-dependency procedural audio synthesis for all gameplay actions.
    /// Generates sci-fi tones on Awake so no external audio files are required.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource thrustSource;
        private AudioSource ambientSource;

        private AudioClip launchClip;
        private AudioClip ringPassClip;
        private AudioClip impactClip;
        private AudioClip relayClip;
        private AudioClip warningClip;
        private AudioClip failureClip;
        private AudioClip thrustClip;
        private AudioClip ambientClip;

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

            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;

            thrustSource = gameObject.AddComponent<AudioSource>();
            thrustSource.loop = true;
            thrustSource.playOnAwake = false;
            thrustSource.volume = 0.25f;

            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.loop = true;
            ambientSource.playOnAwake = false;
            ambientSource.volume = 0.15f;

            GenerateProceduralAudioClips();

            if (ambientClip != null)
            {
                ambientSource.clip = ambientClip;
                ambientSource.Play();
            }
        }

        private void GenerateProceduralAudioClips()
        {
            launchClip = CreateChirpClip("SFX_Launch", 850f, 320f, 0.18f);
            ringPassClip = CreateTwoToneClip("SFX_RingPass", 784f, 1046f, 0.22f);
            impactClip = CreateBellClip("SFX_Impact", 523f, 0.35f);
            relayClip = CreateArpeggioClip("SFX_RelayActive", new float[] { 523f, 659f, 784f, 1046f }, 0.6f);
            warningClip = CreateChirpClip("SFX_Warning", 400f, 600f, 0.15f);
            failureClip = CreateChirpClip("SFX_Failure", 450f, 150f, 0.5f);
            thrustClip = CreateRumbleClip("SFX_Thrust", 75f, 1.0f);
            ambientClip = CreateAmbientPadClip("SFX_CosmicAmbient", 110f, 4.0f);

            thrustSource.clip = thrustClip;
        }

        public void PlayLaunch() => PlayClip(launchClip, 0.8f);
        public void PlayRingPass() => PlayClip(ringPassClip, 0.9f);
        public void PlayImpact() => PlayClip(impactClip, 0.85f);
        public void PlayRelayActivated() => PlayClip(relayClip, 1.0f);
        public void PlayWarning() => PlayClip(warningClip, 0.75f);
        public void PlayFailure() => PlayClip(failureClip, 0.9f);

        public void SetThrustActive(bool active)
        {
            if (thrustSource == null) return;
            if (active && !thrustSource.isPlaying) thrustSource.Play();
            else if (!active && thrustSource.isPlaying) thrustSource.Stop();
        }

        private void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (clip != null && sfxSource != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

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
            int noteLength = samples / freqs.Length;

            for (int i = 0; i < samples; i++)
            {
                int noteIdx = Mathf.Clamp(i / noteLength, 0, freqs.Length - 1);
                float freq = freqs[noteIdx];
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
                float noise = (Random.value * 2f - 1f) * 0.2f;
                float wave = Mathf.Sin(2f * Mathf.PI * freq * (i / (float)sampleRate)) * 0.8f;
                data[i] = wave + noise;
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
                float timeSec = i / (float)sampleRate;
                // Layer fundamental + fifth + octave with slow chorus modulation
                float lfo = 1f + 0.05f * Mathf.Sin(2f * Mathf.PI * 0.25f * timeSec);
                float w1 = Mathf.Sin(2f * Mathf.PI * baseFreq * lfo * timeSec) * 0.4f;
                float w2 = Mathf.Sin(2f * Mathf.PI * (baseFreq * 1.5f) * timeSec) * 0.25f;
                float w3 = Mathf.Sin(2f * Mathf.PI * (baseFreq * 2.0f) * timeSec) * 0.15f;
                // Smooth loop crossfade at boundaries
                float loopFade = Mathf.Sin(t * Mathf.PI);
                data[i] = (w1 + w2 + w3) * loopFade * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
