using System;
using System.Globalization;
using System.IO;

namespace ConsoleFileManager.FileManager
{
    public partial class FileManager
    {
         private bool RecursiveFilesCopy(string fromPath, string toPath, bool replaceAllFiles)
        {
            var files = Directory.GetFiles(fromPath);
            var dirs = Directory.GetDirectories(fromPath);

            // if there is no dirs and files in the directory, then go to the previous recursion level
            // and return value indicating that operation is not aborted
            if (files.Length == 0 && dirs.Length == 0)
                return true;

            
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
                    CurrentShownInfo = new Info("Подождите... идёт операция копирования.");
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
                        CurrentShownInfo = new Info("Подождите... идёт операция копирования.");
                        ShowInfoWindow("Операция");


                        if (replacementResult == FileOperationDialogResult.Skip)
                            break;

                        if (replacementResult == FileOperationDialogResult.Abort)
                            return false;
                    }
                } while (replacementResult == FileOperationDialogResult.TryAgain);
            }

            
            // recursively copy all files from directories in current directory
            foreach (var dir in dirs)
            {
                var destinationPath = Path.Combine(toPath, Path.GetFileName(dir));

                if (!Directory.Exists(destinationPath))
                    Directory.CreateDirectory(destinationPath);
                
                return RecursiveFilesCopy(dir, destinationPath, replaceAllFiles);
            }

            
            return true;
        }


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