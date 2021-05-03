using System;
using System.Globalization;
using System.IO;

namespace ConsoleFileManager.FileManager
{
    public partial class FileManager
    {
        /// <summary>
        /// Recursively copy all files from specified directory to specified directory. <para/>
        /// This method calls itself on each directory in the specified directory.
        /// </summary>
        /// <param name="fromPath">The path to the directory to copy files from.</param>
        /// <param name="toPath">The path to the directory to copy files to.</param>
        /// <param name="replaceAllFiles">The value indicating whether the dialog window should be shown if
        /// file with the same name exists in destination directory.</param>
        /// <returns>True - if operation was successful. False - if operation was aborted. </returns>
        private bool RecursiveFilesCopy(string fromPath, string toPath, bool replaceAllFiles)
        {
            string[] files = null;
            string[] dirs = null;


            /* Try to get files and directories from current directory */
            try
            {
                files = Directory.GetFiles(fromPath);
                dirs = Directory.GetDirectories(fromPath);
            }
            catch (UnauthorizedAccessException)
            {
                var directoryAccessResult = ShowFileOperationDialog(
                    "Отсутствуют права доступа",
                    $"Операция не может быть завершена, так отсутствуют права доступа к папке {Path.GetFileName(fromPath)}.",
                    null,
                    "пропустить эту папку",
                    "прервать операцию"
                );

                // show a stub window so that the user knows that the program is not frozen
                CurrentShownInfo = new Info("Идёт операция удаления файлов. Пожалуйста, подождите...");
                ShowInfoWindow("Операция");


                if (directoryAccessResult == FileOperationDialogResult.Skip)
                    return true;

                if (directoryAccessResult == FileOperationDialogResult.Abort)
                    return false;
            }

            
            // if there is no dirs and files in the directory, then go to the previous recursion level
            // and return value indicating that operation is not aborted
            if (files.Length == 0 && dirs.Length == 0)
                return true;

            
            /* Copy all files from current directory */
            foreach (var file in files)
            {
                var copiedFilePath = Path.Combine(toPath, Path.GetFileName(file));
                var copyResult = FileOperationDialogResult.TryAgain;
                var replacementResult = FileOperationDialogResult.Skip;

                
                // if files are not being replaced automatically then
                // check file existence until user skips the file or aborts the operation in the dialog
                if (!replaceAllFiles && File.Exists(copiedFilePath))
                {
                    // show dialog to get what to do with current file
                    copyResult = ShowFileOperationDialog(
                        "Замена или пропуск файлов",
                        $"В папке назначения уже есть файл {Path.GetFileName(file)}",
                        "заменить файл в папке назначения",
                        "пропустить этот файл",
                        "прервать операцию"
                    );
                    
                    
                    // show a stub window so that the user knows that the program is not frozen
                    CurrentShownInfo = new Info("Идёт операция копирования файлов. Пожалуйста, подождите...");
                    ShowInfoWindow("Операция");
                    
                    
                    if (copyResult == FileOperationDialogResult.Skip)
                        continue;
                    
                    if (copyResult == FileOperationDialogResult.Abort)
                        return false;
                }

                
                // if copy result is TryAgain then file should be replaced
                bool replace = copyResult == FileOperationDialogResult.TryAgain;
                
                
                // try to replace file until file is replaced, or user skips the file, or operation is aborted
                do
                {
                    try
                    {
                        File.Copy(file, copiedFilePath, replace);
                        break;
                    }
                    catch (IOException e)
                    {
                        // handle exception only if file is occupied by another process
                        if (e.GetType().IsSubclassOf(typeof(IOException)))
                            throw;
                        
                        replacementResult = ShowFileOperationDialog(
                            "Файл уже используется",
                            $"Операция не может быть завершена, так как файл {Path.GetFileName(file)} " +
                            "открыт в другой программе.\nЗакройте программу и повторите попытку.",
                            "повторить попытку",
                            "пропустить этот файл",
                            "прервать операцию"
                        );

                        // show a stub window so that the user knows that the program is not frozen
                        CurrentShownInfo = new Info("Идёт операция копирования файлов. Пожалуйста, подождите...");
                        ShowInfoWindow("Операция");


                        if (replacementResult == FileOperationDialogResult.Skip)
                            break;

                        if (replacementResult == FileOperationDialogResult.Abort)
                            return false;
                    }
                } while (replacementResult == FileOperationDialogResult.TryAgain);
            }

            
            /* Recursively copy all files from directories in current directory */
            foreach (var dir in dirs)
            {
                var destinationPath = Path.Combine(toPath, Path.GetFileName(dir));

                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);
                
                bool copiedSuccessfully = RecursiveFilesCopy(dir, destinationPath, replaceAllFiles);

                if (!copiedSuccessfully)
                    return false;
            }
            
