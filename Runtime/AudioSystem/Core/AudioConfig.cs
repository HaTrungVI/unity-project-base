using UnityEngine;

namespace ProjectBase.Audio
{
    [CreateAssetMenu(fileName = "AudioConfig", menuName = "ProjectBase/Audio/Audio Config")]
    public class AudioConfig : ScriptableObject
    {
        [Header("Defaults")]
        [Range(0f, 1f)] [SerializeField] private float _defaultMusicVolume = 0.7f;
        [Range(0f, 1f)] [SerializeField] private float _defaultSFXVolume = 1f;

        [Header("SFX Pool")]
        [SerializeField] private int _maxConcurrentSFX = 8;
        [SerializeField] private int _preWarmSFXSources = 4;

        [Header("Music")]
        [SerializeField] private float _defaultFadeDuration = 0.5f;

        public float DefaultMusicVolume => _defaultMusicVolume;
        public float DefaultSFXVolume => _defaultSFXVolume;
        public int MaxConcurrentSFX => _maxConcurrentSFX;
        public int PreWarmSFXSources => _preWarmSFXSources;
        public float DefaultFadeDuration => _defaultFadeDuration;
    }
}
