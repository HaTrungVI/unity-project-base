using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Data.Core
{
    public interface IDataRegistry
    {
        IReadOnlyList<IDataModule> Modules { get; }
        bool IsInitialized { get; }

        void Initialize(IDataSyncAdapter syncAdapter = null);
        UniTask LoadAllAsync(CancellationToken ct = default);
        UniTask SaveAllAsync(CancellationToken ct = default);
        UniTask FlushDirtyAsync(CancellationToken ct = default);
    }
}
