using System;
using System.Collections.Generic;
using System.IO;
using ConsoleFileManager.Properties;
using System.Text.Json;

namespace ConsoleFileManager.FileManager
{
    public partial class FileManager
    {
        private static FileManager instance;
        
        
        /// <summary> The configuration of the application. </summary>
        private AppConfig config;
        
        /// <summary> The flag showing whether the application is finishing its work at the moment. </summary>
        private bool isExiting = false;
        

        /// <summary> Get copy of the configuration of the application. </summary>
        public AppConfig Config => new AppConfig {
                BackgroundColor = config.BackgroundColor,
                ForegroundColor = config.ForegroundColor,
                FilesPerPage = config.FilesPerPage,
                WindowBorderSymbol = config.WindowBorderSymbol
        };

        /// <summary> Get the path of the current shown directory. </summary>
        public string CurrentDirectory { get; private set; }
        
        /// <summary> Get the number of the current shown page. </summary>
        public int CurrentShownPage { get; private set; }

        /// <summary> Get the current shown information in information window. </summary>
        public Info CurrentShownInfo { get; set; } = Info.Empty;

        
        /// <summary>
        /// Initialize a new instance of File Manager that has configuration read from the config file, and state
        /// restored from the save file.
        /// </summary>
        private FileManager()
        {
            ReadConfig();
            ApplyConfig();
            RestoreLastState();
        }


        /// <summary>
        /// Get instance of the File Manager.
        /// </summary>
        public static FileManager GetInstance()
        {
            if (instance is null)
                instance = new FileManager();

            return instance;
        }


        /// <summary>
        /// Read the config file and save read configurations. <para/>
        /// If any problem occurs while reading the config file, the default configuration will be used.
        /// </summary>
        private void ReadConfig()
        {
            try
            {
                // read config from json file and deserialize it to AppConfig instance
                var configText = File.ReadAllText(Settings.Default.configFilePath);
                config = JsonSerializer.Deserialize<AppConfig>(configText);
            }
            catch (Exception e)
            {
                // if something goes wrong then initialize a default config
                config = new AppConfig();
                ErrorLogger.LogError(e);
            }
        }


        /// <summary>
        /// Check values from config and apply them if they are correct, otherwise - apply default value or
        /// fix value to make it correct.
        /// </summary>
        private void ApplyConfig()
        {
            Console.BackgroundColor = config.ParseColor(config.BackgroundColor, Settings.Default.backgroundColorDefault);
            Console.ForegroundColor = config.ParseColor(config.ForegroundColor, Settings.Default.foregroundColorDefault);
            
            // the window border symbol must be a string with length of 1, so remove all symbols after first if needed
            if (config.WindowBorderSymbol.Length > 1) 
                config.WindowBorderSymbol = config.WindowBorderSymbol.Remove(1);
            
            
            // amount of file to show per page can not be non-positive, so apply default value if needed
            if (config.FilesPerPage <= 0)
                config.FilesPerPage = Settings.Default.filesPerPageDefault;


            Console.Title = Settings.Default.appName;
        }


        /// <summary>
        /// Draw the window border to the full width of the buffer area using the window border symbol from the configuration
        /// </summary>
        private void DrawWindowBorder()
        {
            Console.WriteLine();
            
            for (var i = 0; i < Console.BufferWidth; i++)
                Console.Write(config.WindowBorderSymbol);
            
            Console.WriteLine();
        }


        /// <summary>
        /// Get the root directory on the first available fixed of removable drive. <para/>
        /// If there is no any, then get the root directory of the first available drive. <para/>
        /// If there is problems with all checked drives, then error message will be created and no directory will
        /// be shown.
        /// </summary>
        /// <returns>Root path of the rive.</returns>
        private bool TryRestoreFromRootOnAnyDrive(out string restoredDirectory, out string restoredPage)
        {
            restoredDirectory = null;
            restoredPage = Settings.Default.showPageDefault.ToString();
            
            try
            {
                DriveInfo[] drives = DriveInfo.GetDrives();

                
                // try to get root directory on any fixed drive
                foreach (var drive in drives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                        restoredDirectory = drive.RootDirectory.FullName;
                }
                
                
                // then, if there is not available fixed drives, try get root directory on any removable drive
                if (restoredDirectory is null)
                {
                    foreach (var drive in drives)
                    {
                        if (drive.IsReady && drive.DriveType == DriveType.Removable)
                            restoredDirectory = drive.RootDirectory.FullName;
                    }
                }

                
                // if there is no available fixed or removable drive to start from, then return root directory for the first available drive.
                restoredDirectory ??= drives[0].RootDirectory.FullName;
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);
            }


