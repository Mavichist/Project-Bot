using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace RoleBot
{
    public class RoleBotConfig : BotConfig
    {
        [JsonInclude]
        public Dictionary<string, ulong> EmojiRoles
        {
            get;
            private set;
        }
        [JsonInclude]
        public ulong RolePostID
        {
            get;
            set;
        }

        public RoleBotConfig(char indicator) : base(indicator)
        {
            EmojiRoles = new Dictionary<string, ulong>();
        }
    }
}