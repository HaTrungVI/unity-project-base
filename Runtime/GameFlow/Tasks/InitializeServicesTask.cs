using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Asset.Core;
using ProjectBase.Asset.SceneManagement;
using ProjectBase.Common.Patterns;
using ProjectBase.UI.Core;

namespace ProjectBase.GameFlow.Tasks
{
    public class InitializeServicesTask : IBootstrapTask
    {
        public string Name => "Initializing";
        public float Weight => 0.1f;

        public async UniTask Execute(IProgress<float> progress, CancellationToken ct)
        {
            _ = AssetLoader.Instance;
            // _ = UIManager.Instance;
            _ = PoolManager.Instance;
            _ = SceneLoader.Instance;

            progress?.Report(0.5f);
            await UniTask.CompletedTask;
        }
    }
}
