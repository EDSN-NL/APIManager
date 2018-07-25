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
    internal sealed class CryptString
    {
        private readonly SecureString _internalKey;
        private SecureString _encryptedString;
        private string _salt;

        /// <summary>
        /// Get- or set the encrypted string value.
        /// </summary>
        internal SecureString EncryptedString
        {
            get { return this._encryptedString; }
            set { this._encryptedString = value; }
        }

        /// <summary>
        /// Get- or set the salt value to be used for encryption or decryption.
        /// </summary>
        internal string Salt { set { this._salt = value; } }

        /// <summary>
        /// Default constructor, creates a new instance that is ready to accept strings for encryption or decryption.
        /// Since we use this for simple, low-security tasks only, it used a fixed 'seed' for the key.
        /// </summary>
        internal CryptString()
        {
            this._internalKey = new SecureString();
            // This is the 'SecurePasswordForECDM'...
            byte[] secret = { 0x53, 0x65, 0x63, 0x75, 0x72, 0x65, 0x50, 0x61, 0x73, 0x73,
                              0x77, 0x6f, 0x72, 0x64, 0x46, 0x6f, 0x72, 0x45, 0x43, 0x44, 0x46 };
            foreach (byte b in secret) this._internalKey.AppendChar((char)b);
            this._encryptedString = new SecureString();
            this._salt = GetDefaultSalt();
        }

        /// <summary>
        /// Constructor that receives an unencrypted string and stores this as an encrypted string, using 'default salt'.
        /// </summary>
        /// <param name="plainString">String to encrypt.</param>
        internal CryptString(SecureString plainString) : this()
        {
            this._encryptedString = Encrypt(plainString);
        }

        /// <summary>
        /// Constructor that receives an unencrypted string and salt and uses these to create and encypted string.
        /// </summary>
        /// <param name="plainString">String to encrypt.</param>
        /// <param name="salt">Salt to be used for encryption.</param>
        internal CryptString(SecureString plainString, string salt) : this()
        {
            this._encryptedString = Encrypt(plainString, salt);
            this._salt = salt;
        }

        /// <summary>
        /// Returns the decrypted value of our CryptString object.
        /// </summary>
        /// <returns>Decrypted value of current object.</returns>
        internal SecureString Decrypt()
        {
            return Decrypt(this._encryptedString, this._salt);
        }

        /// <summary>
        /// Decrypt a string using 'default salt'.
        /// </summary>
        /// <param name="text">Encrypted string to be decrypted.</param>
        /// <returns>Decrypted string or empty string on invalid input.</returns>
        internal static SecureString Decrypt(SecureString encryptedText)
        {
            var saltProvider = new CryptString();
            return Decrypt(encryptedText, saltProvider.GetDefaultSalt());
        }

        /// <summary>
        /// Decrypt a string using 'externally provided' salt.
        /// </summary>
        /// <param name="text">Encrypted to be decrypted.</param>
        /// <param name="salt">Salt to be used for decryption (must match salt used for encryption).</param>
        /// <returns>Decrypted string or empty string on invalid input.</returns>
        internal static SecureString Decrypt(SecureString encryptedText, string salt)
        {
            var result = new SecureString();
            var keyProvider = new CryptString();
            if (encryptedText == null || encryptedText.Length == 0) return result;

            int textLen = (encryptedText.Length * sizeof(char));
            RijndaelManaged rijndaelCipher;
            byte[] unencryptedData;
            byte[] encryptedData = new byte[textLen];
            IntPtr bufPtr = IntPtr.Zero;
            ICryptoTransform decryptor;
            MemoryStream memoryStream;
            int decryptedDataLength;

            using (rijndaelCipher = new RijndaelManaged())
            {
                var secretKey = keyProvider.GetSecretKey(salt);

                // First we need to turn the input string into a byte array.
                encryptedData = Convert.FromBase64String(ToPlainString(encryptedText));

                // Create a decryptor from the existing SecretKey bytes.
                decryptor = rijndaelCipher.CreateDecryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
            }

            // Create a MemoryStream that is going to hold the encrypted bytes.
            // Create a CryptoStream for decryption. Always use Read mode for decryption.
            using (memoryStream = new MemoryStream(encryptedData))
            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
            {
                // Since at this point we don't know what the size of decrypted data will be, allocate the buffer long enough 
                // to hold EncryptedData; DecryptedData is never longer than EncryptedData.
                unencryptedData = new byte[textLen];

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
            return ToSecureString(Encoding.Unicode.GetString(unencryptedData, 0, decryptedDataLength));
        }

        /// <summary>
        /// Decrypt method using unsafe, plain, strings for in- and output. Do not use for sensitive info!
        /// Since salt is not specified, the method uses 'default salt'.
        /// </summary>
        /// <param name="encryptedText">Text to be decrypted.</param>
        /// <returns>Decrypted text as 'plain' string.</returns>
        internal static string DecryptPlain(string encryptedText)
        {
            return ToPlainString(Decrypt(ToSecureString(encryptedText)));
        }

        /// <summary>
        /// Decrypt method using unsafe, plain, strings for in- and output. Do not use for sensitive info!
        /// </summary>
        /// <param name="encryptedText">Text to be decrypted.</param>
        /// <param name="salt">Salt to be used for decryption.</param>
        /// <returns>Decrypted text as 'plain' string.</returns>
        internal static string DecryptPlain(string encryptedText, string salt)
        {
            return ToPlainString(Decrypt(ToSecureString(encryptedText), salt));
        }

        /// <summary>
        /// Encrypt the specified string using 'default salt'.
        /// </summary>
        /// <param name="text">String to be encrypted.</param>
        /// <returns>Encrypted string or empty string on invalid input.</returns>
        internal static SecureString Encrypt(SecureString text)
        {
            var saltProvider = new CryptString();
            return Encrypt(text, saltProvider.GetDefaultSalt());
        }

        /// <summary>
        /// Encrypt te specified string using specified salt.
        /// </summary>
        /// <param name="text">String to be encrypted.</param>
        /// <param name="salt">Salt to be added before encryption.</param>
        /// <returns>Encrypted string or empty string on invalid input.</returns>
        internal static SecureString Encrypt(SecureString text, string salt)
        {
            var result = new SecureString();
            var keyProvider = new CryptString();
            if (text == null || text.Length == 0) return result;

            int textLen = (text.Length * sizeof(char));
            RijndaelManaged rijndaelCipher;
            ICryptoTransform encryptor;
            MemoryStream memoryStream;
            byte[] encryptedData;

            using (rijndaelCipher = new RijndaelManaged())
            {
                // Create a encryptor from the existing secretKey bytes.
                // We use 32 bytes for the secret key. The default Rijndael key length is 256 bit (32 bytes) and then 16 bytes for the 
                // Initialization Vector (IV). The default Rijndael IV length is 128 bit (16 bytes).
                var secretKey = keyProvider.GetSecretKey(salt);
                encryptor = rijndaelCipher.CreateEncryptor(secretKey.GetBytes(32), secretKey.GetBytes(16));
            }

            // Create a MemoryStream that is going to hold the encrypted bytes.
            // Create a CryptoStream through which we are going to be processing our data. CryptoStreamMode.Write means that we 
            // are going to write data to the stream and the output will be written in the MemoryStream we have provided.
            using (memoryStream = new MemoryStream())
            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
            {
                IntPtr bufPtr = IntPtr.Zero;
                try
                {
                    // First we need to turn the input string into a byte array.
                    byte[] textData = new byte[textLen];
                    bufPtr = Marshal.SecureStringToBSTR(text);
                    Marshal.Copy(bufPtr, textData, 0, textLen);

                    cryptoStream.Write(textData, 0, textLen);
                    cryptoStream.FlushFinalBlock();
                    encryptedData = memoryStream.ToArray();
                    memoryStream.Close();
                    cryptoStream.Close();
                }
                finally
                {
                    // Since SecureStringToBSTR has allocated memory, assure that this is released...
                    Marshal.ZeroFreeBSTR(bufPtr);
                }
            }

            // Convert encrypted data into a base64-encoded secure string. A common mistake would be to use an Encoding class for that.
            // It does not work, because not all byte values can be represented by characters. We are going to be using Base64 encoding,
            // which is designed exactly for what we are trying to do.
            return ToSecureString(Convert.ToBase64String(encryptedData));
        }

        /// <summary>
        /// Encrypt method using unsafe, plain, strings for in- and output. Do not use for sensitive info!
        /// Since salt is not specified, the method uses 'default salt'.
        /// </summary>
        /// <param name="text">Text to be encrypted.</param>
        /// <returns>Encrypted text as 'plain' string.</returns>
        internal static string EncryptPlain(string text)
        {
            return ToPlainString(Encrypt(ToSecureString(text)));
        }

        /// <summary>
        /// Encrypt method using unsafe, plain, strings for in- and output. Do not use for sensitive info!
        /// </summary>
        /// <param name="text">Text to be encrypted.</param>
        /// <param name="salt">Salt to be used for encryption.</param>
        /// <returns>Encrypted text as 'plain' string.</returns>
        internal static string EncryptPlain(string text, string salt)
        {
            return ToPlainString(Encrypt(ToSecureString(text), salt));
        }

        /// <summary>
        /// Returns an 'ordinary' string representation of the secure, encrypted string. Note that using encrypted string as an ordinary string
        /// has negative impact on security of this class.
        /// </summary>
        /// <returns>String representation of our encrypted string.</returns>
        public override string ToString()
        {
            return ToPlainString(this._encryptedString);
        }

        /// <summary>
        /// Returns an 'ordinary' string representation of the specified secure string. Note that using a secure string as an ordinary string
        /// has negative impact on security of string.
        /// </summary>
        /// <returns>String representation of specified secure string.</returns>
        internal static string ToPlainString(SecureString secureString)
        {
            if (secureString != null && secureString.Length > 0)
                return new System.Net.NetworkCredential(string.Empty, secureString).Password;
            else return string.Empty;
        }

        /// <summary>
        /// Helper function that translates an 'ordinary' string into a secure string.
        /// </summary>
        /// <param name="plainString">Plain string to convert.</param>
        /// <returns>SecureString representation of input.</returns>
        internal static SecureString ToSecureString(string plainString)
        {
            var result = new SecureString();
            if (!string.IsNullOrEmpty(plainString)) foreach (char ch in plainString.ToCharArray()) result.AppendChar(ch);
            return result;
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

