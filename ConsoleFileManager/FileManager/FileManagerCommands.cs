﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ConsoleFileManager.Properties;

namespace ConsoleFileManager.FileManager
{
    public partial class FileManager
    {
        private List<ConsoleCommand> commands;


        private void InitializeCommands()
        {
            commands = new List<ConsoleCommand> {
                new ConsoleCommand(
                    "gotd", 
                    GoToDirectory, 
                    new Regex(@"^(gotd (""[^/*?""<>|]+"")( -p \d+)?)$"),
                    "p"
                ),
                new ConsoleCommand(
                    "cpy",
                    Copy,
                    new Regex(@"^(cpy (""[^/*?""<>|]+"") (""[^/*?""<>|]+"")( -rf (true|false))?)$"),
                    "rf"
                ),
                new ConsoleCommand(
                    "del", 
                    Delete,
                    new Regex(@"^(del (""[^/*?""<>|]+"")( -r true)?)$"),
                    "r"
                ),
                new ConsoleCommand(
                    "info",
                    GetFileInfo,
                    new Regex(@"^(info (""[^/*?""<>|]+""))$")
                ),
                new ConsoleCommand(
                    "exit", 
                    Exit, 
                    new Regex(@"^(exit)$")
                )
            };
        }
        

        public void ExecuteCommand(string commandStr)
        {
            /* Check if command entered correctly */
            ConsoleCommand command = null;
            var isCorrect = false;

            // check if entered command string matches any command signature
            for (var i = 0; i < commands.Count && !isCorrect; i++)
            {
                command = commands[i];
                isCorrect = command.IsMatchSignature(commandStr);
            }

            if (!isCorrect)
            {
                CurrentShownInfo = new Info(
                    "Команда не распознана. Такое может быть, если такой команды не существует, либо, если один" +
                    " или несколько аргументов имеют неверный формат.",
                    InfoType.Warning
                );
                
                return;
            }

            var args = command.GetCommandArgs(commandStr);
            command.Execute(args);

        }


        private string ParsePath(string pathToParse, string currentDirectory)
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
        

        private void GoToDirectory(params string[] args)
        {
            /* Check and parse arguments */
            var pathArg = args[0];
            var pageToShowArg = args[1];

            string path;
            int pageToShow;


            path = ParsePath(pathArg, CurrentDirectory);

            // path can be null only if current directory is null
            if (path is null)
            {
                CurrentShownInfo = new Info(
                    "Невозможно перейти по указанному пути. Укажите абсолютный путь.",
                    InfoType.Warning
                );
                return;
            }
            
            // check if parsed directory exists
            if (!Directory.Exists(path))
            {
                CurrentShownInfo = new Info(
                    $"Ошибка исполнения команды: указанная директория не существует.",
                    InfoType.Warning
                );
                return;
            }

            
            // if directory from command is ok then check if page number is ok
            var parsedPageNum = (pageToShowArg is null) ? Settings.Default.showPageDefault : int.Parse(pageToShowArg);

            pageToShow = (parsedPageNum == 0) ? Settings.Default.showPageDefault : parsedPageNum;
            
            
            /* After the arguments checked, apply new current path and new current page */
            CurrentDirectory = path;
            CurrentShownPage = pageToShow;
            CurrentShownInfo = Info.Empty;
        }

        
        private void Copy(params string[] args)
        {
            
        }

        private void Delete(params string[] args)
        {
            
        }

