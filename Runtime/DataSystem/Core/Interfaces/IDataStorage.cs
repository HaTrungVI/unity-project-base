using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Data.Core
{
    public interface IDataStorage
    {
        UniTask SaveAsync(string key, byte[] data, CancellationToken ct = default);
        UniTask<byte[]> LoadAsync(string key, CancellationToken ct = default);
        UniTask<bool> ExistsAsync(string key, CancellationToken ct = default);
        UniTask DeleteAsync(string key, CancellationToken ct = default);
    }
}
