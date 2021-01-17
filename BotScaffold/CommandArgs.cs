using System.Collections.Generic;
using System.Text.RegularExpressions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BotScaffold
{
    /// <summary>
    /// Encapsulates DSharpPlus objects, config and command parameters.
    /// </summary>
    /// <typeparam name="TConfig">The type of config object.</typeparam>
    public class CommandArgs<TConfig> where TConfig : BotConfig
    {
        /// <summary>
        /// The config object for the current bot and guild.
        /// </summary>
        public TConfig Config
        {
            get;
            private set;
        }
        /// <summary>
        /// The successful regex match for this command.
        /// </summary>
        private Match Match
        {
            get;
            set;
        }
        /// <summary>
        /// Retrieves a string match for a command parameter.
        /// <param name="argName">The name of the command parameter as it was in the regular expression.</param>
        /// </summary>
        public string this[string argName]
        {
            get
            {
                return Match.Groups[argName].Value;
            }
        }
        /// <summary>
        /// The original message creation event arguments for the command.
        /// </summary>
        private MessageCreateEventArgs Args
        {
            get;
            set;
        }
        /// <summary>
        /// The command message.
        /// </summary>
        public DiscordMessage Message
        {
            get
            {
                return Args.Message;
            }
        }
        /// <summary>
        /// The channel the command occurred in.
        /// </summary>
        public DiscordChannel Channel
        {
            get
            {
                return Args.Channel;
            }
        }
        /// <summary>
        /// The guild the command occurred in.
        /// </summary>
        public DiscordGuild Guild
        {
            get
            {
                return Args.Guild;
            }
        }
        /// <summary>
        /// The author of the command.
        /// </summary>
        public DiscordUser Author
        {
            get
            {
                return Args.Author;
            }
        }
        /// <summary>
        /// A list of users mentioned by this command.
        /// </summary>
        public IReadOnlyList<DiscordUser> MentionedUsers
        {
            get
            {
                return Args.MentionedUsers;
            }
        }
        /// <summary>
        /// A list of roles mentioned by this command.
        /// </summary>
        public IReadOnlyList<DiscordRole> MentionedRoles
        {
            get
            {
                return Args.MentionedRoles;
            }
        }
        /// <summary>
        /// A list of channels mentioned by this command.
        /// </summary>
        public IReadOnlyList<DiscordChannel> MentionedChannels
        {
            get
            {
                return Args.MentionedChannels;
            }
        }

        /// <summary>
        /// Creates a new command args object from the regex match, the message arguments and the
        /// config for the current bot and server/guild.
        /// </summary>
        /// <param name="match">The original regex match for the command.</param>
        /// <param name="args">The arguments from the message creation event.</param>
        /// <param name="config">The config object for the server/guild.</param>
        public CommandArgs(Match match, MessageCreateEventArgs args, TConfig config)
        {
            Match = match;
            Args = args;
            Config = config;
        }
    }
}