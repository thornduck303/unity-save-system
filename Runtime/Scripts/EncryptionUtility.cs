using System;
using System.IO;
using UnityEngine;
using System.Text;
using System.Security.Cryptography;

namespace ThornDuck.SaveSystem.Encryption
{
    /// <summary>
    /// Provides data encryption and decryption.
    /// </summary>
    /// <seealso cref="SaveSystem"/>
    /// <author>Murilo M. Grosso</author>
    public static class EncryptionUtility
    {
        // Must be 32 bytes long
        private const string KEY = "f3c8b9e4a1dc720c54aa9d2be7f03c1d9e8b7426c5ff13d0b27c4e89df5612ab";

        /// <summary>
        /// Encrypts a plain text.
        /// </summary>
        /// <param name="plainText">The string to encrypt.</param>
        /// <returns>
        /// A cipher string.
        /// Returns <c>null</c> if <paramref name="plainText"/> is null.
        /// </returns>
        public static string EncryptString(string plainText)
        {
            if(plainText == null)
                return null;

            byte[] key = Encoding.UTF8.GetBytes(KEY.Substring(0,32));

            using (var aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.Key = key;
                aesAlgorithm.GenerateIV();
                ICryptoTransform encryptor = aesAlgorithm.CreateEncryptor(aesAlgorithm.Key, aesAlgorithm.IV);

                using (var memoryStream = new MemoryStream())
                {
                    memoryStream.Write(aesAlgorithm.IV, 0, aesAlgorithm.IV.Length);
                    using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (var streamWriter = new StreamWriter(cryptoStream))
                            streamWriter.Write(plainText);
                    }
                    // [IV (16 bytes)][Encrypted bytes...]
                    return Convert.ToBase64String(memoryStream.ToArray());
                }
            }
            
        }

        /// <summary>
        /// Decrypts a cipher string produced by <see cref="EncryptString"/>.
        /// </summary>
        /// <param name="cipherText">The encoded encrypted data.</param>
        /// <returns>
        /// The decrypted plain text string.  
        /// Returns <c>null</c> if <paramref name="cipherText"/> is null or invalid.
        /// </returns>
        public static string DecryptString(string cipherText)
        {
            if (cipherText == null)
                return null;

            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - 16];

            Array.Copy(fullCipher, iv, iv.Length);
            Array.Copy(fullCipher, 16, cipher, 0, cipher.Length);

            byte[] key = Encoding.UTF8.GetBytes(KEY.Substring(0, 32));
            using (var aesAlgorithm = Aes.Create())
            {
                aesAlgorithm.Key = key;
                aesAlgorithm.IV = iv;
                ICryptoTransform decryptor = aesAlgorithm.CreateDecryptor(aesAlgorithm.Key, aesAlgorithm.IV);

                using (var memoryStream = new MemoryStream(cipher))
                {
                    using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (var streamReader = new StreamReader(cryptoStream))
                        {
                            try
                            {
                                return streamReader.ReadToEnd();
                            }
                            catch(Exception e)
                            {
                                if (Debug.isDebugBuild)
                                    Debug.LogError($"[ENCRYPTION UTILITY] Failed to decrypt cipher:\n{e}");
                                return null;
                            }
                        }
                    }
                }
            }
        }
    }
}
