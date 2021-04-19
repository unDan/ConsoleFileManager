using System;
using System.Collections.Generic;
using System.IO;
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
            
            // check if parsed directory exists and is available
            var errorMsg = CheckDirectory(path, out _);

            // if error message is not null then an exception was thrown while trying to access the directory.
            if (errorMsg != null)
            {
                CurrentShownInfo = new Info(
                    $"Ошибка исполнения команды: {errorMsg}",
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
            
        }

        private void Exit(params string[] args)
        {
            isExiting = true;
        }
        
    }
}