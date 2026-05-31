using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Data.Core
{
    public interface IDataModule
    {
        string ModuleId { get; }
        int Version { get; }
        bool IsDirty { get; }
        bool HasSaveFile { get; }
        bool IsInitialized { get; }
        bool SyncEnabled { get; }
        bool HasPendingSync { get; }

        UniTask SaveAsync(CancellationToken ct = default);
        UniTask LoadAsync(CancellationToken ct = default);
        UniTask DeleteSaveAsync(CancellationToken ct = default);
        void ResetToDefaults();
        void MarkDirty();
        void Initialize(IDataStorage storage, IDataSerializer serializer);
        void SetSyncAdapter(IDataSyncAdapter adapter);
        string GetDataJson();

        void ConfirmSync();
        void RollbackSync();

        event Action<string> OnDataChanged;
        event Action<string> OnSaveCompleted;
        event Action<string, Exception> OnSaveFailed;
    }
}
