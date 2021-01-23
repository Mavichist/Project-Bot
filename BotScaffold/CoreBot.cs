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
        [CommandAttribute("shutdown", CommandLevel = CommandLevel.Admin)]
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
        [CommandAttribute("register admin role", CommandLevel = CommandLevel.Owner, ParameterRegex = "<@&(?<roleID>\\d+)>")]
        protected async Task RegisterAdminRole(CommandArgs<BotConfig> args)
        {
            ulong roleID = ulong.Parse(args["roleID"]);

            DiscordRole role = args.Guild.GetRole(roleID);

            if (role != null)
            {
                HashSet<ulong> adminRoleIDs = Instance.Details.GetAdminRoleIDs(args.Guild.Id);

                if (!adminRoleIDs.Contains(roleID))
                {
                    adminRoleIDs.Add(roleID);
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role now has administrative privileges.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role already has administrative privileges.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("That role doesn't seem to exist.");
            }
        }
        /// <summary>
        /// Deregisters a role as an administrator role so it can no longer use admin commands.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("deregister admin role", CommandLevel = CommandLevel.Owner, ParameterRegex = "<@&(?<roleID>\\d+)")]
        protected async Task DeregisterAdminRole(CommandArgs<BotConfig> args)
        {
            ulong roleID = ulong.Parse(args["roleID"]);

            DiscordRole role = args.Guild.GetRole(roleID);
            
            HashSet<ulong> adminRoleIDs = Instance.Details.GetAdminRoleIDs(args.Guild.Id);

            if (role != null)
            {
                if (adminRoleIDs.Remove(roleID))
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role now has no administrative privileges.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role did not have administrative privileges.");
                }
            }
            else
            {
                if (adminRoleIDs.Remove(roleID))
                {
                    await args.Channel.SendMessageAsync($"The role doesn't seem to exist but is still registered as admin. I'll deregister it.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The role doesn't seem to exist and it's not registered as an admin. Havin' a giggle?");
                }
            }
        }
    }
}