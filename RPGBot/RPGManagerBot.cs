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
    public class RPGManagerBot : BotInstance.Bot<RPGBotConfig>
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
        /// Determines whether the target is within weapons range.
        /// </summary>
        /// <param name="damage">The weapon used for the attack.</param>
        /// <param name="targetID">The ID of the target user.</param>
        /// <param name="args">The command arguments for the attack.</param>
        /// <returns></returns>
        private async Task<bool> IsTargetInRange(DamageProfile damage, ulong targetID, CommandArgs<RPGBotConfig> args)
        {
            foreach (var post in await args.Channel.GetMessagesBeforeAsync(args.Message.Id, damage.Range))
            {
                if (post.Author.Id == targetID)
                {
                    return true;
                }
            }
            return false;
        }
        private async Task DoAttack(CommandArgs<RPGBotConfig> args, DiscordMember author, DiscordMember target, Player attacker, Player defender)
        {
            DamageProfile.Result result = attacker.Damage + defender.Armor;
            defender.Resources.AlterHealth(-result.Damage);
            attacker.Resources.AlterStamina(-attacker.Damage.ManaCost);
            attacker.Resources.AlterStamina(-attacker.Damage.StaminaCost);
            
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"{author.DisplayName} attacks {target.DisplayName}!");
            builder.WithDescription($"Using {attacker.Damage.Description} against {defender.Armor.Description}");
            builder.WithColor(author.Color);
            
            if (result.Miss)
            {
                builder.AddField("**But misses!**", "*How embarassing.*");
            }
            else
            {
                if (result.CriticalHit)
                {
                    builder.AddField("🗡 **Critical hit!** 🗡", $"{attacker.Damage.CriticalStrike} vs {defender.Armor.Protection}");
                }
                if (result.PrimaryResisted)
                {
                    builder.AddField("🛡 **Primary stat resisted!** 🛡", $"{attacker.Damage.PrimaryType}");
                }
                if (result.SecondaryResisted)
                {
                    builder.AddField("✋ **Secondary stat resisted!** ✋", $"{attacker.Damage.SecondaryType}");
                }
                if (result.PrimaryVulnerable)
                {
                    builder.AddField("⚡ **Supereffective!** ⚡", $"{attacker.Damage.PrimaryType}");
                }
                if (result.SecondaryVulnerable)
                {
                    builder.AddField("👊 **Effective!** 👊", $"{attacker.Damage.SecondaryType}");
                }
                builder.AddField($"⚔ **Damage Dealt** 🏹", $"*{result.Damage} Health*");
            }

            DiscordEmbed embed = builder.Build();

            await args.Channel.SendMessageAsync(null, false, embed);
        }
        private DiscordEmbed GetWeaponEmbed(string name, DamageProfile profile)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle("New weapon created!");
            builder.WithDescription($"{name} - {profile.Description}");

            builder.AddField("⚔ Damage Magnitude", $"{profile.Magnitude}");
            builder.AddField("📈 Damage Spread", $"{profile.Spread}");
            builder.AddField("🗡 Critical Strike", $"{profile.CriticalStrike}");
            builder.AddField("🎯 Accuracy", $"{profile.Accuracy}");
            builder.AddField("💥 Primary Type", $"{profile.PrimaryType}");
            builder.AddField("🔥 Secondary Type", $"{profile.SecondaryType}");
            builder.AddField("🔵 Mana Cost", $"{profile.ManaCost}");
            builder.AddField("🟢 Stamina Cost", $"{profile.StaminaCost}");

            return builder.Build();
        }
        private DiscordEmbed GetArmorEmbed(string name, ArmorProfile profile)
    {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle("New armor piece created!");
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
        /// A command for issuing an attack on another user.
        /// The target user must have a recent post in the channel within the weapon's range.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("attack", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task Attack(CommandArgs<RPGBotConfig> args)
        {
            if (args.Config.PVPChannels.Contains(args.Channel.Id))
            {
                DiscordMember author = await args.Guild.GetMemberAsync(args.Author.Id);
                if (args.MentionedUsers.Count > 0)
                {
                    DiscordUser targetUser = args.FirstMentionedUser;

                    if (targetUser != null)
                    {
                        DiscordMember target = await args.Guild.GetMemberAsync(targetUser.Id);
                        Player attacker = args.Config.GetPlayer(author.Id);
                        Player defender = args.Config.GetPlayer(target.Id);

                        if (attacker.IsAlive)
                        {
                            if (defender.IsAlive)
                            {
                                if (attacker.CanAttack)
                                {
                                    if (await IsTargetInRange(attacker.Damage, target.Id, args))
                                    {
                                        await DoAttack(args, author, target, attacker, defender);
                                    }
                                    else
                                    {
                                        await args.Channel.SendMessageAsync($"{author.Mention} the target is out of range!");
                                    }    
                                }
                                else
                                {
                                    await args.Channel.SendMessageAsync($"{author.Mention} you don't have enough resources to attack at the moment!");
                                }
                            }
                            else
                            {
                                await args.Channel.SendMessageAsync($"{author.Mention} the target is already dead!");
                            }
                        }
                        else
                        {
                            await args.Channel.SendMessageAsync($"{author.Mention} you can't attack because you're dead!");
                        }
                    }
                    else
                    {
                        await args.Channel.SendMessageAsync($"{author.Mention} the target user doesn't seem to exist. Havin' a giggle?");
                    }
                }
                else
                {
                    await args.Channel.SendMessageAsync($"{author.Mention} you have to specify a target.");
                }
            }
        }
        /// <summary>
        /// A command for showing the user's purse.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show stash", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowStash(CommandArgs<RPGBotConfig> args)
        {
            if (args.Config.StatChannels.Contains(args.Channel.Id))
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
        /// A command for showing the user's health, mana and stamina.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show stats", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowMe(CommandArgs<RPGBotConfig> args)
        {
            if (args.Config.StatChannels.Contains(args.Channel.Id))
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
        /// Attempts to forge a weapon given the supplied arguments.
        /// The supplied arguments are given as a simple Json string, which should match the damage
        /// profile data structure.
        /// Normally I wouldn't use serialized data as input, but as far as I'm aware this particular
        /// implementation is safe.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("forge weapon", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>.+)\"\\s+```(?<json>.*)```")]
        protected async Task ForgeWeapon(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];
            string json = args["json"];

            try
            {
                DamageProfile profile = JsonSerializer.Deserialize<DamageProfile>(json);

                args.Config.Weapons[name] = profile;

                DiscordEmbed embed = GetWeaponEmbed(name, profile);
                
                await args.Channel.SendMessageAsync(null, false, embed);
            }
            catch (JsonException e)
            {
                await args.Channel.SendMessageAsync("The Json entered wasn't valid.");
            }
        }
        /// <summary>
        /// A command for showing a specific weapon.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show weapon", CommandLevel = CommandLevel.Unrestricted, ParameterRegex = "\"(?<name>.+)\"")]
        protected async Task ShowWeapon(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];

            if (args.Config.Weapons.TryGetValue(name, out DamageProfile profile))
            {
                DiscordEmbed embed = GetWeaponEmbed(name, profile);
                await args.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                await args.Channel.SendMessageAsync("That weapon doesn't exist.");
            }
        }
        /// <summary>
        /// A command for listing all weapons available on a server.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show all weapons", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowAllWeapons(CommandArgs<RPGBotConfig> args)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle("All weapons on this server:");
            foreach (var weapon in args.Config.Weapons)
            {
                builder.AddField(weapon.Key, weapon.Value.Description);
            }
            await args.Channel.SendMessageAsync(null, false, builder.Build());
        }
        /// <summary>
        /// A command for removing a weapon from a server.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("remove weapon", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>.+)\"")]
        protected async Task RemoveWeapon(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];

            if (args.Config.Weapons.Remove(name))
            {
                await args.Channel.SendMessageAsync($"I have removed **{name}** from the server.");
            }
            else
            {
                await args.Channel.SendMessageAsync("That weapon does not exist.");
            }
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

                await args.Channel.SendMessageAsync(null, false, embed);
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
        /// A command for toggling whether a channel can host stat commands.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("toggle stat channel", CommandLevel = CommandLevel.Admin)]
        protected async Task ToggleStatChannel(CommandArgs<RPGBotConfig> args)
        {
            if (args.Config.StatChannels.Contains(args.Channel.Id))
            {
                args.Config.StatChannels.Remove(args.Channel.Id);
                await args.Channel.SendMessageAsync("This channel is no longer a stats channel!");
            }
            else
            {
                args.Config.StatChannels.Add(args.Channel.Id);
                await args.Channel.SendMessageAsync("This channel is now a stats channel!");
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