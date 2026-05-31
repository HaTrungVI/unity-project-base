using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Common.Patterns;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace ProjectBase.Asset.SceneManagement
{
    public struct SceneLoadStartedEvent
    {
        public string SceneKey;
    }

    public struct SceneLoadCompletedEvent
    {
        public string SceneKey;
    }

    public class SceneLoader : MonoSingleton<SceneLoader>, ISceneLoader
    {
        private readonly Dictionary<string, AsyncOperationHandle<SceneInstance>> _loadedScenes = new();

        public async UniTask LoadSceneAsync(AssetReference sceneRef,
            LoadSceneMode mode = LoadSceneMode.Single,
            IProgress<float> progress = null,
            CancellationToken ct = default)
        {
            var key = sceneRef.RuntimeKey.ToString();
            EventBus.Publish(new SceneLoadStartedEvent { SceneKey = key });

            var handle = Addressables.LoadSceneAsync(sceneRef, mode, activateOnLoad: false);

            while (!handle.IsDone)
            {
                ct.ThrowIfCancellationRequested();
                progress?.Report(handle.PercentComplete);
                await UniTask.Yield(ct);
            }

            if (handle.Status != AsyncOperationStatus.Succeeded)
                throw new Exception($"[SceneLoader] Failed to load scene: {key}");

            progress?.Report(1f);

            await handle.Result.ActivateAsync().ToUniTask(cancellationToken: ct);

            if (mode == LoadSceneMode.Additive)
                SceneManager.SetActiveScene(handle.Result.Scene);

            _loadedScenes[key] = handle;
            EventBus.Publish(new SceneLoadCompletedEvent { SceneKey = key });
        }

        public async UniTask UnloadSceneAsync(AssetReference sceneRef,
            CancellationToken ct = default)
        {
            var key = sceneRef.RuntimeKey.ToString();

            if (!_loadedScenes.TryGetValue(key, out var handle)) return;

            await Addressables.UnloadSceneAsync(handle).ToUniTask(cancellationToken: ct);
            _loadedScenes.Remove(key);
        }

        protected override void OnDestroy()
        {
            foreach (var handle in _loadedScenes.Values)
            {
                if (handle.IsValid())
                    Addressables.UnloadSceneAsync(handle);
            }
            _loadedScenes.Clear();

            base.OnDestroy();
        }
    }
}
