using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ProjectBase.Data.Core
{
    public static class AesEncryptor
    {
        private const int IvSize = 16;

        public static byte[] Encrypt(byte[] data, string key)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey(key);
            aes.GenerateIV();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using var encryptor = aes.CreateEncryptor();
            var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);

            var result = new byte[IvSize + encrypted.Length];
            Array.Copy(aes.IV, 0, result, 0, IvSize);
            Array.Copy(encrypted, 0, result, IvSize, encrypted.Length);
            return result;
        }

        public static byte[] Decrypt(byte[] data, string key)
        {
            using var aes = Aes.Create();
            aes.Key = DeriveKey(key);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var iv = new byte[IvSize];
            Array.Copy(data, 0, iv, 0, IvSize);
            aes.IV = iv;

            var encrypted = new byte[data.Length - IvSize];
            Array.Copy(data, IvSize, encrypted, 0, encrypted.Length);

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);
        }

        private static readonly Dictionary<string, byte[]> _keyCache = new();

        private static byte[] DeriveKey(string key)
        {
            if (_keyCache.TryGetValue(key, out var cached))
                return cached;

            using var sha256 = SHA256.Create();
            var derived = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            _keyCache[key] = derived;
            return derived;
        }
    }
}
