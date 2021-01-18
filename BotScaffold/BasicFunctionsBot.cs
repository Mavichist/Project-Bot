using System.Threading.Tasks;

namespace BotScaffold
{
    public class BasicFunctionsBot : BotInstance.Bot<BotConfig>
    {
        public BasicFunctionsBot(string name) : base(name)
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
        /// <param name="match">The regex match for command parameters.</param>
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