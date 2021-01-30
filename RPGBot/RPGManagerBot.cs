using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus.Entities;

namespace RPGBot
{
    /// <summary>
    /// Represents a bot for handling point-based awards, based on user reactions.
    /// </summary>
    public partial class RPGManagerBot : BotInstance.Bot<RPGBotConfig>
    {
        private const int BAR_RESOLUTION = 16;

        private Dictionary<ulong, DateTime> lastWarnings = new Dictionary<ulong, DateTime>();

        /// <summary>
        /// Creates a new instance of an award manager bot, with the specified name.
        /// </summary>
        /// <param name="name">The name of this bot (used for loading config).</param>
        public RPGManagerBot(string name) : base(name)
        {

        }
        
        /// <summary>
        /// A command for showing the user's purse.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show stash", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowStash(CommandArgs<RPGBotConfig> args)
        {
            if (args.Config.HelpChannelID == args.Channel.Id)
            {
                DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);
                Player player = args.Config.GetPlayer(member.Id);

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                builder.WithTitle($"**{member.DisplayName}'s Stash:**");
                builder.WithThumbnail(member.AvatarUrl);
                builder.WithColor(member.Color);
                
                foreach (var currency in player.Currency)
                {
                    DiscordEmoji emoji = DiscordEmoji.FromName(args.Instance.Client, currency.Key);
                    builder.AddField(emoji.FormatName(), $"**{currency.Value}**", true);
                }

                await args.Channel.SendMessageAsync(null, false, builder.Build());
            }
        }
        /// <summary>
        /// A command for displaying the inventory of a specific user.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show inventory", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowInventory(CommandArgs<RPGBotConfig> args)
        {
            DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);
            Player player = args.Config.GetPlayer(member.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"{member.DisplayName}'s Inventory:");
            builder.WithThumbnail(member.AvatarUrl);
            builder.WithColor(member.Color);

            for (int i = 0; i < player.Inventory.Length; i++)
            {
                Item? item = player.Inventory[i];
                if (item.HasValue)
                {
                    builder.AddField($"Item slot {i}:", $"*{item.Value.Name}*", true);
                }
                else
                {
                    builder.AddField($"Item slot {i}:", "*Empty.*", true);
                }
            }

