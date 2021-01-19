using BotScaffold;
using DSharpPlus.Entities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RainbowRoleBot
{
    public class RainbowRoleManagerBot : BotInstance.Bot<RainbowRoleBotConfig>
    {
        public static int ONE_SECOND = 1000;
        public static int ONE_MINUTE = ONE_MINUTE * 60;
        public static int ONE_HOUR = ONE_MINUTE * 60;
        public static int ONE_DAY = ONE_HOUR * 24;

        private Timer colorChangeTimer;

        public RainbowRoleManagerBot(string name) : base(name)
        {

        }

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
        private void ChangeColors(object state)
        {
            ChangeColorsAsync().GetAwaiter().GetResult();
        } 
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

        protected override RainbowRoleBotConfig CreateDefaultConfig()
        {
            return new RainbowRoleBotConfig('!');
        }
        protected override void OnConnected()
        {
            base.OnConnected();

            colorChangeTimer = new Timer(ChangeColors, null, 0, 10 * ONE_SECOND);
        }
    }
}