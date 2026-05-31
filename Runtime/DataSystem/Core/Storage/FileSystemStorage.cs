using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectBase.Data.Core
{
    public class FileSystemStorage : IDataStorage
    {
        private readonly IFileSystem _fileSystem;
        private readonly string _basePath;

        public FileSystemStorage(IFileSystem fileSystem, string basePath)
        {
            _fileSystem = fileSystem;
            _basePath = basePath;
            _fileSystem.EnsureDirectory(_basePath);
        }

        public async UniTask SaveAsync(string key, byte[] data, CancellationToken ct = default)
        {
            var path = GetPath(key);
            await _fileSystem.WriteAllBytesAsync(path, data, ct);
        }

        public async UniTask<byte[]> LoadAsync(string key, CancellationToken ct = default)
        {
            var path = GetPath(key);
            return await _fileSystem.ReadAllBytesAsync(path, ct);
        }

        public UniTask<bool> ExistsAsync(string key, CancellationToken ct = default)
        {
            var path = GetPath(key);
            return UniTask.FromResult(_fileSystem.Exists(path));
        }

        public UniTask DeleteAsync(string key, CancellationToken ct = default)
        {
            var path = GetPath(key);
            _fileSystem.Delete(path);
            return UniTask.CompletedTask;
        }

        private string GetPath(string key)
        {
            return System.IO.Path.Combine(_basePath, key + ".dat");
        }
    }
}
