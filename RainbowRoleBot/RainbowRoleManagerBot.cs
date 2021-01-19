using BotScaffold;
using DSharpPlus.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RainbowRoleBot
{
    /// <summary>
    /// A simple bot that periodically recolors a list of roles, forming a kind of rainbow effect.
    /// </summary>
    public class RainbowRoleManagerBot : BotInstance.Bot<RainbowRoleBotConfig>
    {
        public static int ONE_SECOND = 1000;
        public static int ONE_MINUTE = ONE_MINUTE * 60;
        public static int ONE_HOUR = ONE_MINUTE * 60;
        public static int ONE_DAY = ONE_HOUR * 24;

        private Timer colorChangeTimer;

        /// <summary>
        /// Creates a new rainbow role manager bot with the specified name.
        /// </summary>
        /// <param name="name">The name of the bot (used for config files).</param>
        public RainbowRoleManagerBot(string name) : base(name)
        {

        }

        /// <summary>
        /// An asynchronous method for changing the color of this bot's managed roles.
        /// </summary>
        /// <returns>A task representing the work to be done.</returns>
        private async Task ChangeColorsAsync()
        {
            Random r = new Random();
            foreach (ulong guildID in EnumerateGuildIDs())
            {
                DiscordGuild guild = await Instance.Client.GetGuildAsync(guildID);
                RainbowRoleBotConfig config = GetConfig(guildID);
                foreach (ulong roleID in config.RainbowRoles)
                {
                    DiscordRole role = guild.GetRole(roleID);
                    float red = 0.5f + (float)r.NextDouble() * 0.5f;
                    float green = 0.5f + (float)r.NextDouble() * 0.5f;
                    float blue = 0.5f + (float)r.NextDouble() * 0.5f;
                    await role.ModifyAsync(null, null, new DiscordColor(red, green, blue));
                }
            }
        }
        /// <summary>
        /// A wrapper method to be fed into the colorChangeTimer object.
        /// </summary>
        /// <param name="state">A state object for the start of each thread. Currently unused.</param>
        private void ChangeColors(object state)
        {
            ChangeColorsAsync().GetAwaiter().GetResult();
        } 
        /// <summary>
        /// A command for adding a role to this bot.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("rainbow role add", CommandLevel = CommandLevel.Admin, ParameterRegex = "<@&(?<roleID>\\d+)>")]
        private async Task AddRainbowRole(CommandArgs<RainbowRoleBotConfig> args)
        {
            ulong roleID = ulong.Parse(args["roleID"]);
            DiscordRole role = args.Guild.GetRole(roleID);

            if (role != null)
            {
                if (!args.Config.RainbowRoles.Contains(roleID))
                {
                    args.Config.RainbowRoles.Add(roleID);
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role is now a rainbow role.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role is already a rainbow role.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("That role doesn't seem to exist.");
            }
        }
        /// <summary>
        /// A command for removing a role from this bot.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("rainbow role remove", CommandLevel = CommandLevel.Admin, ParameterRegex = "<@&(?<roleID>\\d+)>")]
        private async Task RemoveRainbowRole(CommandArgs<RainbowRoleBotConfig> args)
        {
            ulong roleID = ulong.Parse(args["roleID"]);
            DiscordRole role = args.Guild.GetRole(roleID);

            if (role != null)
            {
                if (args.Config.RainbowRoles.Remove(roleID))
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role is no longer a rainbow role.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role isn't a rainbow role.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("That role doesn't seem to exist.");
            }
        }

        /// <summary>
        /// Creates a default config object to suit this bot.
        /// </summary>
        /// <returns>The config object.</returns>
        protected override RainbowRoleBotConfig CreateDefaultConfig()
        {
            return new RainbowRoleBotConfig('!');
        }
        /// <summary>
        /// Fires when the bot first connects.
        /// </summary>
        protected override void OnConnected()
        {
            base.OnConnected();

            colorChangeTimer = new Timer(ChangeColors, null, 0, 10 * ONE_SECOND);
        }
    }
}