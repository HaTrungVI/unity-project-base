using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectBase.Audio
{
    public interface IAudioManager
    {
        float MusicVolume { get; set; }
        float SFXVolume { get; set; }
        bool IsMusicMuted { get; set; }
        bool IsSFXMuted { get; set; }

        void PlaySFX(AssetReference clipRef, float volumeScale = 1f);
        void PlaySFX(AudioClip clip, float volumeScale = 1f);
        UniTask PlayMusicAsync(AssetReference clipRef, bool loop = true,
            float fadeDuration = 0.5f, CancellationToken ct = default);
        void StopMusic(float fadeDuration = 0.5f);
        void PauseMusic();
        void ResumeMusic();
    }
}
