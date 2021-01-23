using System.Collections.Generic;
using System.Text.RegularExpressions;
using BotScaffold;
using DSharpPlus.Entities;

namespace AwardBot
{
    public class AbilityArgs
    {
        public AwardBotConfig Config
        {
            get
            {
                return Args.Config;
            }
        }
        private Match Match
        {
            get;
            set;
        }
        public string this[string argumentName]
        {
            get
            {
                return Match.Groups[argumentName].Value;
            }
        }
        private CommandArgs<AwardBotConfig> Args
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

        public AbilityArgs(Match match, CommandArgs<AwardBotConfig> args)
        {
            Match = match;
            Args = args;
        }
    }
}