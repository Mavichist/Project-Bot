using System.Threading.Tasks;

namespace BotScaffold
{
    /// <summary>
    /// A very simple bot that provides basic functionality for bot instances.
    /// </summary>
    public class CoreBot : BotInstance.Bot<BotConfig>
    {
        /// <summary>
        /// Creates a new instance of a core bot with the specified name.
        /// </summary>
        /// <param name="name">The name of the bot (used for config files).</param>
        public CoreBot(string name) : base(name)
        {

        }

        /// <summary>
        /// Creates a default config data structure for new servers.
        /// </summary>
        /// <returns>a config data structure.</returns>
        protected override BotConfig CreateDefaultConfig()
        {
            return new BotConfig('!');
        }
        /// <summary>
        /// A simple parameterless command for shutting down the bot.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("shutdown", CommandLevel = CommandLevel.Admin)]
        protected async Task Shutdown(CommandArgs<BotConfig> args)
        {
            await args.Channel.SendMessageAsync("Shutting down...");
            Instance.Stop();
        }
    }
}