using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Data.Core
{
    public class PlatformFileSystem : IFileSystem
    {
        public async UniTask WriteAllBytesAsync(string path, byte[] data, CancellationToken ct = default)
        {
            var tempPath = path + ".tmp";
            await UniTask.RunOnThreadPool(() =>
            {
                File.WriteAllBytes(tempPath, data);

                if (File.Exists(path))
                    File.Delete(path);

                File.Move(tempPath, path);
            }, cancellationToken: ct);
        }

        public async UniTask<byte[]> ReadAllBytesAsync(string path, CancellationToken ct = default)
        {
            return await UniTask.RunOnThreadPool(
                () => File.ReadAllBytes(path),
                cancellationToken: ct);
        }

        public bool Exists(string path) => File.Exists(path);

        public void Delete(string path)
        {
            if (File.Exists(path))
                File.Delete(path);

            var tmpPath = path + ".tmp";
            if (File.Exists(tmpPath))
                File.Delete(tmpPath);
        }

        public void EnsureDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
        }
    }
}
