using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using ProjectBase.Common.Patterns;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectBase.Audio
{
    public class AudioManager : MonoSingleton<AudioManager>, IAudioManager
    {
        [SerializeField] private AudioConfig _config;

        private AudioSource _musicSource;
        private readonly List<AudioSource> _sfxPool = new();
        private readonly Dictionary<string, AsyncOperationHandle<AudioClip>> _loadedClips = new();
        private Tween _musicFadeTween;
        private float _musicVolume;
        private float _sfxVolume;
        private bool _isMusicMuted;
        private bool _isSFXMuted;

        public float MusicVolume
        {
            get => _musicVolume;
            set
            {
                _musicVolume = Mathf.Clamp01(value);
                if (_musicSource != null && !_isMusicMuted)
                    _musicSource.volume = _musicVolume;
            }
        }

        public float SFXVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        public bool IsMusicMuted
        {
            get => _isMusicMuted;
            set
            {
                _isMusicMuted = value;
                if (_musicSource != null)
                    _musicSource.mute = value;
            }
        }

        public bool IsSFXMuted
        {
            get => _isSFXMuted;
            set => _isSFXMuted = value;
        }

        protected override void OnInitialized()
        {
            if (_config == null)
            {
                Debug.LogError("[AudioManager] AudioConfig is not assigned.");
                return;
            }

            _musicVolume = _config.DefaultMusicVolume;
            _sfxVolume = _config.DefaultSFXVolume;

            _musicSource = gameObject.AddComponent<AudioSource>();
            _musicSource.playOnAwake = false;
            _musicSource.loop = true;
            _musicSource.volume = _musicVolume;

            for (int i = 0; i < _config.PreWarmSFXSources; i++)
                _sfxPool.Add(CreateSFXSource());
        }

        public void PlaySFX(AudioClip clip, float volumeScale = 1f)
        {
            if (clip == null || _isSFXMuted) return;
            var source = GetAvailableSFXSource();
            source.clip = clip;
            source.volume = _sfxVolume * volumeScale;
            source.Play();
        }

        public void PlaySFX(AssetReference clipRef, float volumeScale = 1f)
        {
            if (clipRef == null || !clipRef.RuntimeKeyIsValid() || _isSFXMuted) return;
            LoadAndPlaySFX(clipRef, volumeScale).Forget();
        }

        private async UniTaskVoid LoadAndPlaySFX(AssetReference clipRef, float volumeScale)
        {
            try
            {
                var key = clipRef.RuntimeKey.ToString();
                AudioClip clip;

                if (_loadedClips.TryGetValue(key, out var handle) && handle.IsValid())
                {
                    clip = handle.Result;
                }
                else
                {
                    var newHandle = Addressables.LoadAssetAsync<AudioClip>(clipRef);
                    clip = await newHandle.ToUniTask();
                    _loadedClips[key] = newHandle;
                }

                PlaySFX(clip, volumeScale);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[AudioManager] Failed to load SFX: {e.Message}");
            }
        }

        public async UniTask PlayMusicAsync(AssetReference clipRef, bool loop = true,
            float fadeDuration = 0.5f, CancellationToken ct = default)
        {
            if (clipRef == null || !clipRef.RuntimeKeyIsValid()) return;

            var key = clipRef.RuntimeKey.ToString();
            AudioClip clip;

            if (_loadedClips.TryGetValue(key, out var handle) && handle.IsValid())
            {
                clip = handle.Result;
            }
            else
            {
                var newHandle = Addressables.LoadAssetAsync<AudioClip>(clipRef);
                clip = await newHandle.ToUniTask(cancellationToken: ct);
                _loadedClips[key] = newHandle;
            }

            if (_musicSource.isPlaying)
            {
                await FadeMusicVolume(0f, fadeDuration, ct);
                _musicSource.Stop();
            }

            _musicSource.clip = clip;
            _musicSource.loop = loop;
            _musicSource.volume = 0f;
            _musicSource.Play();

            await FadeMusicVolume(_isMusicMuted ? 0f : _musicVolume, fadeDuration, ct);
        }

        public void StopMusic(float fadeDuration = 0.5f)
        {
            if (!_musicSource.isPlaying) return;
            StopMusicAsync(fadeDuration).Forget();
        }

        private async UniTaskVoid StopMusicAsync(float fadeDuration)
        {
            await FadeMusicVolume(0f, fadeDuration, destroyCancellationToken);
            _musicSource.Stop();
        }

        public void PauseMusic()
        {
            if (_musicSource.isPlaying)
                _musicSource.Pause();
        }

        public void ResumeMusic()
        {
            if (!_musicSource.isPlaying && _musicSource.clip != null)
                _musicSource.UnPause();
        }

        private UniTask FadeMusicVolume(float target, float duration, CancellationToken ct)
        {
            _musicFadeTween?.Kill();
            if (duration <= 0f)
            {
                _musicSource.volume = target;
                return UniTask.CompletedTask;
            }

            var tcs = new UniTaskCompletionSource();
            _musicFadeTween = DOTween.To(
                () => _musicSource.volume,
                x => _musicSource.volume = x,
                target,
                duration
            ).SetAutoKill(true).OnComplete(() => tcs.TrySetResult());

            ct.Register(() =>
            {
                _musicFadeTween?.Kill();
                tcs.TrySetCanceled();
            });

            return tcs.Task;
        }

        private AudioSource GetAvailableSFXSource()
        {
            for (int i = 0; i < _sfxPool.Count; i++)
            {
                if (!_sfxPool[i].isPlaying)
                    return _sfxPool[i];
            }

            if (_sfxPool.Count < _config.MaxConcurrentSFX)
            {
                var source = CreateSFXSource();
                _sfxPool.Add(source);
                return source;
            }

            return _sfxPool[0];
        }

        private AudioSource CreateSFXSource()
        {
            var source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = false;
            return source;
        }

        protected override void OnDestroy()
        {
            _musicFadeTween?.Kill();
            foreach (var kvp in _loadedClips)
            {
                if (kvp.Value.IsValid())
                    Addressables.Release(kvp.Value);
            }
            _loadedClips.Clear();
            base.OnDestroy();
        }
    }
}
