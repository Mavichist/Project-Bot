using BotScaffold;

namespace MarkovBot
{
    /// <summary>
    /// Represents markov bot configuration information for a specific server.
    /// </summary>
    public class MarkovBotConfig : BotConfig
    {
        /// <summary>
        /// Creates a new markov bot config object using the specified indicator.
        /// </summary>
        /// <param name="indicator">The character all commands for this bot must begin with.</param>
        public MarkovBotConfig(char indicator) : base(indicator)
        {
            
        }
    }
}