using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ConsoleFileManager
{
    public static class ExtraFunctional
    {
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
        
        
        public static string ParsePath(string pathToParse, string currentDirectory)
        {
            string path;
            
            // if path starts do not starts with '\' or '..\' then just add the path to the current directory
            if (!Path.IsPathRooted(pathToParse) || pathToParse.StartsWith("..\\"))
                path = (currentDirectory is null) ? null : Path.Combine(currentDirectory, pathToParse);
            
            else if (pathToParse.StartsWith("\\"))
                path = (currentDirectory is null) ? null : Path.Combine(currentDirectory, pathToParse.TrimStart('\\'));
            
            else
                path = pathToParse;

            return path;
        }
        
        
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


        public static bool IsDrive(string path)
        {
            string driveName = path.TrimEnd('\\');
            
            if (driveName.Length == 2 && driveName[1] == ':')
                return char.IsLetter(driveName[0]);

            return false;
        }


        public static bool IsFile(string path)
        {
            return Path.HasExtension(path);
        }
    }
}