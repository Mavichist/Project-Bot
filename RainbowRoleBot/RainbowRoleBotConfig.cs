using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace RainbowRoleBot
{
    /// <summary>
    /// Represents a configuration for a rainbow role management bot.
    /// </summary>
    public class RainbowRoleBotConfig : BotConfig
    {
        /// <summary>
        /// A list of role IDs pertaining to roles whose color should be altered by a rainbow role
        /// management bot.
        /// </summary>
        [JsonInclude]
        public List<ulong> RainbowRoles
        {
            get;
            private set;
        } = new List<ulong>();

        /// <summary>
        /// Constructs a new config object with the specified indicator.
        /// </summary>
        /// <param name="indicator">The character that each command must start with.</param>
        public RainbowRoleBotConfig(char indicator) : base(indicator)
        {
            
        }
    }
}