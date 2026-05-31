using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Common.Patterns;
using UnityEngine;

namespace ProjectBase.Data.Core
{
    [CreateAssetMenu(fileName = "DataRegistry", menuName = "ProjectBase/Data Registry")]
    public class DataRegistry : ScriptableObject, IDataRegistry
    {
        [Header("Storage Configuration")]
        [SerializeField] private DataStorageConfig _storageConfig;

        [Header("Data Modules")]
        [SerializeField] private List<ScriptableObject> _modules = new();

        private readonly List<IDataModule> _resolvedModules = new();
        private bool _isInitialized;

        public IReadOnlyList<IDataModule> Modules => _resolvedModules;
        public bool IsInitialized => _isInitialized;
        public DataStorageConfig StorageConfig => _storageConfig;

        public void Initialize(IDataSyncAdapter syncAdapter = null)
        {
            if (_isInitialized) return;

            _resolvedModules.Clear();
            var storage = _storageConfig.CreateStorage();
            var serializer = _storageConfig.CreateSerializer();

            for (var i = 0; i < _modules.Count; i++)
            {
                if (_modules[i] is IDataModule module)
                {
                    module.Initialize(storage, serializer);
                    if (syncAdapter != null)
                        module.SetSyncAdapter(syncAdapter);
                    _resolvedModules.Add(module);
                }
                else if (_modules[i] != null)
                {
                    Debug.LogWarning(
                        $"[DataRegistry] Module '{_modules[i].name}' does not implement IDataModule");
                }
            }

            _isInitialized = true;
        }

        public async UniTask LoadAllAsync(CancellationToken ct = default)
        {
            if (!_isInitialized) Initialize();

            for (var i = 0; i < _resolvedModules.Count; i++)
                await _resolvedModules[i].LoadAsync(ct);

            EventBus.Publish(new DataRegistryLoadedEvent());
        }

        public async UniTask SaveAllAsync(CancellationToken ct = default)
        {
            for (var i = 0; i < _resolvedModules.Count; i++)
                await _resolvedModules[i].SaveAsync(ct);

            EventBus.Publish(new DataRegistrySavedEvent());
        }

        public async UniTask FlushDirtyAsync(CancellationToken ct = default)
        {
            for (var i = 0; i < _resolvedModules.Count; i++)
            {
                if (_resolvedModules[i].IsDirty)
                    await _resolvedModules[i].SaveAsync(ct);
            }
        }

#if UNITY_EDITOR
        public List<ScriptableObject> GetRawModules() => _modules;

        public void AddModule(ScriptableObject module)
        {
            if (module is IDataModule && !_modules.Contains(module))
            {
                _modules.Add(module);
                UnityEditor.EditorUtility.SetDirty(this);
            }
        }

        public void RemoveModule(ScriptableObject module)
        {
            _modules.Remove(module);
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