            // if restored directory is null then there is a problem with drives
            if (restoredDirectory is null)
            {
                CurrentDirectory = null;
                CurrentShownPage = 0;
                CurrentShownInfo = new Info(
                    "Не удалось загрузить данные о папках и файлах: не оказалось ни одного доступного диска.",
                    InfoType.Error
                );
                
                return false;
            }
            
            
            // if drives are ok, then path will be the root directory of some drive
            CurrentShownInfo = new Info(
                "Невозможно восстановить предыдущую сессию. Показана корневая папка диска.",
                InfoType.Warning
            );
            
            return true;
        }
        

        /// <summary>
        /// Check the directory at specified path to have no problems with accessing it. <para/>
        /// Returns error message if there are some problems with accessing the directory. <para/>
        /// Returns null if there are no problems with accessing the directory.
        /// </summary>
        /// <param name="dirPath">The path to the directory to check.</param>
        /// <param name="filesAndDirsAmount">The total amount of files and directories in this directory. </param>
        /// <returns>Error message if any error occurs, or null if no error occured.</returns>
        public string CheckDirectory(string dirPath, out int filesAndDirsAmount)
        {
            try
            {
                var dirsAmount = Directory.GetDirectories(dirPath).Length;
                var filesAmount = Directory.GetFiles(dirPath).Length;

                filesAndDirsAmount = dirsAmount + filesAmount;
                return null;
            }
            catch (Exception e)
            {
                filesAndDirsAmount = 0;
                return e.Message;
            }
        }


        /// <summary>
        /// Restore current directory and shown page since the last app work. <para/>
        /// Current directory and shown page are read from the save file. If save file does not contain saved directory
        /// then directory will be taken as root of any available drive.
        /// </summary>
        public void RestoreLastState()
        {
            string restoredDirectory;
            string restoredPage;
            
            try
            {
                using var fs = new FileStream(Settings.Default.lastStateSaveFilePath, FileMode.Open, FileAccess.Read);
                using var sr = new StreamReader(fs);

                restoredDirectory = sr.ReadLine();
                restoredPage = sr.ReadLine();


                // if restored directory is null or empty then saved data was manually deleted by the user
                // or it's the first application start and nothing was saved yet.
                if (string.IsNullOrEmpty(restoredDirectory))
                {    
                    // apply root directory of the drive as current and set the first page as page to show
                    var restoredFromDrive= TryRestoreFromRootOnAnyDrive(out restoredDirectory, out restoredPage);
                    
                    if (!restoredFromDrive)
                        return;
                } 
                else if (string.IsNullOrEmpty(restoredPage))
                {
                    // set the first page as page to show
                    restoredPage = Settings.Default.showPageDefault.ToString();
                }
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);
                
                // apply root directory of the drive as current and set the first page as page to show
                var restoredFromDrive= TryRestoreFromRootOnAnyDrive(out restoredDirectory, out restoredPage);
                
                if (!restoredFromDrive)
                    return;
            }
            
            
            string error = CheckDirectory(restoredDirectory, out _);

            
            // if any exception was thrown while trying to access the directory then error message will not be null;
            // it means that there is a problem with restored path, so try to get a root path on any drive
            if (error != null)
            {
                // apply root directory of the drive as current and set the first page as page to show
                var restoredFromDrive= TryRestoreFromRootOnAnyDrive(out restoredDirectory, out restoredPage);
                
                if (!restoredFromDrive)
                    return;
            }

            
            // if restored path exists and correct then take it as current directory
            CurrentDirectory = restoredDirectory;

            // check if restored page number is an integer number
            // if not - then take the default value
            CurrentShownPage = int.TryParse(restoredPage, out var page) ? page : Settings.Default.showPageDefault;
            
