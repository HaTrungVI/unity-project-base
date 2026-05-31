using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectBase.Asset.Core
{
    public interface IAssetLoader
    {
        UniTask<T> LoadAssetAsync<T>(AssetReference reference, CancellationToken ct = default)
            where T : Object;

        UniTask<T> LoadAssetAsync<T>(string address, CancellationToken ct = default)
            where T : Object;

        UniTask<GameObject> InstantiateAsync(AssetReference reference,
            Transform parent = null, CancellationToken ct = default);

        void Release<T>(T asset) where T : Object;

        void ReleaseInstance(GameObject instance);
    }
}
