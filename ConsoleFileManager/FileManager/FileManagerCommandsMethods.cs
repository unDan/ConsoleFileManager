using System;
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

        
    }
}