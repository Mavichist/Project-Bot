using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace RoleBot
{
    /// <summary>
    /// Represents the configuration of a role-reaction bot for a given server.
    /// </summary>
    public class RoleBotConfig : BotConfig
    {
        /// <summary>
        /// A dictionary describing the association between emojis and roles on this server.
        /// Emoji names form the key of this dictionary (including the colons), while role IDs form the values.
        /// Roles do not need to be managed by this bot in order to be registered with emojis.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, ulong> EmojiRoles
        {
            get;
            private set;
        }
        /// <summary>
        /// A list of roles this bot manages.
        /// Managed roles can be deleted en-masse.
        /// </summary>
        [JsonInclude]
        public List<ulong> ManagedRoles
        {
            get;
            private set;
        } = new List<ulong>();
        /// <summary>
        /// The ID for the post whose reactions will be monitored and roles distributed for.
        /// </summary>
        [JsonInclude]
        public ulong RolePostID
        {
            get;
            set;
        }
        /// <summary>
        /// The ID of the channel where the role post resides.
        /// </summary>
        [JsonInclude]
        public ulong RolePostChannelID
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new role bot config object using an indicator.
        /// </summary>
        /// <param name="indicator">The indicator commands need to start with.</param>
        public RoleBotConfig(char indicator) : base(indicator)
        {
            EmojiRoles = new Dictionary<string, ulong>();
        }
    }
}