using System;
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
        
        
        public static string GetCopyFileName(string fileName, string extension)
        {
            var copyCheckRegex = new Regex(@"( — копия \(\d+\))$");
            
            // if file name ends with ' - копия' или ' - копия (число)'
            if (copyCheckRegex.IsMatch(fileName))
            {
                var firstDigitIndex = fileName.LastIndexOf('(') + 1;
                var lastDigitIndex = fileName.LastIndexOf(')') - 1;

                var amount = int.Parse(fileName.Substring(firstDigitIndex, lastDigitIndex - firstDigitIndex + 1));
                amount++;

                return copyCheckRegex.Replace(fileName, $" — копия ({amount})") + extension;
            }
            else
                return fileName + " — копия (1)" + extension;
        }
    }
}