using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using ProjectBase.Common.Patterns;
using UnityEngine;

namespace ProjectBase.Data.Core
{
    public abstract class DataModuleBase : ScriptableObject, IDataModule
    {
        [Header("Module Settings")]
        [SerializeField] private string _moduleId;
        [SerializeField] private int _version = 1;

        [Header("Sync")]
        [SerializeField] private bool _enableSync;

        [NonSerialized] private IDataStorage _storage;
        [NonSerialized] private IDataSerializer _serializer;
        [NonSerialized] private IDataSyncAdapter _syncAdapter;
        [NonSerialized] private bool _isDirty;
        [NonSerialized] private bool _hasSaveFile;
        [NonSerialized] private bool _isInitialized;
        [NonSerialized] private bool _hasPendingSync;
        [NonSerialized] private readonly SemaphoreSlim _ioLock = new(1, 1);

        public string ModuleId => _moduleId;
        public int Version => _version;
        public bool IsDirty => _isDirty;
        public bool HasSaveFile => _hasSaveFile;
        public bool IsInitialized => _isInitialized;
        public bool SyncEnabled => _enableSync && _syncAdapter != null;
        public bool HasPendingSync => _hasPendingSync;

        protected IDataStorage Storage => _storage;
        protected IDataSerializer Serializer => _serializer;
        protected IDataSyncAdapter SyncAdapter => _syncAdapter;
        protected SemaphoreSlim IoLock => _ioLock;

        public event Action<string> OnDataChanged;
        public event Action<string> OnSaveCompleted;
        public event Action<string, Exception> OnSaveFailed;

        public void Initialize(IDataStorage storage, IDataSerializer serializer)
        {
            _storage = storage;
            _serializer = serializer;
            OnInitialize();
            _isInitialized = true;
        }

        public void SetSyncAdapter(IDataSyncAdapter adapter)
        {
            _syncAdapter = adapter;
        }

        protected abstract void OnInitialize();
        public abstract UniTask LoadAsync(CancellationToken ct = default);
        public abstract UniTask SaveAsync(CancellationToken ct = default);
        public abstract UniTask DeleteSaveAsync(CancellationToken ct = default);
        public abstract void ResetToDefaults();
        public abstract string GetDataJson();
        public abstract void ConfirmSync();
        public abstract void RollbackSync();

        public void MarkDirty()
        {
            _isDirty = true;
            OnDataChanged?.Invoke(_moduleId);
            EventBus.Publish(new DataModuleChangedEvent { ModuleId = _moduleId });
        }

        protected void SetDirty(bool value) => _isDirty = value;
        protected void SetHasSaveFile(bool value) => _hasSaveFile = value;
        protected void SetPendingSync(bool value) => _hasPendingSync = value;

        protected void NotifySaveCompleted()
        {
            OnSaveCompleted?.Invoke(_moduleId);
            EventBus.Publish(new DataModuleSavedEvent { ModuleId = _moduleId });
        }

        protected void NotifySaveFailed(Exception e)
        {
            OnSaveFailed?.Invoke(_moduleId, e);
            EventBus.Publish(new DataModuleSaveFailedEvent { ModuleId = _moduleId, Error = e });
        }

        protected void EnsureInitialized()
        {
            if (!_isInitialized)
                throw new InvalidOperationException(
                    $"DataModule '{_moduleId}' not initialized. Call Initialize() first.");
        }
    }

    public abstract class DataModule<T> : DataModuleBase where T : class, new()
    {
        [Header("Default Data")]
        [SerializeField] private T _defaultData = new();

        [Header("Current Data")]
        [SerializeField] private T _currentData = new();

        [NonSerialized] private T _runtimeData;
        [NonSerialized] private T _snapshot;

        protected T Data => _runtimeData ?? _currentData;

        protected override void OnInitialize()
        {
            _runtimeData = CloneData(_currentData);
        }

        public override async UniTask LoadAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            await IoLock.WaitAsync(ct);
            try
            {
                if (SyncEnabled && SyncAdapter.IsOnline)
                {
                    await LoadFromServer(ct);
                    return;
                }
                await LoadFromLocal(ct);
            }
            finally
            {
                IoLock.Release();
            }
        }

        private async UniTask LoadFromServer(CancellationToken ct)
        {
            try
            {
                var json = await SyncAdapter.LoadFromServerAsync(ModuleId, ct);
                if (!string.IsNullOrEmpty(json))
                {
                    _runtimeData = JsonUtility.FromJson<T>(json);
                    SetHasSaveFile(true);
                    SetDirty(false);
                    await SaveToLocalQuiet(ct);
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DataModule] Server load failed for '{ModuleId}', falling back to local: {e.Message}");
            }
            await LoadFromLocal(ct);
        }

