using System;
using System.IO;

namespace SimpleZip.Services
{
    public static class FileService
    {
        public static string RemoveExtension(string filename, string extension)
        {
            return filename.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? filename.Substring(0, filename.Length - extension.Length) : filename;
        }

        public static string GetNextAvailableNumberedZip(string destinationPath, string baseName)
        {
            int counter = 1;
            while (File.Exists(Path.Combine(destinationPath, $"{baseName} ({counter}).zip")))
            {
                counter++;
            }
            return $"{baseName} ({counter})";
        }
    }
}
