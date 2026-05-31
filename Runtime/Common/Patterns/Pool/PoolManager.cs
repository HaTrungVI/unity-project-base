using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ProjectBase.Common.Patterns
{
    public class PoolManager : MonoSingleton<PoolManager>, IPoolManager
    {
        private readonly Dictionary<string, IObjectPoolInternal> _pools = new();
        private readonly Dictionary<string, AsyncOperationHandle<GameObject>> _prefabHandles = new();
        private readonly Dictionary<Component, string> _instanceToKey = new();
        private readonly List<string> _tempKeys = new();
        private readonly List<Component> _tempComponents = new();

        public async UniTask WarmPool<T>(AssetReference prefabRef, int count,
            CancellationToken ct = default) where T : Component, IPoolable
        {
            var key = prefabRef.RuntimeKey.ToString();
            var pool = await GetOrCreatePool<T>(prefabRef, key, ct);
            pool.Warm(count);
        }

        public async UniTask<T> Spawn<T>(AssetReference prefabRef, Vector3 position,
            Quaternion rotation, Transform parent = null,
            CancellationToken ct = default) where T : Component, IPoolable
        {
            var key = prefabRef.RuntimeKey.ToString();
            var pool = await GetOrCreatePool<T>(prefabRef, key, ct);
            var instance = pool.Get(position, rotation, parent);
            _instanceToKey[instance] = key;
            return instance;
        }

        public void Despawn<T>(T instance) where T : Component, IPoolable
        {
            if (instance == null) return;

            if (!_instanceToKey.TryGetValue(instance, out var key)) return;

            if (_pools.TryGetValue(key, out var pool))
                pool.ReturnUntyped(instance);

            _instanceToKey.Remove(instance);
        }

        public void ReleasePool(AssetReference prefabRef)
        {
            var key = prefabRef.RuntimeKey.ToString();
            ReleasePoolByKey(key);
        }

        public void ReleaseAllPools()
        {
            _tempKeys.Clear();
            _tempKeys.AddRange(_pools.Keys);
            foreach (var key in _tempKeys)
                ReleasePoolByKey(key);
            _tempKeys.Clear();
        }

        private void ReleasePoolByKey(string key)
        {
            if (_pools.TryGetValue(key, out var pool))
            {
                pool.Clear();
                _pools.Remove(key);
            }

            if (_prefabHandles.TryGetValue(key, out var handle))
            {
                if (handle.IsValid())
                    Addressables.Release(handle);
                _prefabHandles.Remove(key);
            }

            _tempComponents.Clear();
            foreach (var kvp in _instanceToKey)
            {
                if (kvp.Value == key)
                    _tempComponents.Add(kvp.Key);
            }
            foreach (var instance in _tempComponents)
                _instanceToKey.Remove(instance);
            _tempComponents.Clear();
        }

        private async UniTask<ObjectPool<T>> GetOrCreatePool<T>(AssetReference prefabRef,
            string key, CancellationToken ct) where T : Component, IPoolable
        {
            if (_pools.TryGetValue(key, out var existing))
                return (ObjectPool<T>)existing;

            var handle = Addressables.LoadAssetAsync<GameObject>(prefabRef);
            var prefab = await handle.ToUniTask(cancellationToken: ct);
            _prefabHandles[key] = handle;

            var component = prefab.GetComponent<T>();
            if (component == null)
                throw new Exception($"[PoolManager] Prefab does not have component {typeof(T).Name}");

            var poolRoot = new GameObject($"Pool_{key}").transform;
            poolRoot.SetParent(transform);

            var pool = new ObjectPool<T>(component, poolRoot);
            _pools[key] = pool;
            return pool;
        }

        protected override void OnDestroy()
        {
            ReleaseAllPools();
            base.OnDestroy();
        }
    }
}
