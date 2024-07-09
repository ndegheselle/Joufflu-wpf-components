using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Joufflu.Shared.Helpers
{
    internal class FileSystemHelper
    {
        /// <summary>
        /// Get a valid file name, for example when copying (FileName - Copy (2).ext)
        /// </summary>
        /// <param name="destinationFolder">Destination folder of the file</param>
        /// <param name="fileName">Requested file name (with extension)</param>
        /// <returns></returns>
        public static string GetValidCopyName(string destinationFolder, string fileName)
        {
            return GetValidName(destinationFolder, fileName, "{0} - Copy{1}{2}");
        }

        /// <summary>
        /// Get a valid folder name, for example when copying (New folder (2))
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <returns></returns>
        public static string GetValidNewFolderName(string destinationFolder)
        {
            return GetValidName(destinationFolder, "New folder", "{0} {1}");
        }

        /// <summary>
        /// Get a valid name, for example when copying (FileName - Copy (2).ext)
        /// </summary>
        /// <param name="destinationFolder"></param>
        /// <param name="fileName"></param>
        /// <param name="nameFormat"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        private static string GetValidName(string destinationFolder, string fileName, string nameFormat, int number = 1)
        {
            string path = Path.Combine(destinationFolder, fileName);
            if (!File.Exists(path) && !Directory.Exists(path)) return fileName;

            string numberString = number > 1 ? $"({number})" : "";
            string destinationName = String.Format(nameFormat, Path.GetFileNameWithoutExtension(fileName), numberString, Path.GetExtension(fileName));
            string destinationPath = Path.Combine(destinationFolder, destinationName);
            if (File.Exists(destinationPath) || Directory.Exists(destinationPath))
            {
                return GetValidName(destinationFolder, fileName, nameFormat, number + 1);
            }
            return destinationName;
        }

        /// <summary>
        /// Is the root path a parent folder of the child path?
        /// </summary>
        /// <param name="childPath"></param>
        /// <param name="rootPath"></param>
        /// <returns></returns>
        public static bool IsInSubfolder(string childPath, string rootPath)
        {
            if (File.Exists(rootPath)) rootPath = Path.GetDirectoryName(rootPath);
            return childPath.StartsWith(rootPath);
        }
    }
}