            // if last state restored successfully then there is no info to show
            CurrentShownInfo = Info.Empty;
        }
        
        
        /// <summary>
        /// Save current application state - write to the save file the current working directory and
        /// the current shown page.
        /// </summary>
        public void SaveState() 
        {
            try
            {
                using var fs = new FileStream(Settings.Default.lastStateSaveFilePath, FileMode.Create, FileAccess.Write);
                using var sw = new StreamWriter(fs);

                sw.WriteLine(CurrentDirectory);
                sw.WriteLine(CurrentShownPage);
            }
            catch (Exception e)
            {
                ErrorLogger.LogError(e);
            }
        }

        
        /// <summary>
        /// Show specified range of files from specified collection.
        /// </summary>
        private void ShowFiles(List<string> dirsAndFilesList, int firstShownFileNum, int lastShownFileNum)
        {
            DrawWindowBorder();
            
            Console.WriteLine($"> {CurrentDirectory ?? "???"}");

            for (var i = firstShownFileNum; i <= lastShownFileNum; i++)
            {
                var shownFileName = (i < dirsAndFilesList.Count) ? dirsAndFilesList[i] : "";
                Console.Out.WriteLine($"\t{shownFileName}");
            }
                
            DrawWindowBorder();
        }
        

        /// <summary>
        /// Show files from the current directory on the current page.
        /// </summary>
        public void ShowFilesWindow()
        {
            // current directory can be null only if there was a serious problem with all drives caused by OS (could not get
            // root directory on any drive), so the only left option is to show error message and empty window.
            if (CurrentDirectory is null)
            {
                ShowFiles(new List<string>(), 1, config.FilesPerPage);
                return;
            }
            
            
            var dirsAndFilesList = new List<string>();
            
            try
            {
                /* Get all files and directories from the current directory*/
                var dirsAndFiles = Directory.GetFileSystemEntries(CurrentDirectory);

                foreach (var df in dirsAndFiles)
                {
                    var nameWithoutPath = Path.GetFileName(df);
                    
                    // if extension is empty then it is directory [D], otherwise it is file [F]
                    var type = Path.GetExtension(df) == string.Empty ? "[D]" : "[F]"; 
                    
                    // in the console it should be shown as: '[D] SomeDirectory' or: '[F] SomeFile.ext'
                    dirsAndFilesList.Add($"{type} {nameWithoutPath}");
                }
                
            }
            catch (Exception e)
            {
                // the execution can get here only if there is a serious problem with directory that is caused by OS,
                // so application can not handle it, and it's better to show an error message to user to let them know it,
                // then just show the drive's root.
                
                ErrorLogger.LogError(e);
                
                // show message saying that there is a problem with current directory that can not be handled by the application
                CurrentShownInfo = new Info(
                    "Произошла проблема с текущей директорией. Попробуйте перейти в другую директорию.",
                    InfoType.Error
                );
                
                // show empty window
                ShowFiles(new List<string>(), 1, config.FilesPerPage);
                
                return;
            }
            
            
            /* Check if page number is correct */
            // from there both directories and files will be called files (in variables names)
               
            var totalFilesAmount = dirsAndFilesList.Count;
                
            // number of the first shown file on the current page
            var firstShownFileNum = (CurrentShownPage - 1) * config.FilesPerPage; 
                
            // number of the last shown file on the current page
            var lastShownFileNum = CurrentShownPage * config.FilesPerPage - 1;
                
            if (totalFilesAmount - 1 < firstShownFileNum)
            {
                if (totalFilesAmount == 0)
                {
                    CurrentShownInfo = new Info(
                        "Директория пуста.",
                        InfoType.Warning
                    );
                }
                else
                {
                    CurrentShownInfo = new Info(
                        "На странице с указанными номером нет файлов. Показана первая страница.",
                        InfoType.Warning
                    );

                    CurrentShownPage = 1;
                }
            }
                
            /*Draw window and show files */
            ShowFiles(dirsAndFilesList, firstShownFileNum, lastShownFileNum);
        }


        /// <summary>
        /// Show window with information about some file or with message/warning/error to let user know that
        /// something went wrong.
        /// </summary>
        public void ShowInfoWindow()
        {
            DrawWindowBorder();
            
            Console.Out.Write(CurrentShownInfo.Data);
            
            DrawWindowBorder();
        }


        public void Loop()
        {
            while (true)
            {
                Console.Clear();
                
                ShowFilesWindow();
                ShowInfoWindow();

                var enteredCommand = Console.In.ReadLine();

                if (isExiting)
                    break;
            }
        }
    }
}