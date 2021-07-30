using System.Text.Json.Serialization;
using System.Threading;
using BotScaffold;

namespace PurgeBot
{
    public class PurgeBotConfig : BotConfig
    {
        [JsonIgnore]
        public CancellationTokenSource TokenSource
        {
            get;
            set;
        } = new CancellationTokenSource();

        public PurgeBotConfig(char indicator) : base(indicator)
        {
            
        }
    }
}