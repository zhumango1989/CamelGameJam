using UnityEngine;
using System.Collections.Generic;
using GameJam.Core;

namespace GameJam.Audio
{
    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private int sfxPoolSize = 10;

        [Header("Volume")]
        [SerializeField, Range(0, 1)] private float masterVolume = 1f;
        [SerializeField, Range(0, 1)] private float bgmVolume = 0.8f;
        [SerializeField, Range(0, 1)] private float sfxVolume = 1f;

        private readonly List<AudioSource> _sfxPool = new();
        private readonly Dictionary<string, AudioClip> _audioCache = new();

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Mathf.Clamp01(value);
                UpdateVolumes();
            }
        }

        public float BGMVolume
        {
            get => bgmVolume;
            set
            {
                bgmVolume = Mathf.Clamp01(value);
                UpdateVolumes();
            }
        }

        public float SFXVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Mathf.Clamp01(value);
                UpdateVolumes();
            }
        }

        protected override void Awake()
        {
            base.Awake();
            InitializeAudioSources();
            InitializeSFXPool();
        }

        private void InitializeAudioSources()
        {
            if (bgmSource == null)
            {
                var bgmObj = new GameObject("BGM Source");
                bgmObj.transform.SetParent(transform);
                bgmSource = bgmObj.AddComponent<AudioSource>();
                bgmSource.loop = true;
                bgmSource.playOnAwake = false;
            }

            if (sfxSource == null)
            {
                var sfxObj = new GameObject("SFX Source");
                sfxObj.transform.SetParent(transform);
                sfxSource = sfxObj.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
            }
        }

        private void InitializeSFXPool()
        {
            for (int i = 0; i < sfxPoolSize; i++)
            {
                var source = CreateSFXSource();
                _sfxPool.Add(source);
            }
        }

        private AudioSource CreateSFXSource()
        {
            var obj = new GameObject("SFX Pool Source");
            obj.transform.SetParent(transform);
            var source = obj.AddComponent<AudioSource>();
            source.playOnAwake = false;
            return source;
        }

        private AudioSource GetAvailableSFXSource()
        {
            foreach (var source in _sfxPool)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }

            var newSource = CreateSFXSource();
            _sfxPool.Add(newSource);
            return newSource;
        }

        private void UpdateVolumes()
        {
            if (bgmSource != null)
            {
                bgmSource.volume = masterVolume * bgmVolume;
            }

            foreach (var source in _sfxPool)
            {
                source.volume = masterVolume * sfxVolume;
            }
        }

        public void PlayBGM(AudioClip clip, float fadeTime = 1f)
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying) return;

            StartCoroutine(FadeBGM(clip, fadeTime));
        }

        private System.Collections.IEnumerator FadeBGM(AudioClip clip, float fadeTime)
        {
            float startVolume = bgmSource.volume;

            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * Time.deltaTime / (fadeTime / 2);
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.Play();

            float targetVolume = masterVolume * bgmVolume;
            while (bgmSource.volume < targetVolume)
            {
                bgmSource.volume += targetVolume * Time.deltaTime / (fadeTime / 2);
                yield return null;
            }

            bgmSource.volume = targetVolume;
        }

        public void StopBGM(float fadeTime = 1f)
        {
            StartCoroutine(FadeOutBGM(fadeTime));
        }

        private System.Collections.IEnumerator FadeOutBGM(float fadeTime)
        {
            float startVolume = bgmSource.volume;

            while (bgmSource.volume > 0)
            {
                bgmSource.volume -= startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }

            bgmSource.Stop();
            bgmSource.volume = startVolume;
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;

            var source = GetAvailableSFXSource();
            source.volume = masterVolume * sfxVolume * volumeScale;
            source.clip = clip;
            source.Play();
        }

        public void PlaySFX(AudioClip clip, Vector3 position, float volumeScale = 1f, float spatialBlend = 1f)
        {
            if (clip == null) return;

            var source = GetAvailableSFXSource();
            source.transform.position = position;
            source.volume = masterVolume * sfxVolume * volumeScale;
            source.spatialBlend = spatialBlend;
            source.clip = clip;
            source.Play();
        }

        public void PlaySFXOneShot(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null) return;
            sfxSource.PlayOneShot(clip, masterVolume * sfxVolume * volumeScale);
        }

        public void PauseBGM() => bgmSource?.Pause();
        public void ResumeBGM() => bgmSource?.UnPause();

        public void SaveSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", masterVolume);
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.Save();
        }

        public void LoadSettings()
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.8f);
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            UpdateVolumes();
        }
    }
}
