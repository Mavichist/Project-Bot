using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BotScaffold;

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