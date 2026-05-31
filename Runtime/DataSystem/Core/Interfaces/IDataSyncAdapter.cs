using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Data.Core
{
    public interface IDataSyncAdapter
    {
        bool IsOnline { get; }
        UniTask<string> LoadFromServerAsync(string moduleId, CancellationToken ct = default);
        UniTask<bool> SaveToServerAsync(string moduleId, string jsonData, CancellationToken ct = default);
    }
}
