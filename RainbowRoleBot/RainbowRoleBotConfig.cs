using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace RainbowRoleBot
{
    public class RainbowRoleBotConfig : BotConfig
    {
        [JsonInclude]
        public List<ulong> RainbowRoles
        {
            get;
            private set;
        } = new List<ulong>();

        public RainbowRoleBotConfig(char indicator) : base(indicator)
        {
            
        }
    }
}