using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace ProjectBase.Asset.SceneManagement
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(AssetReference sceneRef,
            LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null,
            CancellationToken ct = default);

        UniTask UnloadSceneAsync(AssetReference sceneRef,
            CancellationToken ct = default);
    }
}
