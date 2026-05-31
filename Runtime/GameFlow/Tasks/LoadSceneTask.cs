using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Asset.SceneManagement;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace ProjectBase.GameFlow.Tasks
{
    public class LoadSceneTask : IBootstrapTask
    {
        private readonly AssetReference _sceneRef;
        private readonly LoadSceneMode _mode;

        public string Name { get; }
        public float Weight { get; }

        public LoadSceneTask(AssetReference sceneRef,
            LoadSceneMode mode = LoadSceneMode.Single,
            string name = "Loading scene",
            float weight = 0.3f)
        {
            _sceneRef = sceneRef;
            _mode = mode;
            Name = name;
            Weight = weight;
        }

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            progress?.Report(0f);
            await SceneLoader.Instance.LoadSceneAsync(_sceneRef, _mode, progress, ct);
            progress?.Report(1f);
            await UniTask.Delay(1000, cancellationToken: ct); // Ensure the scene is fully loaded before proceeding
        }
    }
}
