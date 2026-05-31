using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ProjectBase.Data.Core
{
    public class DataAutoSaver : MonoBehaviour
    {
        [SerializeField] private DataRegistry _registry;
        [SerializeField] private float _autoSaveIntervalSeconds = 5f;
        [SerializeField] private bool _loadOnStart = true;

        private CancellationTokenSource _cts;

        public DataRegistry Registry => _registry;

        private async UniTaskVoid Start()
        {
            if (_registry == null)
            {
                Debug.LogError("[DataAutoSaver] DataRegistry is not assigned.");
                return;
            }

            _cts = new CancellationTokenSource();
            _registry.Initialize();

            if (_loadOnStart)
                await _registry.LoadAllAsync(_cts.Token);

            RunAutoSaveLoop(_cts.Token).Forget();
        }

        private async UniTaskVoid RunAutoSaveLoop(CancellationToken ct)
        {
            var intervalMs = (int)(_autoSaveIntervalSeconds * 1000);

            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(intervalMs, cancellationToken: ct);
                await _registry.FlushDirtyAsync(ct);
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && _registry != null && _registry.IsInitialized)
                _registry.FlushDirtyAsync().Forget();
        }

        private void OnApplicationQuit()
        {
            if (_registry != null && _registry.IsInitialized)
                _registry.FlushDirtyAsync().Forget();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
