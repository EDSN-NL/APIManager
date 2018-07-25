using System;
using System.IO;
using System.Text;
using System.IO.Compression;
using Framework.Context;

namespace Framework.Util
{
    /// <summary>
    /// The Compression class provides methods to compress and uncompress strings and single files.
    /// </summary>
    internal static class Compression
    {
        // Configuration settings used by this module:
        private const string _CompressedFileSuffix = "CompressedFileSuffix";

        /// <summary>
        /// Compress the specified file and optionally remove the original, uncompressed, file.
        /// </summary>
        /// <param name="fileName">Absolute path to te file to be compressed.</param>
        /// <returns>The name of the generated compressed file.</returns>
        public static string FileZip(string fileName, bool deleteOriginal)
        {
            string outName = fileName.Substring(0, fileName.LastIndexOf(".")) + ContextSlt.GetContextSlt().GetConfigProperty(_CompressedFileSuffix);
            fileName = fileName.Replace('\\', '/');
            string entryName = fileName.Substring(fileName.LastIndexOf('/') + 1);
            using (FileStream fs = new FileStream(outName, FileMode.Create))
            using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                arch.CreateEntryFromFile(fileName, entryName);
            }
            if (deleteOriginal) File.Delete(fileName);
            return outName;
        }

        /// <summary>
        /// Compress the specified string into an array of bytes and return this as a Base64-encoded text string.
        /// </summary>
        /// <param name="str">The string to be compressed.</param>
        /// <returns>Compressed string as Base64 string.</returns>
        public static string StringZip(string str)
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
        /// Decompresses the specified file to its associated directory.
        /// </summary>
        /// <param name="fileName">Absolute path to te file to be inflated.</param>
        public static void FileUnzip(string fileName)
        {
            fileName = fileName.Replace('\\', '/');
            string dirName = fileName.Substring(0, fileName.LastIndexOf('/'));

            using (FileStream fs = new FileStream(fileName, FileMode.Open))
            using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Read))
            {
                arch.ExtractToDirectory(dirName);
            }
        }

        /// <summary>
        /// Deflate a byte array that has been compressed earlier with call to 'Zip'. The array must be passed as a Base64-encoded
        /// text string.
        /// </summary>
        /// <param name="bytes">Base64-encoded text string that must be deflated.</param>
        /// <returns>The original string.</returns>
        internal static string StringUnzip(string base64String)
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
