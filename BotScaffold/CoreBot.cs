using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

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
        [Usage("Using this command will shut down the bot on a server level. It is for debugging only.")]
        [Command("shutdown", CommandLevel = CommandLevel.Owner)]
        protected async Task Shutdown(CommandArgs<BotConfig> args)
        {
            await args.Channel.SendMessageAsync("Shutting down...");
            Instance.Stop();
        }
        /// <summary>
        /// A command for saving all configuration objects immediately.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [Usage("This command will cause an immediate save of bot config information.")]
        [Command("save all", CommandLevel = CommandLevel.Admin)]
        protected async Task SaveAll(CommandArgs<BotConfig> args)
        {
            Instance.SaveAll();
            await args.Channel.SendMessageAsync("Saved all config.");
        }
    }
}