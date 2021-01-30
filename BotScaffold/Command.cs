using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace BotScaffold
{
    public enum CommandLevel { Unrestricted = 0, Admin = 1, Owner = 2 }
    public delegate Task CommandCallback<TConfig>(CommandArgs<TConfig> args) where TConfig : BotConfig;

    /// <summary>
    /// A simple class for handling command strings and their corresponding actions.
    /// </summary>
    public class Command<TConfig> : IComparable<Command<TConfig>> where TConfig : BotConfig
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
        public CommandCallback<TConfig> Callback
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
        /// Specifies what users can run this command.
        /// </summary>
        public CommandLevel CommandLevel
        {
            get;
            private set;
        }
        /// <summary>
        /// Contains usage help information for users.
        /// </summary>
        public string UsageInformation
        {
            get;
            private set;
        }
        /// <summary>
        /// Contains information on each argument the command takes.
        /// </summary>
        public List<string> ArgumentInformation
        {
            get;
            private set;
        } = new List<string>();

        /// <summary>
        /// Attempts to invoke this command given the current user string.
        /// </summary>
        /// <param name="args">The message context used to invoke this command attempt.</param>
        /// <param name="config">The configuration object for the specifc bot and server.</params>
        /// <param name="instance">The bot instance running the current bot.</params>
        /// <returns>A task for handling the command.</returns>
        public async Task<CommandState> AttemptAsync(MessageCreateEventArgs args, TConfig config, BotInstance instance)
        {
            Match commandMatch = Regex.Match(args.Message.Content, $"^{config.Indicator}{CommandString}(?<parameters>[\\w\\W]*)$");
            if (commandMatch.Success)
            {
                if (ParameterRegex is null)
                {
                    await Callback(new CommandArgs<TConfig>(null, args, config, instance));
                    return CommandState.Handled;
                }
                else
                {
                    string parameters = commandMatch.Groups["parameters"].Value;
                    Match parameterMatch = Regex.Match(parameters, ParameterRegex);
                    if (parameterMatch.Success)
                    {
                        await Callback(new CommandArgs<TConfig>(parameterMatch, args, config, instance));
                        return CommandState.Handled;
                    }
                    else
                    {
                        return CommandState.ParameterError;
                    }
                }
            }
            else
            {
                return CommandState.Unhandled;
            }
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
        public int CompareTo(Command<TConfig> other)
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
        public static List<Command<TConfig>> GetCommands(object o)
        {
            List<Command<TConfig>> commands = new List<Command<TConfig>>();

            Type t = o.GetType();

            foreach (MethodInfo m in t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                CommandAttribute attr = m.GetCustomAttribute<CommandAttribute>();
                UsageAttribute usage = m.GetCustomAttribute<UsageAttribute>();
                CommandCallback<TConfig> callback = m.CreateDelegate<CommandCallback<TConfig>>(o);

                Command<TConfig> command = new Command<TConfig>()
                {
                    CommandString = attr.CommandString,
                    ParameterRegex = attr.ParameterRegex,
                    Callback = callback,
                    CommandLevel = attr.CommandLevel,
                    UsageInformation = usage.UsageInfo ?? "No information available."
                };

                foreach (var argument in m.GetCustomAttributes<ArgumentAttribute>())
                {
                    command.ArgumentInformation.Add($"{argument.Name}: {argument.Info}");
                }

                commands.Add(command);

                Console.WriteLine($"Added \"{attr.CommandString}\" to {o}.");
            }

            commands.Sort();

            return commands;
        }
    }
}