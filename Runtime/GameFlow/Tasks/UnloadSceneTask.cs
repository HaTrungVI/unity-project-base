using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Asset.SceneManagement;
using UnityEngine.AddressableAssets;

namespace ProjectBase.GameFlow.Tasks
{
    public class UnloadSceneTask : IBootstrapTask
    {
        private readonly AssetReference _sceneRef;

        public string Name { get; }
        public float Weight { get; }

        public UnloadSceneTask(AssetReference sceneRef,
            string name = "Cleaning up",
            float weight = 0.3f)
        {
            _sceneRef = sceneRef;
            Name = name;
            Weight = weight;
        }

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            progress?.Report(0f);
            await SceneLoader.Instance.UnloadSceneAsync(_sceneRef, ct);
            progress?.Report(1f);
            await UniTask.Delay(1000, cancellationToken: ct); // Ensure the scene is fully unloaded before proceeding
        }
    }
}
