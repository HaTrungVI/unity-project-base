using UnityEngine;

namespace ProjectBase.Data.Core
{
    [CreateAssetMenu(fileName = "DataStorageConfig", menuName = "ProjectBase/Data Storage Config")]
    public class DataStorageConfig : ScriptableObject
    {
        [SerializeField] private string _saveFolder = "SaveData";
        [SerializeField] private bool _useEncryption = true;
        [SerializeField] private bool _useCompression = true;
        [SerializeField] private string _encryptionKey = "CHANGE_THIS_KEY_FOR_YOUR_PROJECT";

        public string SaveFolder => _saveFolder;
        public bool UseEncryption => _useEncryption;
        public bool UseCompression => _useCompression;

        public IDataStorage CreateStorage()
        {
            var fileSystem = new PlatformFileSystem();
            var basePath = System.IO.Path.Combine(Application.persistentDataPath, _saveFolder);
            return new FileSystemStorage(fileSystem, basePath);
        }

        public IDataSerializer CreateSerializer()
        {
            return new JsonBinarySerializer(_useCompression, _useEncryption, _encryptionKey);
        }
    }
}
