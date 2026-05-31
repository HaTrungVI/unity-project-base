using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Common.Patterns;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectBase.Asset.Core
{
    public class AssetLoader : MonoSingleton<AssetLoader>, IAssetLoader
    {
        private readonly Dictionary<Object, AsyncOperationHandle> _handleCache = new();
        private readonly Dictionary<GameObject, AsyncOperationHandle<GameObject>> _instanceHandles = new();

        public async UniTask<T> LoadAssetAsync<T>(AssetReference reference,
            CancellationToken ct = default) where T : Object
        {
            var handle = reference.LoadAssetAsync<T>();
            var result = await handle.ToUniTask(cancellationToken: ct);
            _handleCache[result] = handle;
            return result;
        }

        public async UniTask<T> LoadAssetAsync<T>(string address,
            CancellationToken ct = default) where T : Object
        {
            var handle = Addressables.LoadAssetAsync<T>(address);
            var result = await handle.ToUniTask(cancellationToken: ct);
            _handleCache[result] = handle;
            return result;
        }

        public async UniTask<GameObject> InstantiateAsync(AssetReference reference,
            Transform parent = null, CancellationToken ct = default)
        {
            var handle = Addressables.InstantiateAsync(reference, parent);
            var result = await handle.ToUniTask(cancellationToken: ct);
            _instanceHandles[result] = handle;
            return result;
        }

        public void Release<T>(T asset) where T : Object
        {
            if (asset == null) return;

            if (_handleCache.TryGetValue(asset, out var handle))
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
                _handleCache.Remove(asset);
            }
        }

        public void ReleaseInstance(GameObject instance)
        {
            if (instance == null) return;

            if (_instanceHandles.TryGetValue(instance, out _))
            {
                Addressables.ReleaseInstance(instance);
                _instanceHandles.Remove(instance);
            }
        }

        protected override void OnDestroy()
        {
            foreach (var handle in _handleCache.Values)
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
            }
            _handleCache.Clear();

            foreach (var kvp in _instanceHandles)
            {
                if (kvp.Key != null)
                    Addressables.ReleaseInstance(kvp.Key);
            }
            _instanceHandles.Clear();

            base.OnDestroy();
        }
    }
}
