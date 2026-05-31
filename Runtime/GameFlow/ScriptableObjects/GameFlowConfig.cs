using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectBase.GameFlow
{
    [CreateAssetMenu(fileName = "GameFlowConfig", menuName = "ProjectBase/Game Flow Config")]
    public class GameFlowConfig : ScriptableObject
    {
        [Header("Scenes")]
        [SerializeField] private AssetReference _splashScene;
        [SerializeField] private AssetReference _menuScene;
        [SerializeField] private AssetReference _gameplayScene;

        [Header("Splash Settings")]
        [SerializeField] private float _minimumSplashTime = 2f;

        [Header("Preload")]
        [SerializeField] private List<AssetReference> _preloadAssets = new();

        public AssetReference SplashScene => _splashScene;
        public AssetReference MenuScene => _menuScene;
        public AssetReference GameplayScene => _gameplayScene;
        public float MinimumSplashTime => _minimumSplashTime;
        public IReadOnlyList<AssetReference> PreloadAssets => _preloadAssets;
    }
}
