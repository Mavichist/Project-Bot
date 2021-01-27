using System.Text.Json;
using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus.Entities;

namespace RPGBot
{
    public partial class RPGManagerBot
    {
        private DiscordEmbed GetArmorEmbed(string name, ArmorProfile profile)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle("Armor Profile:");
            builder.WithDescription($"{name} - {profile.Description}");

            builder.AddField("🛡 Armor Magnitude", $"{profile.Magnitude}");
            builder.AddField("📈 Armor Spread", $"{profile.Spread}");
            builder.AddField("🧱 Protection", $"{profile.Protection}");
            builder.AddField("🤸 Dodge", $"{profile.Dodge}");
            builder.AddField("🛑 Resists", $"{profile.Resists}");
            builder.AddField("💔 Vulnerability", $"{profile.Vulnerability}");

            return builder.Build();
        }

        /// <summary>
        /// Attempts to forge an armor piece given the supplied arguments.
        /// The supplied arguments are given as a simple Json string, which should match the damage
        /// profile data structure.
        /// Normally I wouldn't use serialized data as input, but as far as I'm aware this particular
        /// implementation is safe.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("forge armor", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>.+)\"\\s+```(?<json>.*)```")]
        protected async Task ForgeArmor(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];
            string json = args["json"];

            try
            {
                ArmorProfile profile = JsonSerializer.Deserialize<ArmorProfile>(json);

                args.Config.Armors[name] = profile;

                DiscordEmbed embed = GetArmorEmbed(name, profile);

                await args.Channel.SendMessageAsync("A new piece of armor has been forged!", false, embed);
            }
            catch (JsonException e)
            {
                await args.Channel.SendMessageAsync("The Json entered wasn't valid.");
            }
        }
        /// <summary>
        /// A command for showing a specific piece of armor.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show armor", CommandLevel = CommandLevel.Unrestricted, ParameterRegex = "\"(?<name>.+)\"")]
        protected async Task ShowArmor(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];

            if (args.Config.Armors.TryGetValue(name, out ArmorProfile profile))
            {
                DiscordEmbed embed = GetArmorEmbed(name, profile);
                await args.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                await args.Channel.SendMessageAsync("That weapon doesn't exist.");
            }
        }
        /// <summary>
        /// A command for listing all armor available on a server.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show all armor", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowAllArmor(CommandArgs<RPGBotConfig> args)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle("All armor on this server:");
            foreach (var armor in args.Config.Armors)
            {
                builder.AddField(armor.Key, armor.Value.Description);
            }
            await args.Channel.SendMessageAsync(null, false, builder.Build());
        }
        /// <summary>
        /// A command for removing a weapon from a server.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("remove armor", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>.+)\"")]
        protected async Task RemoveArmor(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];

            if (args.Config.Armors.Remove(name))
            {
                await args.Channel.SendMessageAsync($"I have removed **{name}** from the server.");
            }
            else
            {
                await args.Channel.SendMessageAsync("That weapon does not exist.");
            }
        }
        /// <summary>
        /// A command for giving a series of mentioned users weapons.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("gift armor", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>.+)\"")]
        protected async Task GiftArmor(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];
            
            if (args.Config.Armors.TryGetValue(name, out ArmorProfile profile))
            {
                foreach (DiscordUser user in args.MentionedUsers)
                {
                    DiscordMember member = await args.Guild.GetMemberAsync(user.Id);
                    Player player = args.Config.GetPlayer(user.Id);
                    
                    player.Armor.CopyFrom(profile);

                    await args.Channel.SendMessageAsync($"**{member.DisplayName}** is now armored with **{name}**, *{profile.Description}*!");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("An armor piece with that name does not exist on this server.");
            }
        }
    }
}