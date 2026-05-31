using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Asset.SceneManagement;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectBase.GameFlow
{
    public class GameBootstrapper : MonoBehaviour
    {
        [SerializeField] private GameFlowConfig _config;

        private async UniTaskVoid Start()
        {
            var ct = this.GetCancellationTokenOnDestroy();

            await Addressables.InitializeAsync().ToUniTask(cancellationToken: ct);
            await SceneLoader.Instance.LoadSceneAsync(_config.SplashScene, ct: ct);
        }
    }
}
