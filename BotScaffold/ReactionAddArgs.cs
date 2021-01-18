using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BotScaffold
{
    /// <summary>
    /// Represents the context surrounding the addition of a reaction to a post.
    /// </summary>
    /// <typeparam name="TConfig">The type of config object for the bot.</typeparam>
    public class ReactionAddArgs<TConfig> where TConfig : BotConfig
    {
        /// <summary>
        /// The config object for the current bot and server/guild.
        /// </summary>
        public TConfig Config
        {
            get;
            private set;
        }
        /// <summary>
        /// The original reaction arguments from the event.
        /// </summary>
        private MessageReactionAddEventArgs Args
        {
            get;
            set;
        }
        /// <summary>
        /// The message that was reacted to.
        /// </summary>
        public DiscordMessage Message
        {
            get
            {
                return Args.Message;
            }
        }
        /// <summary>
        /// The channel the reaction occurred in.
        /// </summary>
        public DiscordChannel Channel
        {
            get
            {
                return Args.Channel;
            }
        }
        /// <summary>
        /// The user who added the reaction.
        /// </summary>
        public DiscordUser User
        {
            get
            {
                return Args.User;
            }
        }
        /// <summary>
        /// The guild the reaction occurred in.
        /// </summary>
        public DiscordGuild Guild
        {
            get
            {
                return Args.Guild;
            }
        }
        /// <summary>
        /// The emoji that was added as a reaction.
        /// </summary>
        public DiscordEmoji Emoji
        {
            get
            {
                return Args.Emoji;
            }
        }
        
        /// <summary>
        /// Creates a new reaction arguments object from the relevant config and event args.
        /// </summary>
        /// <param name="args">The original arguments for the reaction event.</param>
        /// <param name="config">The relevant config object for the bot and server/guild.</param>
        public ReactionAddArgs(MessageReactionAddEventArgs args, TConfig config)
        {
            Args = args;
            Config = config;
        }
    }
}