            await args.Channel.SendMessageAsync(null, false, builder.Build());
        }
        /// <summary>
        /// A command for showing the user's health, mana and stamina.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show stats", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowMe(CommandArgs<RPGBotConfig> args)
        {
            if (args.Config.HelpChannelID == args.Channel.Id)
            {
                DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);
                Player player = args.Config.GetPlayer(member.Id);

                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                builder.WithTitle($"Resources for {member.DisplayName}:");
                builder.WithDescription($"Title: {player.Title}");
                builder.WithThumbnail(member.AvatarUrl);
                builder.WithColor(member.Color);

                string healthBar = CreateBar("🟥", "⬛", player.Resources.Health, player.Resources.MaxHealth);
                builder.AddField($"Health: {player.Resources.Health}/{player.Resources.MaxHealth}", healthBar);

                string manaBar = CreateBar("🟦", "⬛", player.Resources.Mana, player.Resources.MaxMana);
                builder.AddField($"Mana: {player.Resources.Mana}/{player.Resources.MaxMana}", manaBar);

                string staminaBar = CreateBar("🟩", "⬛", player.Resources.Stamina, player.Resources.MaxStamina);
                builder.AddField($"Stamina: {player.Resources.Stamina}/{player.Resources.MaxStamina}", staminaBar);

                builder.AddField($"Status: {(player.IsAlive ? "Alive" : "Dead")}", "*For now...*");

                await args.Channel.SendMessageAsync(null, false, builder.Build());
            }
        }
        /// <summary>
        /// A command for toggling whether a channel can host pvp commands.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("toggle pvp channel", CommandLevel = CommandLevel.Admin)]
        protected async Task TogglePVPChannel(CommandArgs<RPGBotConfig> args)
        {
            if (args.Config.PVPChannels.Contains(args.Channel.Id))
            {
                args.Config.PVPChannels.Remove(args.Channel.Id);
                await args.Channel.SendMessageAsync("This channel is no longer a PvP-enabled zone!");
            }
            else
            {
                args.Config.PVPChannels.Add(args.Channel.Id);
                await args.Channel.SendMessageAsync("This channel is now a PvP-enabled zone! Watch out gamers!");
            }
        }
        /// <summary>
        /// Attempts to forge an item given the supplied arguments.
        /// The supplied arguments are given as a simple Json string, which should match the damage
        /// profile data structure.
        /// Normally I wouldn't use serialized data as input, but as far as I'm aware this particular
        /// implementation is safe.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("forge item", CommandLevel = CommandLevel.Admin, ParameterRegex = "```(?<json>[\\w\\W\\n]*)```")]
        protected async Task ForgeItem(CommandArgs<RPGBotConfig> args)
        {
            string json = args["json"];

            try
            {
                Item item = JsonSerializer.Deserialize<Item>(json);

                args.Config.Items[item.Name] = item;

                DiscordEmbed embed = item.CreateEmbed();

                await args.Channel.SendMessageAsync("A new item has been forged!", false, embed);
            }
            catch (JsonException e)
            {
                await args.Channel.SendMessageAsync("The Json entered wasn't valid.");
            }
        }

        /// <summary>
        /// Fires when a user adds a reaction to a post.
        /// The user and post author cannot be bots.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        protected async override Task ReactionAdded(ReactionAddArgs<RPGBotConfig> args)
        {
            DiscordMessage message = await args.Channel.GetMessageAsync(args.Message.Id);
            string emojiName = args.Emoji.GetDiscordName();
            if (args.User.Id != message.Author.Id)
            {
                Player player = args.Config.GetPlayer(message.Author.Id);

                player.ChangeCurrency(emojiName, 1);
            }
            else
            {
                if (!lastWarnings.TryGetValue(args.User.Id, out DateTime lastWarn) ||
                    DateTime.Now - lastWarn > TimeSpan.FromMinutes(10))
                {
                    await args.Channel.SendMessageAsync($"Nice try, {args.User.Mention}...");
                    lastWarnings[args.User.Id] = DateTime.Now;
                }
            }
        }
        /// <summary>
        /// Fires when a user removes a reaction to a post.
        /// The user and post author cannot be bots.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        protected async override Task ReactionRemoved(ReactionRemoveArgs<RPGBotConfig> args)
        {
            DiscordMessage message = await args.Channel.GetMessageAsync(args.Message.Id);
            if (args.User.Id != message.Author.Id)
            {
                Player player = args.Config.GetPlayer(message.Author.Id);

                string emojiName = args.Emoji.GetDiscordName();
                player.ChangeCurrency(emojiName, -1);
            }
        }
        /// <summary>
        /// Generates the default config file for this bot.
        /// </summary>
        /// <returns>The default config object.</returns>
        protected override RPGBotConfig CreateDefaultConfig()
        {
            return new RPGBotConfig('!');
        }

        /// <summary>
        /// A small helper method that creates a stat bar for visual aid.
        /// </summary>
        /// <param name="unit">The block/character used for each quantized part of the bar.</param>
        /// <param name="value">The value of the statistic the bar represents.</param>
        /// <param name="maxValue">The maximum value of the statistic the bar represents.</param>
        /// <returns>A string containing the formatted bar.</returns>
        public static string CreateBar(string unit, string antiUnit, int value, int maxValue)
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0 ; i < BAR_RESOLUTION; i++)
            {
                if (maxValue / BAR_RESOLUTION * i < value)
                {
                    builder.Append(unit);
                }
                else
                {
                    builder.Append(antiUnit);
                }
            }
            return builder.ToString();
        }
    }
}