using BotScaffold;

namespace MarkovBot
{
    /// <summary>
    /// Represents a markov chain tool for imitating text patterns.
    /// </summary>
    public class MarkovChainBot : BotInstance.Bot<MarkovBotConfig>
    {
        /// <summary>
        /// Creates a new instance of a markov chain bot using the specified name.
        /// </summary>
        /// <param name="name">The name of the bot (used to load config).</param>
        public MarkovChainBot(string name) : base(name)
        {

        }

        /// <summary>
        /// Creates a default config object to suit this bot.
        /// </summary>
        /// <returns>A configuration object.</returns>
        protected override MarkovBotConfig CreateDefaultConfig()
        {
            return new MarkovBotConfig('!');
        }
    }
}