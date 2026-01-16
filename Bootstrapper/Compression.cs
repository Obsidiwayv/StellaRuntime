using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

// Source - https://stackoverflow.com/a
// Posted by kofifus, modified by community. See post 'Timeline' for change history
// Retrieved 2026-01-16, License - CC BY-SA 4.0

namespace StellaBootstrapper
{
    public static class ZipFileEx
    {
        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName) =>
        ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding: null, overwriteFiles: false);

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, bool overwriteFiles) =>
            ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding: null, overwriteFiles: overwriteFiles);

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, Encoding? entryNameEncoding) =>
            ExtractToDirectory(sourceArchiveFileName, destinationDirectoryName, entryNameEncoding: entryNameEncoding, overwriteFiles: false);

        public static void ExtractToDirectory(string sourceArchiveFileName, string destinationDirectoryName, Encoding? entryNameEncoding, bool overwriteFiles)
        {
            ArgumentNullException.ThrowIfNull(sourceArchiveFileName);
            ArgumentNullException.ThrowIfNull(destinationDirectoryName);

            using ZipArchive archive = ZipFile.Open(sourceArchiveFileName, ZipArchiveMode.Read, entryNameEncoding);

            foreach (ZipArchiveEntry entry in archive.Entries)
            {
                DirectoryInfo di = Directory.CreateDirectory(destinationDirectoryName);
                string destinationDirectoryFullPath = di.FullName;
                if (!destinationDirectoryFullPath.EndsWith(Path.DirectorySeparatorChar))
                {
                    char sep = Path.DirectorySeparatorChar;
                    destinationDirectoryFullPath = string.Concat(destinationDirectoryFullPath, new ReadOnlySpan<char>(in sep));
                }

                string entryFullName = entry.FullName;
                if (entryFullName.Length > 0 && entryFullName[0] == '/') entryFullName = entryFullName[1..]; // remove leading root

                string fileDestinationPath = Path.GetFullPath(Path.Combine(destinationDirectoryFullPath, entryFullName.Replace('\0', '_')));

                var IsCaseSensitive = !(OperatingSystem.IsWindows() || OperatingSystem.IsMacOS() || OperatingSystem.IsIOS() || OperatingSystem.IsTvOS() || OperatingSystem.IsWatchOS());
                var stringComparison = IsCaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                if (!fileDestinationPath.StartsWith(destinationDirectoryFullPath, stringComparison)) throw new IOException(@"Extracting Zip entry would have resulted in a file outside the specified destination directory.");

                if (Path.GetFileName(fileDestinationPath).Length == 0)
                {
                    if (entry.Length != 0) throw new IOException(@"Zip entry name ends in directory separator character but contains data.");
                    Directory.CreateDirectory(fileDestinationPath);

                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileDestinationPath)!);
                    entry.ExtractToFile(fileDestinationPath, overwrite: overwriteFiles);
                }
            }
        }
    }
}