        private async UniTask LoadFromLocal(CancellationToken ct)
        {
            try
            {
                var exists = await Storage.ExistsAsync(ModuleId, ct);
                if (!exists)
                {
                    _runtimeData = CloneData(_defaultData);
                    SetHasSaveFile(false);
                    return;
                }

                var bytes = await Storage.LoadAsync(ModuleId, ct);
                _runtimeData = Serializer.Deserialize<T>(bytes);
                SetHasSaveFile(true);
                SetDirty(false);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataModule] Failed to load '{ModuleId}': {e.Message}");
                await TryBackupCorruptedFile(ct);
                _runtimeData = CloneData(_defaultData);
                SetHasSaveFile(false);
            }
        }

        public override async UniTask SaveAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            await IoLock.WaitAsync(ct);
            try
            {
                var bytes = Serializer.Serialize(_runtimeData);
                await Storage.SaveAsync(ModuleId, bytes, ct);
                SetDirty(false);
                SetHasSaveFile(true);
                NotifySaveCompleted();

                if (SyncEnabled && SyncAdapter.IsOnline)
                    SyncToServerBackground(ct);
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataModule] Failed to save '{ModuleId}': {e.Message}");
                NotifySaveFailed(e);
            }
            finally
            {
                IoLock.Release();
            }
        }

        private void SyncToServerBackground(CancellationToken ct)
        {
            SyncToServerAsync(ct).Forget();
        }

        private async UniTaskVoid SyncToServerAsync(CancellationToken ct)
        {
            try
            {
                var json = JsonUtility.ToJson(_runtimeData);
                var success = await SyncAdapter.SaveToServerAsync(ModuleId, json, ct);
                if (success)
                {
                    EventBus.Publish(new DataSyncCompletedEvent { ModuleId = ModuleId });
                }
                else
                {
                    SetPendingSync(true);
                    EventBus.Publish(new DataSyncFailedEvent
                        { ModuleId = ModuleId, Reason = "Server rejected" });
                }
            }
            catch (Exception e)
            {
                SetPendingSync(true);
                Debug.LogWarning($"[DataModule] Sync failed for '{ModuleId}': {e.Message}");
                EventBus.Publish(new DataSyncFailedEvent
                    { ModuleId = ModuleId, Reason = e.Message });
            }
        }

        private async UniTask SaveToLocalQuiet(CancellationToken ct)
        {
            try
            {
                var bytes = Serializer.Serialize(_runtimeData);
                await Storage.SaveAsync(ModuleId, bytes, ct);
                SetHasSaveFile(true);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[DataModule] Local cache save failed for '{ModuleId}': {e.Message}");
            }
        }

        // --- Optimistic Update ---

        public void ApplyOptimistic(Action<T> mutation)
        {
            EnsureInitialized();
            if (_snapshot == null)
                _snapshot = CloneData(_runtimeData);
            SetPendingSync(true);
            mutation(_runtimeData);
            MarkDirty();
        }

        public override void ConfirmSync()
        {
            _snapshot = null;
            SetPendingSync(false);
        }

        public override void RollbackSync()
        {
            if (_snapshot != null)
            {
                _runtimeData = _snapshot;
                _snapshot = null;
            }
            SetPendingSync(false);
            EventBus.Publish(new DataRollbackEvent { ModuleId = ModuleId });
        }

        // --- Standard operations ---

        public override async UniTask DeleteSaveAsync(CancellationToken ct = default)
        {
            EnsureInitialized();
            await Storage.DeleteAsync(ModuleId, ct);
            SetHasSaveFile(false);
            _runtimeData = CloneData(_defaultData);
            _snapshot = null;
            SetDirty(false);
        }

        public override void ResetToDefaults()
        {
            _runtimeData = CloneData(_defaultData);
            _snapshot = null;
            SetDirty(true);
        }

        public override string GetDataJson()
        {
            var data = _runtimeData ?? _currentData;
            return JsonUtility.ToJson(data, true);
        }

        protected virtual void OnMigrate(int fromVersion, T data) { }

        private T CloneData(T source)
        {
            var json = JsonUtility.ToJson(source);
            return JsonUtility.FromJson<T>(json);
        }

        private async UniTask TryBackupCorruptedFile(CancellationToken ct)
        {
            try
            {
                var exists = await Storage.ExistsAsync(ModuleId, ct);
                if (!exists) return;

                var data = await Storage.LoadAsync(ModuleId, ct);
                var backupKey = ModuleId + ".bak";
                await Storage.SaveAsync(backupKey, data, ct);
                await Storage.DeleteAsync(ModuleId, ct);
                Debug.LogWarning($"[DataModule] Corrupted file backed up as '{backupKey}.dat'");
            }
            catch (Exception e)
            {
                Debug.LogError($"[DataModule] Failed to backup corrupted file: {e.Message}");
            }
        }
    }
}
