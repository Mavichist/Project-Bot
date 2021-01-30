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
        [CommandAttribute("shutdown", CommandLevel = CommandLevel.Owner)]
        protected async Task Shutdown(CommandArgs<BotConfig> args)
        {
            await args.Channel.SendMessageAsync("Shutting down...");
            Instance.Stop();
        }
        /// <summary>
        /// Registers a role as an administrator role so it can use administrator commands.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [Usage("Registers roles as administrative. Users with administrative roles can use admin-restricted commands.")]
        [Argument("Roles", "All mentioned roles accompanying this command will be registered as admin.")]
        [Command("register admin role", CommandLevel = CommandLevel.Owner)]
        protected async Task RegisterAdminRole(CommandArgs<BotConfig> args)
        {
            foreach (DiscordRole role in args.MentionedRoles)
            {
                HashSet<ulong> adminRoleIDs = Instance.Details.GetAdminRoleIDs(args.Guild.Id);

                if (!adminRoleIDs.Contains(role.Id))
                {
                    adminRoleIDs.Add(role.Id);
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role now has administrative privileges.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role already has administrative privileges.");
                }
            }
        }
        /// <summary>
        /// Deregisters a role as an administrator role so it can no longer use admin commands.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [Usage("Deregisters roles as administrative. Users without administrative roles cannot use admin-restricted commands.")]
        [Argument("Roles", "All mentioned roles accompanying this command will be deregistered.")]
        [Command("deregister admin role", CommandLevel = CommandLevel.Owner)]
        protected async Task DeregisterAdminRole(CommandArgs<BotConfig> args)
        {
            foreach (DiscordRole role in args.MentionedRoles)
            {
                HashSet<ulong> adminRoleIDs = Instance.Details.GetAdminRoleIDs(args.Guild.Id);

                if (adminRoleIDs.Remove(role.Id))
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role now has no administrative privileges.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role isn't administrative.");
                }
            }
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