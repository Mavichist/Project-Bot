using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus.Entities;

namespace RPGBot
{
    public class Ability
    {
        [JsonInclude]
        public string Description
        {
            get;
            private set;
        }
        
        public Ability(string description, string settings)
        {
            Description = description;
            Parse(settings);
        }

        private void Parse(string settings)
        {
            
        }

        public async Task Invoke(CommandArgs<RPGBotConfig> args, string mantra)
        {

        }
    }
}