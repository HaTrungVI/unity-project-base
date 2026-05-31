using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ProjectBase.Common.Patterns
{
    public interface IPoolManager
    {
        UniTask WarmPool<T>(AssetReference prefabRef, int count, CancellationToken ct = default)
            where T : Component, IPoolable;

        UniTask<T> Spawn<T>(AssetReference prefabRef, Vector3 position, Quaternion rotation,
            Transform parent = null, CancellationToken ct = default) where T : Component, IPoolable;

        void Despawn<T>(T instance) where T : Component, IPoolable;

        void ReleasePool(AssetReference prefabRef);

        void ReleaseAllPools();
    }
}
