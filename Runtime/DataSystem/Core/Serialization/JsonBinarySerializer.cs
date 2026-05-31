using System;
using System.Text;
using UnityEngine;

namespace ProjectBase.Data.Core
{
    public class JsonBinarySerializer : IDataSerializer
    {
        private const byte FlagCompressed = 0x01;
        private const byte FlagEncrypted = 0x02;

        private readonly bool _useCompression;
        private readonly bool _useEncryption;
        private readonly string _encryptionKey;

        public JsonBinarySerializer(bool useCompression, bool useEncryption, string encryptionKey)
        {
            _useCompression = useCompression;
            _useEncryption = useEncryption;
            _encryptionKey = encryptionKey;
        }

        public byte[] Serialize<T>(T data)
        {
            var json = JsonUtility.ToJson(data, false);
            var bytes = Encoding.UTF8.GetBytes(json);

            byte flags = 0;

            if (_useCompression)
            {
                bytes = GZipCompressor.Compress(bytes);
                flags |= FlagCompressed;
            }

            if (_useEncryption)
            {
                bytes = AesEncryptor.Encrypt(bytes, _encryptionKey);
                flags |= FlagEncrypted;
            }

            var result = new byte[1 + bytes.Length];
            result[0] = flags;
            Array.Copy(bytes, 0, result, 1, bytes.Length);
            return result;
        }

        public T Deserialize<T>(byte[] bytes)
        {
            var flags = bytes[0];
            var wasCompressed = (flags & FlagCompressed) != 0;
            var wasEncrypted = (flags & FlagEncrypted) != 0;

            var payload = new byte[bytes.Length - 1];
            Array.Copy(bytes, 1, payload, 0, payload.Length);

            if (wasEncrypted)
                payload = AesEncryptor.Decrypt(payload, _encryptionKey);

            if (wasCompressed)
                payload = GZipCompressor.Decompress(payload);

            var json = Encoding.UTF8.GetString(payload);
            return JsonUtility.FromJson<T>(json);
        }
    }
}
