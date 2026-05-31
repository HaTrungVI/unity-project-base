using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Data.Core
{
    public interface IFileSystem
    {
        UniTask WriteAllBytesAsync(string path, byte[] data, CancellationToken ct = default);
        UniTask<byte[]> ReadAllBytesAsync(string path, CancellationToken ct = default);
        bool Exists(string path);
        void Delete(string path);
        void EnsureDirectory(string directoryPath);
    }
}
