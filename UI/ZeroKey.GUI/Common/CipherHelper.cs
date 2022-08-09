using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Rijndael256;

namespace ZeroKey.GUI.Common;

public static class CipherHelper
{
    // This constant is used to determine the keysize of the encryption algorithm in bits.
    // We divide this by 8 within the code below to get the equivalent number of bytes.
    private const int Keysize = 256;

    // This constant determines the number of iterations for the password bytes generation function.
    private const int DerivationIterations = 1000;
    
    public static string Encrypt(string plainText, string passPhrase)
    {
        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
        // so that the same Salt and IV values can be used when decrypting.  
        var saltStringBytes = Generate256BitsOfRandomEntropy();
        var ivStringBytes = Generate256BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(keyBytes);
            var keyParamWithIV = new ParametersWithIV(keyParam, ivStringBytes, 0, 32);

            cipher.Init(true, keyParamWithIV);
            var comparisonBytes = new byte[cipher.GetOutputSize(plainTextBytes.Length)];
            var length = cipher.ProcessBytes(plainTextBytes, comparisonBytes, 0);

            cipher.DoFinal(comparisonBytes, length);
            //                return Convert.ToBase64String(comparisonBytes);
            return Convert.ToBase64String(saltStringBytes.Concat(ivStringBytes).Concat(comparisonBytes).ToArray());
        }
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        // Get the complete stream of bytes that represent:
        // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
        // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
        // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
        // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(Keysize / 8);
            var engine = new RijndaelEngine(256);
            var blockCipher = new CbcBlockCipher(engine);
            var cipher = new PaddedBufferedBlockCipher(blockCipher, new Pkcs7Padding());
            var keyParam = new KeyParameter(keyBytes);
            var keyParamWithIV = new ParametersWithIV(keyParam, ivStringBytes, 0, 32);

            cipher.Init(false, keyParamWithIV);
            var comparisonBytes = new byte[cipher.GetOutputSize(cipherTextBytes.Length)];
            var length = cipher.ProcessBytes(cipherTextBytes, comparisonBytes, 0);

            cipher.DoFinal(comparisonBytes, length);
            //return Convert.ToBase64String(saltStringBytes.Concat(ivStringBytes).Concat(comparisonBytes).ToArray());

            var nullIndex = comparisonBytes.Length - 1;
            while (comparisonBytes[nullIndex] == (byte)0)
                nullIndex--;
            comparisonBytes = comparisonBytes.Take(nullIndex + 1).ToArray();


            var result = Encoding.UTF8.GetString(comparisonBytes, 0, comparisonBytes.Length);

            return result;
        }
    }

    private static byte[] Generate256BitsOfRandomEntropy()
    {
        var randomBytes = new byte[32]; // 32 Bytes will give us 256 bits.
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            // Fill the array with cryptographically secure random bytes.
            rngCsp.GetBytes(randomBytes);
        }
        return randomBytes;
    }
}