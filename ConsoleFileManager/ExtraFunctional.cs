using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleFileManager
{
    /// <summary>
    /// Wrapper for the extra functions used in FileManager but no related to its functional.
    /// </summary>
    public static class ExtraFunctional
    {
        /// <summary>
        /// Converts the given amount of bytes to the highest unit of digital information
        /// having the least value that is greater then 1.
        /// </summary>
        /// <param name="bytes">The amount of bytes to convert.</param>
        /// <param name="type">The unit of digital information corresponding to the resulting value.</param>
        public static double GetNormalizedSize(double bytes, out string type)
        {
            const int CONVERSION_VALUE = 1024;

            var power = 0;
            while (bytes / Math.Pow(CONVERSION_VALUE, power + 1) >= 1)
                power++;

            type = power switch {
                0 => "Б",
                1 => "КБ",
                2 => "МБ",
                3 => "ГБ",
                4 => "ТБ",
                5 => "ПБ",
                _ => "ЭБ"
            };

            return Math.Round(bytes / Math.Pow(CONVERSION_VALUE, power), 2);
        }
        
        
        
        /// <summary>
        /// Transform the specified relative path to absolute path.
        /// </summary>
        /// <param name="pathToParse">Path that is needed to be transformed.</param>
        /// <param name="currentDirectory">The absolute path to transform the relative path to.</param>
        public static string ParsePath(string pathToParse, string currentDirectory)
        {
            string path;
            
            
            /* Check whether the path is relative path and transform it to an absolute path*/
            if (!Path.IsPathRooted(pathToParse)) // if path is relative
                path = (currentDirectory is null) ? null : Path.Combine(currentDirectory, pathToParse);
            
            else if (pathToParse.StartsWith("\\")) // also if path is relative but starts not with folder/file name but with slash
                path = (currentDirectory is null) ? null : Path.Combine(currentDirectory, pathToParse.TrimStart('\\'));
            
            else if (pathToParse.Length == 2 && pathToParse[1] == ':') // special case: if whole path is a drive letter + ':'
                return pathToParse + '\\';
            
            else // if path is absolute
                path = pathToParse;


            
            // the path will be null only if current directory is null and path to parse is a relative path
            if (path is null)
                return null;
            
            
            
            /* Normalize path having '..' to resulting absolute path */
            // "C:\Users\.." => List["C:", "Users" , ".."] => List["C:"] => "C:\"
            var pathSeparated = new List<string>(path.Split(new []{'\\'}, StringSplitOptions.RemoveEmptyEntries));
            
            while (pathSeparated.Count > 0)
            {
                var indexOfPathPartToRemove = pathSeparated.FindIndex(s => s == "..") - 1;

                if (pathSeparated.TrueForAll(s => s == ".."))
                    return "";
                
                if (indexOfPathPartToRemove < 0)
                    break;
                
                pathSeparated.RemoveRange(indexOfPathPartToRemove, 2);
            }
            
            
            /* Join parts of the path to result path string */
            path = string.Join("\\", pathSeparated);
            
            // special case: if path is drive letter and ':' only
            if (path.Length == 2 && path[1] == ':')
                path += '\\';

            
            return path;
        }
        
        
        /// <summary>
        /// Create a name for copied file by adding the suffix and order number of copy.
        /// </summary>
        /// <param name="filePath">The path to file that will be copied.</param>
        /// <param name="dirFiles">All files from the directory where the file will be copied to.</param>
        public static string GetCopyFileName(string filePath, string[] dirFiles)
        {
            var copies = new List<string>();
            
            var extension = Path.GetExtension(filePath);
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            
            
            var copyCheckRegex = new Regex($@"({fileName} — копия \(\d+\)){extension}$");
            
            
            // find all copies of this file
            foreach (var file in dirFiles)
            {
                if (copyCheckRegex.IsMatch(file))
                    copies.Add(Path.GetFileName(file));
            }

            var copyNumber = copies.Count + 1;

            return $"{fileName} — копия ({copyNumber.ToString()}){extension}";
        }


        /// <summary>
        /// Check if the specified path is a path to drive.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True - if the path is a path to drive.
        /// False - if the path is a path to file or directory.</returns>
        public static bool IsDrive(string path)
        {
            string driveName = path.TrimEnd('\\');
            
            if (driveName.Length == 2 && driveName[1] == ':')
                return char.IsLetter(driveName[0]);

            return false;
        }


        /// <summary>
        /// Check if the specified path is a path to file.
        /// </summary>
        /// <param name="path">The path to check.</param>
        /// <returns>True - if the path is the path to file.
        /// False - if the path is a path to directory or drive.</returns>
        public static bool IsFile(string path)
        {
            return Path.HasExtension(path);
        }
    }
}