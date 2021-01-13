using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace BotScaffold
{
    public delegate Task CommandCallback(Match match, MessageCreateEventArgs args);

    /// <summary>
    /// A simple class for handling command strings and their corresponding actions.
    /// </summary>
    public class Command : IComparable<Command>
    {
        /// <summary>
        /// The entire command portion of the user input.
        /// The command does not include parameters, only the keywords used to identify it.
        /// </summary>
        public string CommandString
        {
            get;
            private set;
        }
        /// <summary>
        /// A delegate to the method that will handle this command.
        /// </summary>
        public CommandCallback Callback
        {
            get;
            private set;
        }
        /// <summary>
        /// The Regex string that will be used to extract parameters from the supplied user string.
        /// </summary>
        public string ParameterRegex
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new command object by mapping a command string to a callback handler.
        /// </summary>
        /// <param name="commandString">The string that identifies the command.</param>
        /// <param name="parameterRegex">A regular expression for extracting parameter values.</param>
        /// <param name="callback">The callback that handles the command execution.</param>
        public Command(string commandString, string parameterRegex, CommandCallback callback)
        {
            CommandString = commandString;
            ParameterRegex = parameterRegex;
            Callback = callback;
        }

        /// <summary>
        /// Attempts to invoke this command given the current user string.
        /// </summary>
        /// <param name="args">The message context used to invoke this command attempt.</param>
        /// <returns>A boolean indicating whether the attempt was successful.</returns>
        public async Task<bool> AttemptAsync(MessageCreateEventArgs args)
        {
            Match m = Regex.Match(args.Message.Content, $"(?:{CommandString})\\s*{ParameterRegex}");
            if (m.Success)
            {
                await Callback(m, args);
                return true;
            }
            else return false;
        }
        /// <summary>
        /// Generates a hash code that can be used to quickly identify whether two commands are not
        /// equal. Uses the default string hashing on the command string.
        /// </summary>
        /// <returns>An integer denoting the hash code.</returns>
        public override int GetHashCode()
        {
            return CommandString.GetHashCode();
        }
        /// <summary>
        /// Compares this command to another command. Commands with the smallest command strings are
        /// considered "greater than" others, for sorting purposes.
        /// When the command strings are equal in length, whether or not the command has parameters
        /// will then be used to determine precedence.
        /// </summary>
        /// <param name="other">The other command to compare with.</param>
        /// <returns>An integer denoting the comparison.</returns>
        public int CompareTo(Command other)
        {
            int comp = CommandString.Length - other.CommandString.Length;
            if (comp == 0)
            {
                comp -= ParameterRegex is null ? 0 : 1;
                comp += other.ParameterRegex is null ? 0 : 1;
            }
            return comp;
        }
    
        /// <summary>
        /// Generates a list of command objects based on the command attributes for a given object.
        /// </summary>
        /// <param name="o">The object whose public instance methods contains command attributes to
        /// generate commands from.</param>
        /// <returns>A list of commands, sorted in order of least significant command string to most
        /// significant command string.</returns>
        public static List<Command> GetCommands(object o)
        {
            List<Command> commands = new List<Command>();

            Type t = o.GetType();

            foreach (MethodInfo m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                CommandAttribute commandAttribute = m.GetCustomAttribute<CommandAttribute>();
                if (commandAttribute != null)
                {
                    CommandCallback callback = m.CreateDelegate<CommandCallback>(o);
                    commands.Add(new Command(commandAttribute.CommandString, commandAttribute.ParameterRegex, callback));
                }
            }

            commands.Sort();

            return commands;
        }
    }
}