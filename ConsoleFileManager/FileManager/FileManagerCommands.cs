using System;
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
                    new Regex(@"^(gotd (""[^/*?""<>|]*"")( -p \d+)?)$"),
                    "p"
                ),
                new ConsoleCommand(
                    "cpy",
                    Copy,
                    new Regex(@"^(cpy (""[^/*?""<>|]+"") (""[^/*?""<>|]*"")( -rf (true|false))?)$"),
                    "rf"
                ),
                new ConsoleCommand(
                    "del", 
                    Delete,
                    new Regex(@"^(del (""[^/*?""<>|]*"")( -r true)?)$"),
                    "r"
                ),
                new ConsoleCommand(
                    "info",
                    GetFileInfo,
                    new Regex(@"^(info (""[^/*?""<>|]*""))$")
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
        
        
        
        private void GoToDirectory(params string[] args)
        {
            /* Check and parse arguments */
            var pathArg = args[0];
            var pageToShowArg = args[1];

            string path;
            int pageToShow;


            path = ExtraFunctional.ParsePath(pathArg, CurrentDirectory);

            // path can be null only if current directory is null
            if (path is null)
            {
                CurrentShownInfo = new Info(
                    "Невозможно перейти по указанному пути. Укажите абсолютный путь.",
                    InfoType.Error
                );
                return;
            }
            
            // check if parsed directory exists
            if (!Directory.Exists(path))
            {
                CurrentShownInfo = new Info(
                    $"Ошибка исполнения команды: указанная директория не существует.",
                    InfoType.Error
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
            /* Check and parse arguments */
            var copiedPathArg = args[0];
            var destinationPathArg = args[1];
            var replaceFileArg = args[2];

            string copiedPath;
            string destinationPath;
            bool replaceFiles;

            copiedPath = ExtraFunctional.ParsePath(copiedPathArg, CurrentDirectory);
            destinationPath = ExtraFunctional.ParsePath(destinationPathArg, CurrentDirectory);
            
            // path can be null only if current directory is null
            if (copiedPath is null)
            {
                CurrentShownInfo = new Info(
                    "Невозможно использовать указанный путь. Укажите абсолютный путь.",
                    InfoType.Error
                );
                return;
            }
            
            // check if copied file or directory exists
            if (!File.Exists(copiedPath) && !Directory.Exists(copiedPath))
            {
                CurrentShownInfo = new Info(
                    "Ошибка исполнения команды: указанный файл или директория не существует.",
                    InfoType.Error
                );
                
                return;
            }

            
            var copiedObjIsFile = Path.HasExtension(copiedPath);

            
            // check if destination path is a directory that exists
            if (!Directory.Exists(destinationPath))
            {
                CurrentShownInfo = new Info(
                    "Ошибка исполнения команды: папки назначения не существует, либо путь указывал на файл.",
                    InfoType.Error
                );
                
                return;
            }

            // parse extra arg
            replaceFiles = replaceFileArg is null ? false : bool.Parse(replaceFileArg);

            
            try
            {
                if (copiedObjIsFile)
                {
                    var newFilePath = Path.Combine(destinationPath, Path.GetFileName(copiedPath));
                    
                    // if file in destination folder exists and no extra arg was specified
                    // then warn the user
                    if (replaceFileArg is null && File.Exists(newFilePath))
                    {
                        CurrentShownInfo = new Info(
                            $"В папке назначения уже есть файл с именем {Path.GetFileName(newFilePath)}.\n" +
                            "Если вы желаете заменить файл в папке назначения, " +
                            "повторите команду с указанием аргумента замены как true:\n" +
                            $"cpy \"{copiedPath}\" \"{destinationPath}\" -rf true\n" +
                            "Если же вы желаете создать в папке назначения ещё один такой же файл, " +
                            "повторите команду с указанием аргумента замены как false:\n" +
                            $"cpy \"{copiedPath}\" \"{destinationPath}\" -rf false"
                        );
                        
                        return;
                    }

                    
                    // if replace file arg was specified as false, then create new name for the copied file
                    if (!replaceFiles && File.Exists(newFilePath))
                    {
                        var newFileName =  ExtraFunctional.GetCopyFileName(
                            newFilePath,
                            Directory.GetFiles(destinationPath)
                        );

                        newFilePath = Path.Combine(destinationPath, newFileName);
                    }
                    
                    
                    File.Copy(copiedPath, newFilePath, replaceFiles);
                    
                    CurrentShownInfo = Info.Empty;
                }
                else
                {
                    if (!Directory.Exists(destinationPath))
                        Directory.CreateDirectory(destinationPath);
                    
                    
                    // show a stub window so that the user knows that the program is not frozen
                    CurrentShownInfo = new Info("Идёт операция копирования файлов. Пожалуйста, подождите...");
                    ShowInfoWindow("Операция");
                    
                    
                    // recursively copy all files and dirs to another dir
                    var copiedSuccessfully = RecursiveFilesCopy(copiedPath, destinationPath, replaceFiles);
                    
                    CurrentShownInfo = copiedSuccessfully ? Info.Empty : new Info("Операция копирования была прервана.");
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);

                CurrentShownInfo = new Info(
                    $"Произошла ошибка при попытке скопировать {(copiedObjIsFile ? "файл" : "папку")}: {e.Message}",
                    InfoType.Error
                );
            }
        }

        
        
        private void Delete(params string[] args)
        {
            /* Check and parse arguments */
            var pathArg = args[0];
            var recursiveArg = args[1];

            string path;
            bool recursiveDeletion;

            path = ExtraFunctional.ParsePath(pathArg, CurrentDirectory);
            
            // path can be null only if current directory is null
            if (path is null)
            {
                CurrentShownInfo = new Info(
                    "Невозможно использовать указанный путь. Укажите абсолютный путь.",
                    InfoType.Error
                );
                return;
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                CurrentShownInfo = new Info(
                    "Ошибка исполнения команды: указанный файл или директория не существует.",
                    InfoType.Error
                );
                return;
            }

            recursiveDeletion = recursiveArg != null;
            
            
            var isFile = Path.HasExtension(path);
            
            try
            {
                if (isFile)
                    File.Delete(path);
                else
                {
                    if (Directory.GetFileSystemEntries(path).Length != 0 && recursiveDeletion == false)
                    {
                        CurrentShownInfo = new Info(
                            "Указанная директория не пуста. Если вы желаете удалить папку вместе со всем её " +
                            "содержимым, повторите команду, указав аргумент рекурсивного удаления:\n" +
                            $"del \"{path}\" -r true"
                        );
                        
                        return;
                    }
                    
                    
                    // show a stub window so that the user knows that the program is not frozen
                    CurrentShownInfo = new Info("Идёт операция удаления файлов. Пожалуйста, подождите...");
                    ShowInfoWindow("Операция");
                    
                    
                    // recursively delete all files and dirs
                    var deletedSuccessfully = RecursiveFilesDeletion(path);
                    
                    if (deletedSuccessfully)
                        Directory.Delete(path);
                    
                    
                    CurrentShownInfo = deletedSuccessfully ? Info.Empty : new Info("Операция удаления была прервана.");
                    
                    if (CurrentDirectory == path)
                        CurrentDirectory = Path.GetDirectoryName(path.TrimEnd('\\'));
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);

                CurrentShownInfo = new Info(
                    $"Произошла ошибка при попытке удалить {(isFile ? "файл" : "папку")}: {e.Message}",
                    InfoType.Error
                );
            }
        }

        
        
        private void GetFileInfo(params string[] args)
        {
            /* Check and parse path to the file */
            var pathArg = args[0];

            string path = ExtraFunctional.ParsePath(pathArg, CurrentDirectory);
            
            
            // path can be null only if current directory is null
            if (path is null)
            {
                CurrentShownInfo = new Info(
                    "Невозможно использовать указанный путь. Укажите абсолютный путь.",
                    InfoType.Error
                );
                return;
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                CurrentShownInfo = new Info(
                    "Ошибка исполнения команды: указанный файл или директория не существует.",
                    InfoType.Error
                );
                return;
            }
            
            
            /* Get info */
            string info;
            
            // main information
            string name;
            string extension;
            string location;

            // size information
            long sizeBytes;
            
            FileAttributes attributes;

            
            try
            {
                if (ExtraFunctional.IsDrive(path))
                {
                    var driveInfo = new DriveInfo(path);
                    info = CreateDriveInfo(driveInfo);
                }
                else if (ExtraFunctional.IsFile(path))
                {
                    /* Collect main information */
                    name = Path.GetFileNameWithoutExtension(path);
                    extension = Path.GetExtension(path);
                    location = Path.GetDirectoryName(path);

                    attributes = File.GetAttributes(path);
                
                    
                    /* Try to get the size of the file */
                    try
                    {
                        var dirFilesInfo = new List<FileInfo>(new DirectoryInfo(location).GetFiles());
                        FileInfo fileInfo = dirFilesInfo.Find(fInfo => fInfo.Name == (name + extension));
                    
                        sizeBytes = fileInfo.Length;
                    }
                    catch (Exception e)
                    {
                        // exception will be thrown only if app has no rights to access the file or folder
                        // in that case just warn the user about it and show the size of the file as 'Неизвестно'
                        
                        sizeBytes = -1;

                        ShowFileOperationDialog(
                            "Отказано в доступе к файлу",
                            "У приложения отсутствуют необходимые права для получения информации о размерах" +
                            $" файла {name}{extension}. Размер файла будет указан, как \"Неизвестно\".",
                            "продолжить",
                            null,
                            null
                        );
                    }

                    
                    /* Create info string */
                    info = CreateFileInfo(
                        name,
                        extension,
                        location,
                        sizeBytes,
                        File.GetCreationTime(path),
                        File.GetLastWriteTime(path),
                        File.GetLastAccessTime(path),
                        attributes
                    );
                }
                else
                {
                    name = Path.GetFileNameWithoutExtension(path);
                    location = Path.GetDirectoryName(path);
                    
                    
                    /* Try to get the size of the directory and its attributes */
                    var dirDirsInfo = new List<DirectoryInfo>(new DirectoryInfo(location).GetDirectories());
                    DirectoryInfo dirInfo = dirDirsInfo.Find(dInfo => dInfo.Name == name);

                    attributes = dirInfo.Attributes;

                    
                    // show info window to let user know that application is not frozen
                    Console.Clear();
                    CurrentShownInfo = new Info(
                        "Выполняется вычисление размера папки. Пожалуйста, подождите..."
                    );
                    ShowInfoWindow("Вычисление размера папки");
                    
                    
                    try
                    {
                        sizeBytes = dirInfo.EnumerateFiles("*", SearchOption.AllDirectories).Sum(fi => fi.Length);
                    }
                    catch (Exception e)
                    {
                        // exception will be thrown only if app has no rights to access the file or folder
                        // in that case just warn the user about it and show the size of the file as 'Неизвестно'
                        
                        sizeBytes = -1;

                        ShowFileOperationDialog(
                            "Отказано в доступе к файлу",
                            "У приложения отсутствуют необходимые права для получения информации о размерах" +
                            $" папки {name}. Размер папки будет указан, как \"Неизвестно\".",
                            "продолжить",
                            null,
                            null
                        );
                    }
                    
                    
                    /* Create info string */
                    info = CreateDirInfo(
                        name,
                        location,
                        sizeBytes,
                        dirInfo.CreationTime,
                        attributes
                    );
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);

                var type = ExtraFunctional.IsDrive(path) ? "диске" : (ExtraFunctional.IsFile(path) ? "файле" : "папке");
                
                CurrentShownInfo = new Info(
                    $"Произошла ошибка при попытке получить информацию о {type}: {e.Message}",
                    InfoType.Error
                );
                
                return;
            }


            CurrentShownInfo = new Info(info, InfoType.FileInfo);
        }

        private void Exit(params string[] args)
        {
            isExiting = true;
        }
        
    }
}