        private void GetFileInfo(params string[] args)
        {
            /* Check and parse path to the file */
            var pathArg = args[0];

            string path = ParsePath(pathArg, CurrentDirectory);
            
            // path can be null only if current directory is null
            if (path is null)
            {
                CurrentShownInfo = new Info(
                    "Невозможно перейти по указанному пути. Укажите абсолютный путь.",
                    InfoType.Warning
                );
                return;
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                CurrentShownInfo = new Info(
                    "Ошибка исполнения команды: указанный файл или директория не существует.",
                    InfoType.Warning
                );
                return;
            }
            
            
            /* Get info */
            var isFile = Path.HasExtension(path);
            string info;
            
            // main information
            string name;
            string extension;
            string location;

            // size information
            string sizeType = "";
            double sizeBytes;
            double sizeNormalized;
                
            // time information
            DateTime creationTime;
            DateTime lastChangeTime;
            DateTime lastOpenedTime;
                
            FileAttributes attributes;

            try
            {
                if (isFile)
                {
                    attributes = File.GetAttributes(path);

                    
                    name = Path.GetFileNameWithoutExtension(path);
                    extension = Path.GetExtension(path);
                    location = Path.GetDirectoryName(path);
                
                    
                    creationTime = File.GetCreationTime(path);
                    lastChangeTime = File.GetLastWriteTime(path);
                    lastOpenedTime = File.GetLastAccessTime(path);
                    
                    
                    /* Try to get the size of the file */
                    var dirFilesInfo = new List<FileInfo>(new DirectoryInfo(location).GetFiles());
                    FileInfo fileInfo = dirFilesInfo.Find(fInfo => fInfo.Name == (name + extension));
                    
                    sizeBytes = fileInfo is null ? -1 : fileInfo.Length;
                    sizeNormalized = fileInfo is null ? -1 : Converter.GetNormalizedSize(sizeBytes, out sizeType);
                    
                    
                    /* Join all retrieved data to info string */
                    string sizeNormStr;
                    string sizeBytesStr;

                    if (sizeBytes > 0d)
                    {
                        sizeNormStr = sizeNormalized.ToString(CultureInfo.CurrentCulture) + " " + sizeType;
                        sizeBytesStr = sizeBytes.ToString(CultureInfo.CurrentCulture) + " байт";
                    }
                    else
                    {
                        sizeNormStr = "неизвестно";
                        sizeBytesStr = "незивестно";
                    }

                    info =
                        $"Имя:          {name}\n" +
                        $"Тип:          Файл ({extension})\n" +
                        $"Расположение: {location}\n" +
                        $"Размер:       {sizeNormStr} ({sizeBytesStr})\n" +
                        $"Создан:       {creationTime.ToString(CultureInfo.CurrentCulture)}\n" +
                        $"Изменен:      {lastChangeTime.ToString(CultureInfo.CurrentCulture)}\n" +
                        $"Открыт:       {lastOpenedTime.ToString(CultureInfo.CurrentCulture)}\n" +
                        "\n" +
                        "Атрибуты:\n";
                }
                else
                {
                    name = Path.GetFileNameWithoutExtension(path);
                    location = Path.GetDirectoryName(path);
                
                    
                    // get time info
                    creationTime = File.GetCreationTime(path);
                    
                    
                    // get the size of the file and its attributes
                    var dirDirsInfo = new List<DirectoryInfo>(new DirectoryInfo(location).GetDirectories());
                    DirectoryInfo dirInfo = dirDirsInfo.Find(dInfo => dInfo.Name == name);

                    attributes = dirInfo.Attributes;
                    

                    try
                    {
                        sizeBytes = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                        sizeNormalized = Converter.GetNormalizedSize(sizeBytes, out sizeType);
                    }
                    catch (Exception e)
                    {
                        ErrorLogger.LogError(e);
                        sizeBytes = -1;
                        sizeNormalized = -1;
                    }
                    
                
                    // create info string
                    string sizeNormStr;
                    string sizeBytesStr;

                    if (sizeBytes > 0d)
                    {
                        sizeNormStr = sizeNormalized.ToString(CultureInfo.CurrentCulture) + " " + sizeType;
                        sizeBytesStr = sizeBytes.ToString(CultureInfo.CurrentCulture) + " байт";
                    }
                    else
                    {
                        sizeNormStr = "неизвестно";
                        sizeBytesStr = "неизвестно";
                    }
                    

                    info =
                        $"Имя:          {name}\n" +
                        $"Тип:          Папка с файлами\n" +
                        $"Расположение: {location}\n" +
                        $"Размер:       {sizeNormStr} ({sizeBytesStr})\n" +
                        $"Создан:       {creationTime.ToString(CultureInfo.CurrentCulture)}\n" +
                        "\n" +
                        "Атрибуты:\n";
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);

                var type = isFile ? "файле" : "папке";
                
                CurrentShownInfo = new Info(
                    $"Произошла ошибка при попытке получить информацию о {type}: {e.Message}",
                    InfoType.Error
                );
                
                return;
            }
            
            
            /* Check attributes and add them to the info string */
            if ((attributes & FileAttributes.ReadOnly) != 0) 
                info += $"\t      Только для чтения{(isFile? "" : " (применимо только к файлам в каталоге)")}\n";
                
            if ((attributes & FileAttributes.Hidden) != 0) 
                info += "\t      Скрытый\n";

            if (isFile && (attributes & FileAttributes.Temporary) != 0)
                info += "\t      Временный файл\n";

            if (isFile && (attributes & FileAttributes.System) != 0)
                info += "\t      Системный файл\n";
            
            if (!isFile && (attributes & FileAttributes.Directory) != 0)
                info += "\t      Каталог\n";

            if ((attributes & FileAttributes.Device) != 0)
                info += "\t      Зарезервирован для будущего использования\n";

            if ((attributes & FileAttributes.Archive) != 0)
                info += $"\t      {(isFile? "Файл" : "Каталог")} готов для архивирования\n";

            if ((attributes & FileAttributes.NotContentIndexed) == 0)
                info += $"\t      Содержимое {(isFile? "этого файла" : "файлов этого каталога")} индексируется в дополнение к свойствам файла\n";

            if ((attributes & FileAttributes.Compressed) != 0)
                info += "\t      Содержимое сжато для экономии места на диске\n";

            if ((attributes & FileAttributes.Encrypted) != 0)
                info += "\t      Содержимое шифруется для защиты данных\n";
            
            
            CurrentShownInfo = new Info(info, InfoType.FileInfo);
        }

        private void Exit(params string[] args)
        {
            isExiting = true;
        }
        
    }
}