            return true;
        }
        
        
        
        /// <summary>
        /// Recursively delete all files in specified directory.
        /// </summary>
        /// <param name="dirPath">The path to the directory to delete files from.</param>
        /// <returns>True - if operation was successful. False - if operation was aborted. </returns>
        private bool RecursiveFilesDeletion(string dirPath)
        {
            string[] files = null;
            string[] dirs = null;


            /* Try to get files and directories from current directory */
            try
            {
                files = Directory.GetFiles(dirPath);
                dirs = Directory.GetDirectories(dirPath);
            }
            catch (UnauthorizedAccessException)
            {
                var directoryAccessResult = ShowFileOperationDialog(
                    "Отсутствуют права доступа",
                    $"Операция не может быть завершена, так отсутствуют права доступа к папке {Path.GetFileName(dirPath)}.",
                    null,
                    "пропустить эту папку",
                    "прервать операцию"
                );

                // show a stub window so that the user knows that the program is not frozen
                CurrentShownInfo = new Info("Идёт операция удаления файлов. Пожалуйста, подождите...");
                ShowInfoWindow("Операция");


                if (directoryAccessResult == FileOperationDialogResult.Skip)
                    return true;

                if (directoryAccessResult == FileOperationDialogResult.Abort)
                    return false;
            }


            // if there is no dirs and files in the directory, then go to the previous recursion level
            // and return value indicating that operation is not aborted
            if (files.Length == 0 && dirs.Length == 0)
                return true;

            
            /* Delete all files from current directory */
            foreach (var file in files)
            {
                FileOperationDialogResult deletionResult;
                
                // try to delete file until file is deleted, or user skips the file, or operation is aborted
                do
                {
                    try
                    {
                        File.Delete(file);
                        break;
                    }
                    catch (IOException e)
                    {
                        // handle exception only if file is occupied by another process
                        if (e.GetType().IsSubclassOf(typeof(IOException)))
                            throw;

                        deletionResult = ShowFileOperationDialog(
                            "Файл уже используется",
                            $"Операция не может быть завершена, так как файл {Path.GetFileName(file)} " +
                            "открыт в другой программе.\nЗакройте программу и повторите попытку.",
                            "повторить попытку",
                            "пропустить этот файл",
                            "прервать операцию"
                        );

                        // show a stub window so that the user knows that the program is not frozen
                        CurrentShownInfo = new Info("Идёт операция удаления файлов. Пожалуйста, подождите...");
                        ShowInfoWindow("Операция");


                        if (deletionResult == FileOperationDialogResult.Skip)
                            break;

                        if (deletionResult == FileOperationDialogResult.Abort)
                            return false;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        deletionResult = ShowFileOperationDialog(
                            "Отсутствуют права доступа",
                            $"Операция не может быть завершена, так отсутствуют права доступа к файлу {Path.GetFileName(file)}.",
                            null,
                            "пропустить этот файл",
                            "прервать операцию"
                        );

                        // show a stub window so that the user knows that the program is not frozen
                        CurrentShownInfo = new Info("Идёт операция удаления файлов. Пожалуйста, подождите...");
                        ShowInfoWindow("Операция");


                        if (deletionResult == FileOperationDialogResult.Skip)
                            break;

                        if (deletionResult == FileOperationDialogResult.Abort)
                            return false;
                    }
                    
                } while (deletionResult == FileOperationDialogResult.TryAgain);
            }

            
            /* Recursively delete all files from directories in current directory */
            foreach (var dir in dirs)
            {
                // get path for the next directory to delete
                var nextDir = Path.Combine(dirPath, Path.GetFileName(dir));

                // delete all files and folders in a directory by the specified path
                bool deletedSuccessfully = RecursiveFilesDeletion(nextDir);

                // if operation was aborted then go to the upper recursion level
                if (!deletedSuccessfully)
                    return false;

                // delete the directory only if it is empty
                if (Directory.GetFileSystemEntries(nextDir).Length == 0)
                {
                    var deletionResult = FileOperationDialogResult.Skip;

                    do
                    {
                        try
                        {
                            Directory.Delete(nextDir);
                        }
                        catch (Exception e)
                        {
                            // handle exception only if file is occupied by another process
                            if (e.GetType().IsSubclassOf(typeof(IOException)))
                                throw;

                            deletionResult = ShowFileOperationDialog(
                                "Файл уже используется",
                                $"Операция не может быть завершена, так как папка {Path.GetFileName(nextDir)} " +
                                "открыта в другой программе.\nЗакройте программу и повторите попытку.",
                                "повторить попытку",
                                "пропустить эту папку",
                                "прервать операцию"
                            );

                            // show a stub window so that the user knows that the program is not frozen
                            CurrentShownInfo = new Info("Идёт операция удаления файлов. Пожалуйста, подождите...");
                            ShowInfoWindow("Операция");


                            if (deletionResult == FileOperationDialogResult.Skip)
                                break;

                            if (deletionResult == FileOperationDialogResult.Abort)
                                return false;
                        }
                    } while (deletionResult == FileOperationDialogResult.TryAgain);
                }
            }
            
            return true;
        }


