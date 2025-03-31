using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using _RD3._Universal._Scripts.Utilities;
using UnityEngine;

namespace _RD3.SaveSystem
{
    public class EncryptSystem : Singleton<EncryptSystem>
    {
        private static string key = "8aKzxN5u9wiyT6F2jPpZu1+3D8MzXoT3gUVcM5hJgFs=";
        private void Start()
        {
            key = !PlayerPrefs.HasKey("Key") ? GenerateRandomKey() : PlayerPrefs.GetString("Key");
            PlayerPrefs.SetString("Key", key);
        }

        private string GenerateRandomKey()
        {
            using var aes = Aes.Create();
            aes.KeySize = 128;
            aes.GenerateKey();
            return Convert.ToBase64String(aes.Key);
        }

        public byte[] GenerateRandomIV()
        {
            byte[] iv = new byte[16];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(iv);
            return iv;
        }
        public byte[] EncryptDataAes(string plainText, byte[] iv = null)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = iv; 

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                    }
                    return msEncrypt.ToArray();
                }
            }
        }
      
        public byte[] EncryptDataAes(byte[] plainBytes, byte[] iv = null)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                    csEncrypt.FlushFinalBlock();
                    return msEncrypt.ToArray();
                }
            }
        }
        
        public byte[] DecryptDataToBytes(byte[] cipherText, byte[] iv = null)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Convert.FromBase64String(key);
                aesAlg.IV = iv; 

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                using (MemoryStream output = new MemoryStream())
                {
                    csDecrypt.CopyTo(output);
                    return output.ToArray();
                }
            }
        }
    }
}