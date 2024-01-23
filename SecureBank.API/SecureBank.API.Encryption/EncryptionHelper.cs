using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SecureBank.API.Encryption
{
    public class EncryptionHelper
    {
        #region SERVICES

        private EncryptionConfiguration _configuration;

        #endregion



        #region FIELDS

        private Aes _aes;

        #endregion



        #region CONSTRUCTORS

        public EncryptionHelper(EncryptionConfiguration configuration)
        {
            _configuration = configuration;

            _aes = Aes.Create();
            _aes.Key = _configuration.Key;
            _aes.IV = _configuration.IV;
        }

        #endregion



        #region PUBLIC METHODS

        public byte[] Encrypt(string data)
        {
            ICryptoTransform encryptor = _aes.CreateEncryptor(_aes.Key, _aes.IV);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                {
                    streamWriter.Write(data);
                }
                return memoryStream.ToArray();
            }
        }

        public string Decrypt(byte[] data)
        {
            ICryptoTransform decryptor = _aes.CreateDecryptor(_configuration.Key, _configuration.IV);
            using (MemoryStream memoryStream = new MemoryStream(data))
            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                byte[] outputBytes = new byte[data.Length];
                int decryptedByteCount = cryptoStream.Read(outputBytes, 0, outputBytes.Length);
                return Encoding.UTF8.GetString(outputBytes.Take(decryptedByteCount).ToArray());
            }
        }

        #endregion
    }
}