         /// <summary>
         /// Create a string with information about attributes of file or directory.
         /// </summary>
         /// <param name="attributes">The attributes of the file or directory.</param>
         /// <param name="isFile">The value indicating whether the specified attributes are file attributes or directory attributes.</param>
         private string GetAttributesInfo(FileAttributes attributes, bool isFile)
         {
             var attributesInfo = "";
             
             if ((attributes & FileAttributes.ReadOnly) != 0) 
                 attributesInfo += $"\t      Только для чтения{(isFile? "" : " (применимо только к файлам в каталоге)")}\n";
                
             if ((attributes & FileAttributes.Hidden) != 0) 
                 attributesInfo += "\t      Скрытый\n";

             if (isFile && (attributes & FileAttributes.Temporary) != 0)
                 attributesInfo += "\t      Временный файл\n";

             if (isFile && (attributes & FileAttributes.System) != 0)
                 attributesInfo += "\t      Системный файл\n";
            
             if (!isFile && (attributes & FileAttributes.Directory) != 0)
                 attributesInfo += "\t      Каталог\n";

             if ((attributes & FileAttributes.Device) != 0)
                 attributesInfo += "\t      Зарезервирован для будущего использования\n";

             if ((attributes & FileAttributes.Archive) != 0)
                 attributesInfo += $"\t      {(isFile? "Файл" : "Каталог")} готов для архивирования\n";

             if ((attributes & FileAttributes.NotContentIndexed) == 0)
                 attributesInfo += $"\t      Содержимое {(isFile? "этого файла" : "файлов этого каталога")} индексируется в дополнение к свойствам файла\n";

             if ((attributes & FileAttributes.Compressed) != 0)
                 attributesInfo += "\t      Содержимое сжато для экономии места на диске\n";

             if ((attributes & FileAttributes.Encrypted) != 0)
                 attributesInfo += "\t      Содержимое шифруется для защиты данных\n";

             return attributesInfo;
         }
         

         
         /// <summary>
         /// Create a string with information about the file from the specified values.
         /// </summary>
         /// <param name="name">The name of the file.</param>
         /// <param name="extension">The extension of the file.</param>
         /// <param name="location">The location of the file.</param>
         /// <param name="sizeInBytes">The size of the file in bytes.</param>
         /// <param name="creationTime">The time when the file was created.</param>
         /// <param name="lastChangeTime">The time when the file was last changed.</param>
         /// <param name="lastOpenTime">The time when the file was last opened.</param>
         /// <param name="attributes">The attributes of the file.</param>
         private string CreateFileInfo(string name, string extension, string location, long sizeInBytes, 
             DateTime creationTime, DateTime lastChangeTime, DateTime lastOpenTime, FileAttributes attributes)
         {
             string sizeStr;

             if (sizeInBytes >= 0)
             {
                 var sizeInBytesStr = sizeInBytes.ToString(CultureInfo.CurrentCulture) + " байт";
                 var sizeNormalizedStr =
                     ExtraFunctional.GetNormalizedSize(sizeInBytes, out var type).ToString(CultureInfo.CurrentCulture) +
                     $" {type}";

                 sizeStr = sizeNormalizedStr + " (" + sizeInBytesStr + ")";
             }
             else
                 sizeStr = "Неизвестно";
             

             var creationTimeStr = creationTime.ToString(CultureInfo.CurrentCulture);
             var lastChangeTimeStr = lastChangeTime.ToString(CultureInfo.CurrentCulture);
             var lastOpenTimeStr = lastOpenTime.ToString(CultureInfo.CurrentCulture);

             var attributesStr = GetAttributesInfo(attributes, true);
             
             
             var info =
                 $"Имя:          {name}\n" +
                 $"Тип:          Файл ({extension})\n" +
                 $"Расположение: {location}\n" +
                 $"Размер:       {sizeStr}\n" +
                 $"Создан:       {creationTimeStr}\n" +
                 $"Изменен:      {lastChangeTimeStr}\n" +
                 $"Открыт:       {lastOpenTimeStr}\n" +
                 "\n" +
                 $"Атрибуты:\n{attributesStr}";

             return info;
         }


         
         /// <summary>
         /// Create a string with information about directory from the specified values.
         /// </summary>
         /// <param name="name">The name of the directory.</param>
         /// <param name="location">The location of the directory.</param>
         /// <param name="sizeInBytes">The size of the directory in bytes.</param>
         /// <param name="creationTime">The time when the directory was created.</param>
         /// <param name="attributes">The attributes of the directory.</param>
         private string CreateDirInfo(string name, string location, long sizeInBytes, DateTime creationTime,
             FileAttributes attributes)
         {
             string sizeStr;

             if (sizeInBytes >= 0)
             {
                 var sizeInBytesStr = sizeInBytes.ToString(CultureInfo.CurrentCulture) + " байт";
                 var sizeNormalizedStr =
                     ExtraFunctional.GetNormalizedSize(sizeInBytes, out var type).ToString(CultureInfo.CurrentCulture) +
                     $" {type}";

                 sizeStr = sizeNormalizedStr + " (" + sizeInBytesStr + ")";
             }
             else
                 sizeStr = "Неизвестно";
             

             var creationTimeStr = creationTime.ToString(CultureInfo.CurrentCulture);
             var attributesStr = GetAttributesInfo(attributes, true);
             
             
             var info =
                 $"Имя:          {name}\n" +
                  "Тип:          Папка с файлами\n" +
                 $"Расположение: {location}\n" +
                 $"Размер:       {sizeStr}\n" +
                 $"Создан:       {creationTimeStr}\n" +
                 "\n" +
                 $"Атрибуты:\n{attributesStr}";

             return info;
         }


         
         /// <summary>
         /// Create a string with information about the drive from the specified drive info.
         /// </summary>
         /// <param name="driveInfo">The information about the drive.</param>
         private string CreateDriveInfo(DriveInfo driveInfo)
         {
             string driveType = driveInfo.DriveType switch {
                 DriveType.Fixed => "Локальный диск",
                 DriveType.Network => "Сетевой диск",
                 DriveType.Removable => "USB-накопитель",
                 DriveType.CDRom => "Оптический диск",
                 _ => "Неизвестно"
             };

             var totalSpaceInBytes = driveInfo.TotalSize;
             var freeSpaceInBytes = driveInfo.AvailableFreeSpace;
             var usedSpaceInBytes = totalSpaceInBytes - freeSpaceInBytes;
             
             var totalSpaceNormalized = ExtraFunctional.GetNormalizedSize(totalSpaceInBytes, out var totalSpaceSizeType);
             var freeSpaceNormalized = ExtraFunctional.GetNormalizedSize(freeSpaceInBytes, out var freeSpaceSizeType);
             var usedSpaceNormalized = ExtraFunctional.GetNormalizedSize(usedSpaceInBytes, out var usedSpaceSizeType);

             var usedSpaceStr = $"{usedSpaceNormalized.ToString(CultureInfo.CurrentCulture)} {usedSpaceSizeType} " +
                                $"({usedSpaceInBytes.ToString(CultureInfo.CurrentCulture)} байт)";
             
             var freeSpaceStr = $"{freeSpaceNormalized.ToString(CultureInfo.CurrentCulture)} {freeSpaceSizeType} " +
                                $"({freeSpaceInBytes.ToString(CultureInfo.CurrentCulture)} байт)";
             
             var totalSpaceStr = $"{totalSpaceNormalized.ToString(CultureInfo.CurrentCulture)} {totalSpaceSizeType} " +
                                 $"({totalSpaceInBytes.ToString(CultureInfo.CurrentCulture)} байт)";


             var info =
                 $"Имя:              {driveInfo.Name}\n" +
                 $"Тип:              {driveType}\n" +
                 $"Файловая система: {driveInfo.DriveFormat}\n" +
                 $"Занято:           {usedSpaceStr}\n" +
                 $"Свободно:         {freeSpaceStr}\n" +
                 $"Емкость:          {totalSpaceStr}\n";

             return info;
         }
    }
}