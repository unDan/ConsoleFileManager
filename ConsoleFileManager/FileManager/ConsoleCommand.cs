using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace ConsoleFileManager.FileManager
{
/// <summary>
    /// Represents a console command with its string representation, method that must be executed and information about
    /// arguments for this command.
    /// </summary>
    public class ConsoleCommand
    {
        /// <summary> The method that will be executed when this command is entered in console. </summary>
        private readonly Action<string[]> methodToExecute;

        /// <summary> The signature of this command. </summary>
        private readonly Regex signature;
        
        /// <summary> Get the string representation of this command. </summary>
        public string Name { get; }
        
        /// <summary> Get the amount of arguments that are not necessary for this command.</summary>
        public string[] ExtraArgs { get; }

        /// <summary> Get the value indicating whether this command has unnecessary arguments.</summary>
        public bool HasExtraArgs => ExtraArgs.Length > 0;

        
        
        /// <summary> Get the value indicating whether specified command string matches command's signature.</summary>
        public bool IsMatchSignature(string commandStr) => signature.IsMatch(commandStr);

        /// <summary>
        /// Execute this command by invoking its method with specified arguments.
        /// </summary>
        /// <param name="args">Command arguments.</param>
        /// <returns>Null - if command executed successfully. Fail message otherwise.</returns>
        public void Execute(params string[] args) => methodToExecute(args);


        
        /// <summary>
        /// Initialize a new instance of console command with specified string representation, specified method to execute,
        /// specified amount of required arguments and with specified signatures of extra arguments if needed.
        /// </summary>
        /// <param name="name">The string representation of the command.</param>
        /// <param name="methodToExecute">The method that must be executed when the command is entered in the console.</param>
        /// <param name="signature">The signature of this command.</param>
        /// <param name="extraArgs">The signatures of extra arguments for the command.</param>
        public ConsoleCommand(string name, Action<string[]> methodToExecute, Regex signature, params string[] extraArgs)
        {
            Name = name;
            
            this.methodToExecute = methodToExecute;
            this.signature = signature;

            ExtraArgs = extraArgs;
        }


        /// <summary>
        /// Get all arguments from the command string and parse them according to their signatures.
        /// </summary>
        /// <param name="commandStr">The command to get arguments from.</param>
        /// <returns>Array of arguments got and parsed from the command.</returns>
        public string[] GetCommandArgs(string commandStr)
        {
            // EXAMPLE:
            // if command's signature is: "commandName "STRING" "STRING" -eArg1 INTEGER -eArg2 BOOL -eArg3 BOOL -eArg4 INTEGER"
            // and the command is executed as: "commandName "path1" "path2" -eArg1 10 -eArg3 false"
            // then resulting arguments collection will be: ["path1", "path2", "10", null, "false", null]
            
            // get all required args w/o " symbol from the command string
            var requiredArgs = new List<string>();
            foreach (Match reqArg in Regex.Matches(commandStr, @"""[^/*?""<>|]*"""))
                requiredArgs.Add(reqArg.Value.Trim('\"'));
            
            
            // get all extra args values from the command string
            var extraArgs = new List<string>();
            foreach (var eArgSignature in ExtraArgs)
            {
                Match curExtraArgMatch = new Regex($@"-{eArgSignature} \S+").Match(commandStr);
                
                var curExtraArg = (curExtraArgMatch == Match.Empty) ? null : curExtraArgMatch.Value;
                curExtraArg = curExtraArg?.Split()[1];

                extraArgs.Add(curExtraArg);
            }
            
            var args = new List<string>();
            args.AddRange(requiredArgs);
            args.AddRange(extraArgs);
            
            return args.ToArray();
        }
    }
}