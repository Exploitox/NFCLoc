using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;


namespace NFCLoc.Plugin.medatixx
{
    public class CryptoUtils
    {
        private const CipherMode cipherMode = CipherMode.CBC;
        private const PaddingMode paddingMode = PaddingMode.ISO10126;
        private const string defaultVector = "fdsah123456789";
        private const int iterations = 2;

        public static string CreateSalt()
        {
            //Generate a cryptographic random number.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            byte[] buff = new byte[20];
            rng.GetBytes(buff);

            // Return a Base64 string representation of the random number.
            return Convert.ToBase64String(buff);
        }

        /// <summary>
        /// To encrypt the plain text. This produces the different encrypted text for same string everytime.
        /// </summary>
        /// <param name="plainText">String value to encrypt</param>
        /// <returns></returns>
        public static string Encrypt(string plainText, string saltKey)
        {
            byte[] clearData = Encoding.Unicode.GetBytes(plainText);
            byte[] encryptedData;
            var crypt = GetCrypto(saltKey);
            using (var ms = new MemoryStream())
            {
                using (var cs = new CryptoStream(ms, crypt.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    cs.Write(clearData, 0, clearData.Length);
                    //cs.FlushFinalBlock(); //Have tried this active and commented with no change.
                }
                encryptedData = ms.ToArray();
            }
            //Changed per Xint0's answer.
            return Convert.ToBase64String(encryptedData);
        }

        public static string Decrypt(string cipherText, string saltKey)
        {
            //Changed per Xint0's answer.
            if (!String.IsNullOrEmpty(cipherText))
            {
                byte[] encryptedData = Convert.FromBase64String(cipherText);
                byte[] clearData;
                var crypt = GetCrypto(saltKey);
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, crypt.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(encryptedData, 0, encryptedData.Length);
                        //I have tried adding a cs.FlushFinalBlock(); here as well.
                    }
                    clearData = ms.ToArray();
                }
                return Encoding.Unicode.GetString(clearData);
            }
            else
            {
                return null;
            }
        }

        private static Rijndael GetCrypto(string passphrase)
        {
            var crypt = Rijndael.Create();
            crypt.Mode = cipherMode;
            crypt.Padding = paddingMode;
            crypt.BlockSize = 256;
            crypt.KeySize = 256;
            crypt.Key = new Rfc2898DeriveBytes(passphrase, Encoding.Unicode.GetBytes(defaultVector), iterations).GetBytes(32);
            crypt.IV = new Rfc2898DeriveBytes(passphrase, Encoding.Unicode.GetBytes(defaultVector), iterations).GetBytes(32);
            return crypt;
        }
    }
}
