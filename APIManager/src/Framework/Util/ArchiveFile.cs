using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using Framework.Logging;

namespace Framework.Util
{
    /// <summary>
    /// An Archive file is an archive that hold one to many files in ZIP format.
    /// </summary>
    internal class ArchiveFile
    {
        internal const string _Extension = ".zip";      // The default extension.

        private string _archiveName;                    // The full name of the Archive file.
        private string _archivePath;                    // The Archive path (archiveName minus the actual name of the archive).
        private List<Tuple<string, string>> _entryList;  // A list of all files that must be added to the archive, including their archive folder.

        /// <summary>
        /// Creates a new archive in the file system with the specified name, which must be a full pathname with- or without extension.
        /// If the archivename does not have an extension, a pre-defined extension is added. Please note that we simply look for the last
        /// occurance of the '.' character in the name. If one is found, we simply assume that this is the extension and don't alter the
        /// name any further. So, if you want to use the default extension, refrain from using '.' anywhere in the name or add the extension
        /// yourself by using the 'ArchiveFile._Extension' constant.
        /// Please note that specifying a relative pathname might lead to unwanted side effects if the current directory is not set in advance.
        /// </summary>
        /// <param name="archiveName">Full path- and filename for the archive. Extension is optional.</param>
        internal ArchiveFile(string archiveName)
        {
            Logger.WriteInfo("Framework.Util.ArchiveFile >> Creating new archive with archive name '" + archiveName + "'...");
            this._entryList = new List<Tuple<string, string>>();
            this._archiveName = archiveName;
            if (archiveName.LastIndexOf('.') < 0) this._archiveName += _Extension;

            int pathIdx = archiveName.LastIndexOf('/');
            if (pathIdx < 0) pathIdx = archiveName.LastIndexOf('\\');
            this._archivePath = (pathIdx >= 0) ? archiveName.Substring(0, pathIdx) : ".";
        }

        /// <summary>
        /// Register a filename with the archive. This is only a 'reservation' of the name, nothing is actually written to the file system
        /// until invocation of the 'Create' method! A typical use case is to create an archive, register a bunch of files with the archive 
        /// and then call 'Create' to actually create the archive on the file system.
        /// The provided fileName parameter MUST be a valid fully qualified name that we can open for read.
        /// The optional archivePath MUST be a relative pathname within the archive. That is, it MUST NOT start with a '/' character.
        /// Note that in all cases, the method extracts the relative name of the actual file from the provided full name and appends this
        /// to the archivePath in order to produce the name within the archive. In other words: the archivePath ONLY defines a folder location
        /// within the archive, it does NOT specify a filename!
        /// </summary>
        /// <param name="filename">Fully qualified filename of source file.</param>
        /// <param name="archivePath">Optional relative pathname for archive folder in which we want to store the file.</param>
        internal void AddFile(string filename, string archivePath = null)
        {
            Logger.WriteInfo("Framework.Util.ArchiveFile.AddFile >> Adding file '" + filename + "' to archive '" +
                             this._archiveName + "' with relative path '" + ((archivePath != null) ? archivePath : string.Empty) + "'...");

            // Check whether the provided filename is valid for read...
            if (!File.Exists(filename))
            {
                Logger.WriteError("Framework.Util.ArchiveFile.AddFile >> Illegal filename '" + filename + "' passed to archive '" +
                                  this._archiveName + "'!");
                throw new ArgumentException("Framework.Util.ArchiveFile.AddFile >> Illegal filename '" + filename + "' passed to archive '" + this._archiveName + "'!");
            }

            // Check whether we have an archive path and if so, make sure it's a relative one...
            if (!string.IsNullOrEmpty(archivePath))
            {
                if (archivePath[0] == '\\' || archivePath[0] == '/') archivePath = archivePath.Substring(1, archivePath.Length - 1);
            }
            else archivePath = string.Empty;

            // Make sure this is indeed a new file...
            foreach (Tuple<string, string> entry in this._entryList)
            {
                if (entry.Item1 == filename)
                {
                    Logger.WriteError("Framework.Util.ArchiveFile.AddFile >> Duplicate filename '" + filename + "' passed to archive '" +
                                      this._archiveName + "'!");
                    throw new ArgumentException("Framework.Util.ArchiveFile.AddFile >> Duplicate filename '" + filename + "' passed to archive '" + this._archiveName + "'!");
                }
            }

            // Finally, store the entry in our archive list...
            this._entryList.Add(new Tuple<string, string>(filename, archivePath));
        }

        /// <summary>
        /// Invoke this method to actually create the archive on the file system and write all registered files to it. If the archive already 
        /// exists, it is overwritten by a new version.
        /// </summary>
        /// <returns>True if created ok, false on errors.</returns>
        internal bool Create()
        {
            bool result = true;
            try
            {
                Logger.WriteInfo("Framework.Util.ArchiveFile.Create >> Creating archive '" + this._archiveName + "'...");
                if (File.Exists(this._archiveName)) File.Delete(this._archiveName);
                using (ZipArchive archive = ZipFile.Open(this._archiveName, ZipArchiveMode.Create))
                {
                    foreach (Tuple<string, string> entry in this._entryList)
                    {
                        string fileName = entry.Item1;
                        int pathIdx = entry.Item1.LastIndexOf('/');
                        if (pathIdx < 0) pathIdx = entry.Item1.LastIndexOf('\\');
                        if (pathIdx >= 0) fileName = entry.Item1.Substring(pathIdx + 1, entry.Item1.Length - pathIdx - 1);
                        string entryName = entry.Item2;
                        if (entryName != string.Empty) entryName += "/";
                        entryName += fileName;
                        archive.CreateEntryFromFile(entry.Item1, entryName);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Util.ArchiveFile.Create >> Error creating archive '" + this._archiveName + "' because: " + exc.Message);
                result = false;
            }
            return result;
        }

        /// <summary>
        /// Extracts the contents of the archive to the directory extracted from the archive name (if no extract path is specified), or to the
        /// specified path.
        /// </summary>
        /// <param name="extractPath">Optional name of the directory to which the archive must be extracted. This must be an existing directory.</param>
        /// <returns>True if extracted ok, false on errors.</returns>
        internal bool Extract(string extractPath = null)
        {
            bool result = true;
            try
            {
                using (ZipArchive archive = ZipFile.Open(this._archiveName, ZipArchiveMode.Read))
                {
                    archive.ExtractToDirectory(!string.IsNullOrEmpty(extractPath) ? extractPath : this._archivePath);
                }
            }
            catch (Exception exc)
            {
                Logger.WriteError("Framework.Util.ArchiveFile.Extract >> Error extracting archive '" + this._archiveName + "' because: " + exc.Message);
                result = false;
            }
            return result;
        }
    }
}
