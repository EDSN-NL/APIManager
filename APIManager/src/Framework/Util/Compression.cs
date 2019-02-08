using System;
using System.IO;
using Ionic.Zip;
using System.Text;
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
        /// Recursively compresses the specified directory.
        /// </summary>
        /// <param name="dirName">Absolute path to the directory to be compressed.</param>
        /// <returns>The name of the generated compressed file.</returns>
        public static string DirectoryZip(string dirName)
        {
            string zipName = dirName + ContextSlt.GetContextSlt().GetConfigProperty(_CompressedFileSuffix);
            using (FileStream fs = new FileStream(zipName, FileMode.Create))
            using (ZipFile arch = new ZipFile())
            {
                arch.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                arch.AddDirectory(dirName, ".");
                arch.Save(fs);
            }
            return zipName;
        }

        /// <summary>
        /// Decompresses the specified ZIP file to its parent directory, overwriting existing files.
        /// </summary>
        /// <param name="dirName">Absolute path to te zip file to be inflated.</param>
        public static void DirectoryUnzip(string zipName)
        {
            int extensionIdx = zipName.LastIndexOf('.');
            string dirName = (extensionIdx > 0) ? zipName.Substring(0, extensionIdx) : zipName;

            using (ZipFile arch = new ZipFile(zipName)) arch.ExtractAll(dirName, ExtractExistingFileAction.OverwriteSilently);
        }

        /// <summary>
        /// Compress the specified file and optionally remove the original, uncompressed, file. The function will overwrite an existing compressed file.
        /// </summary>
        /// <param name="fileName">Absolute path to the file to be compressed.</param>
        /// <returns>The name of the generated compressed file.</returns>
        public static string FileZip(string fileName, bool deleteOriginal)
        {
            int extensionIdx = fileName.LastIndexOf('.');
            string outName = fileName;
            if (extensionIdx > 0) outName = fileName.Substring(0, extensionIdx);
            outName += ContextSlt.GetContextSlt().GetConfigProperty(_CompressedFileSuffix);
            //if (!string.IsNullOrEmpty(outName) && File.Exists(outName)) File.Delete(outName);
            fileName = fileName.Replace('\\', '/');
            using (FileStream fs = new FileStream(outName, FileMode.Create))
            using (ZipFile arch = new ZipFile())
            {
                arch.CompressionLevel = Ionic.Zlib.CompressionLevel.BestCompression;
                arch.AddFile(fileName, ".");
                arch.Save(fs);
            }
            if (deleteOriginal) File.Delete(fileName);
            return outName;
        }

        /// <summary>
        /// Decompresses the specified file to its associated directory, overwriting existing files.
        /// </summary>
        /// <param name="fileName">Absolute path to te file to be inflated.</param>
        public static void FileUnzip(string fileName)
        {
            fileName = fileName.Replace('\\', '/');
            string dirName = fileName.Substring(0, fileName.LastIndexOf('/'));
            string bareFileName = fileName.Substring(fileName.LastIndexOf('/') + 1);
            bareFileName = bareFileName.Substring(0, bareFileName.LastIndexOf('.'));
            try
            {
                using (ZipFile arch = new ZipFile(fileName)) arch.ExtractAll(dirName, ExtractExistingFileAction.OverwriteSilently);
            }
            catch (IOException exc)
            {
                // Caught an IO Exception, probably because the 'OverwriteSilently' does not work.
                // Determine the file to be deleted, delete the file and retry.
                string newFileName = exc.Message.Substring(exc.Message.IndexOf(bareFileName));
                newFileName = dirName + "/" + newFileName.Substring(0, newFileName.IndexOf('"'));
                File.Delete(newFileName);
                using (ZipFile arch = new ZipFile(fileName)) arch.ExtractAll(dirName, ExtractExistingFileAction.OverwriteSilently);
            }
        }

        /// <summary>
        /// Compress the specified string into an array of bytes and return this as a Base64-encoded text string.
        /// </summary>
        /// <param name="str">The string to be compressed.</param>
        /// <returns>Compressed string as Base64 string.</returns>
        public static string StringZip(string str)
        {
            var buffer = Encoding.UTF8.GetBytes(str);

            using (var msi = new MemoryStream(buffer))
            using (var mso = new MemoryStream())
            {
                using (var zso = new ZipOutputStream(mso))
                {
                    zso.PutNextEntry("contents");
                    msi.CopyTo(zso);
                }
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
        internal static string StringUnzip(string base64String)
        {
            var base64EncodedBytes = Convert.FromBase64String(base64String);
            using (var msi = new MemoryStream(base64EncodedBytes))
            using (var mso = new MemoryStream())
            {
                using (var zsi = new ZipInputStream(msi))
                {
                    zsi.GetNextEntry();
                    zsi.CopyTo(mso);
                }
                return Encoding.UTF8.GetString(mso.ToArray());
            }
        }
    }
}
