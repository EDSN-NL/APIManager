using System;
using System.IO;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;

namespace Framework.Util
{
    /// <summary>
    /// This class facilitates encryption and decryption of strings using an internal, fixed, key and optional caller-provided salt.
    /// The code is based on a handy example I found on the Internet: http://code-smart.org.uk/tricks/basic-two-way-encryption-in-net/
    /// </summary>
    internal sealed class CryptedString
    {
        private readonly SecureString _internalKey;

        /// <summary>
        /// Default constructor, creates a new instance that is ready to accepts strings for encryption or decryption.
        /// Since we use this for simple, low-security tasks only, it used a fixed 'seed' for the key.
        /// </summary>
        internal CryptedString()
        {
            this._internalKey = new SecureString();
            // This is the 'SecurePasswordForECDM'...
            byte[] secret = { 0x53, 0x65, 0x63, 0x75, 0x72, 0x65, 0x50, 0x61, 0x73, 0x73,
                              0x77, 0x6f, 0x72, 0x64, 0x46, 0x6f, 0x72, 0x45, 0x43, 0x44, 0x46 };
            foreach (byte b in secret) this._internalKey.AppendChar((char)b);
        }

        /// <summary>
        /// Encrypt a string using 'default salt'.
        /// </summary>
        /// <param name="text">String to be encrypted.</param>
        /// <returns>Encrypted string or empty string on invalid input.</returns>
        internal string Encrypt(string text)
        {
            return Encrypt(text, GetDefaultSalt());
        }

        /// <summary>
        /// Encrypt a string using 'externally provided' salt.
        /// </summary>
        /// <param name="text">String to be encrypted.</param>
        /// <param name="salt">Salt to be added before encryption.</param>
        /// <returns>Encrypted string or empty string on invalid input.</returns>
        public string Encrypt(string text, string salt)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;

            RijndaelManaged rijndaelCipher;
            byte[] textData;
            ICryptoTransform encryptor;

            using (rijndaelCipher = new RijndaelManaged())
            {
                var secretKey = GetSecretKey(salt);

                // First we need to turn the input strings into a byte array.
                textData = Encoding.Unicode.GetBytes(text);

                // Create a encryptor from the existing secretKey bytes.
                // We use 32 bytes for the secret key. The default Rijndael key length is 256 bit (32 bytes) and then 16 bytes for the 
                // Initialization Vector (IV). The default Rijndael IV length is 128 bit (16 bytes).
                encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
            }

            MemoryStream memoryStream;
            byte[] encryptedData;

            // Create a MemoryStream that is going to hold the encrypted bytes:
            using (memoryStream = new MemoryStream())
            {
                // Create a CryptoStream through which we are going to be processing our data. CryptoStreamMode.Write means that we 
                // are going to be writing data to the stream and the output will be written in the MemoryStream we have provided.
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(textData, 0, textData.Length);
                    cryptoStream.FlushFinalBlock();
                    encryptedData = memoryStream.ToArray();
                    memoryStream.Close();
                    cryptoStream.Close();
                }
            }

            // Convert encrypted data into a base64-encoded string. A common mistake would be to use an Encoding class for that.
            // It does not work, because not all byte values can be represented by characters. We are going to be using Base64 encoding.
            // That is designed exactly for what we are trying to do.
            var encryptedText = Convert.ToBase64String(encryptedData);

            return encryptedText;
        }

        /// <summary>
        /// Decrypt a string using 'default salt'.
        /// </summary>
        /// <param name="text">Encrypted string to be decrypted.</param>
        /// <returns>Decrypted string or empty string on invalid input.</returns>
        public string Decrypt(string encryptedText)
        {
            return Decrypt(encryptedText, GetDefaultSalt());
        }

        /// <summary>
        /// Decrypt a string using 'externally provided' salt.
        /// </summary>
        /// <param name="text">Encrypted to be decrypted.</param>
        /// <param name="salt">Salt to be used for decryption (must match salt used for encryption).</param>
        /// <returns>Decrypted string or empty string on invalid input.</returns>
        public string Decrypt(string encryptedText, string salt)
        {
            if (string.IsNullOrEmpty(encryptedText)) return string.Empty;

            RijndaelManaged rijndaelCipher;
            byte[] encryptedData;
            ICryptoTransform decryptor;

            using (rijndaelCipher = new RijndaelManaged())
            {
                var secretKey = GetSecretKey(salt);

                // First we need to turn the input strings into a byte array.
                encryptedData = Convert.FromBase64String(encryptedText);

                // Create a decryptor from the existing SecretKey bytes.
                decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
            }

            MemoryStream memoryStream;
            byte[] unencryptedData;
            int decryptedDataLength;

            using (memoryStream = new MemoryStream(encryptedData))
            {
                // Create a CryptoStream. Always use Read mode for decryption.
                using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                {
                    // Since at this point we don't know what the size of decrypted data will be, allocate the buffer long enough 
                    // to hold EncryptedData; DecryptedData is never longer than EncryptedData.
                    unencryptedData = new byte[encryptedData.Length];

                    try
                    {
                        decryptedDataLength = cryptoStream.Read(unencryptedData, 0, unencryptedData.Length);
                    }
                    catch
                    {
                        throw new CryptographicException("Unable to decrypt string");
                    }
                    cryptoStream.Close();
                    memoryStream.Close();
                }
            }

            var decryptedText = Encoding.Unicode.GetString(unencryptedData, 0, decryptedDataLength);
            return decryptedText;
        }

        /// <summary>
        /// Helper function that returns a key based on provided salt plus our internal secret key.
        /// We are using salt to make it harder to guess our key using a dictionary attack.
        /// </summary>
        /// <param name="salt">Salt to be used when creating the key.</param>
        /// <returns>Binary key.</returns>
        private PasswordDeriveBytes GetSecretKey(string salt)
        {
            var encodedSalt = Encoding.ASCII.GetBytes(salt);
            var valuePointer = IntPtr.Zero;
            try
            {
                // The Secret Key will be generated from the internal password and specified salt.
                valuePointer = Marshal.SecureStringToGlobalAllocUnicode(this._internalKey);
                return new PasswordDeriveBytes(Marshal.PtrToStringUni(valuePointer), encodedSalt);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(valuePointer);
            }
        }

        /// <summary>
        /// Helper function that derives salt from our internal key in case no salt has been specified.
        /// </summary>
        /// <returns>generated salt.</returns>
        private string GetDefaultSalt()
        {
            return this._internalKey.Length.ToString(CultureInfo.InvariantCulture);
        }
    }
}

