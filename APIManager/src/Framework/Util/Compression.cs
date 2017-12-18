using System;
using System.IO;
using System.Text;
using System.IO.Compression;

namespace Framework.Util
{
    internal static class Compression
    {
        /// <summary>
        /// Compress the specified string into an array of bytes and return this as a Base64-encoded text string.
        /// </summary>
        /// <param name="str">The string to be compressed.</param>
        /// <returns>Compressed string as Base64 string.</returns>
        public static string Zip(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(bytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(mso, CompressionMode.Compress)) CopyTo(msi, gs);

                var plainTextBytes = mso.ToArray(); 
                return Convert.ToBase64String(plainTextBytes);
            }
        }

        /// <summary>
        /// Deflate a byte array that has been compressed earlier with call to 'Zip'. The array must be passed as a Base64-encoded
        /// text string.
        /// </summary>
        /// <param name="bytes">Base64-encoded text string that must be deflated.</param>
        /// <returns>The original string.</returns>
        internal static string Unzip(string base64String)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64String);
            using (var msi = new MemoryStream(base64EncodedBytes))
            using (var mso = new MemoryStream())
            {
                using (var gs = new GZipStream(msi, CompressionMode.Decompress)) CopyTo(gs, mso);
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }

        /// <summary>
        /// Helper function that copies all bytes from the source stream to the destination stream.
        /// </summary>
        /// <param name="src">Source stream.</param>
        /// <param name="dest">Destination stream.</param>
        private static void CopyTo(Stream src, Stream dest)
        {
            byte[] bytes = new byte[4096];
            int cnt;

            while ((cnt = src.Read(bytes, 0, bytes.Length)) != 0)
            {
                dest.Write(bytes, 0, cnt);
            }
        }
    }
}
