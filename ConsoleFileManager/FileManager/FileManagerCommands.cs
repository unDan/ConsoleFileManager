using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

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
                    "Неверная команда.",
                    InfoType.Warning
                );
                
                return;
            }

            var args = command.GetCommandArgs(commandStr);
            command.Execute(args);

        }
        

        private void GoToDirectory(params string[] args)
        {
            
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
            
        }
        
    }
}