using System;

namespace BotScaffold
{
    /// <summary>
    /// Defines a method that can be called by a discord command.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class CommandAttribute : System.Attribute
    {
        private readonly string commandString;
        
        /// <summary>
        /// The primary command string.
        /// </summary>
        public string CommandString
        {
            get
            {
                return commandString;
            }
        }
        /// <summary>
        /// The regex pattern for extracting command parameters.
        /// If left null, no regex matching will be performed and the command will be treated as
        /// parameterless.
        /// </summary>
        public string ParameterRegex
        {
            get;
            set;
        }
        /// <summary>
        /// Dictates who can use the command.
        /// </summary>
        public CommandLevel CommandLevel
        {
            get;
            set;
        } = CommandLevel.Owner;
        
        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="command">The primary command string.</param>
        public CommandAttribute(string commandString)
        {
            this.commandString = commandString;
        }
    }
}