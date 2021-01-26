using System.Text;
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

        /// <summary>
        /// A command for issuing an attack on another user.
        /// The target user must have a recent post in the channel within the weapon's range.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("attack", CommandLevel = CommandLevel.Unrestricted, ParameterRegex = "<@!(?<targetID>\\d+)>")]
        protected async Task Attack(CommandArgs<RPGBotConfig> args)
        {
            ulong targetID = ulong.Parse(args["targetID"]);
            DiscordMember author = await args.Guild.GetMemberAsync(args.Author.Id);
            DiscordMember target = await args.Guild.GetMemberAsync(targetID);

            if (target != null)
            {
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
                                DamageProfile.Result result = attacker.Damage + defender.Armor;
                                defender.Resources.AlterHealth(-result.Damage);
                                attacker.Resources.AlterStamina(-attacker.Damage.ManaCost);
                                attacker.Resources.AlterStamina(-attacker.Damage.StaminaCost);
                                
                                DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                                builder.WithTitle($"{author.DisplayName}[{attacker.Title}] attacks {target.DisplayName}[{defender.Title}]!");
                                builder.WithDescription($"Using {attacker.Damage.Description}");
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
        /// <summary>
        /// A command for showing the user's purse.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show purse", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowPurse(CommandArgs<RPGBotConfig> args)
        {
            DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);
            Player player = args.Config.GetPlayer(member.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"**{member.DisplayName}'s Purse:**");
            builder.WithThumbnail(member.AvatarUrl);
            builder.WithColor(member.Color);
            
            foreach (var currency in player.Currency)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(args.Instance.Client, currency.Key);
                builder.AddField(emoji.FormatName(), $"**{currency.Value}**", true);
            }

            await args.Channel.SendMessageAsync(null, false, builder.Build());
        }
        /// <summary>
        /// A command for showing the user's health, mana and stamina.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("show resources", CommandLevel = CommandLevel.Unrestricted)]
        protected async Task ShowResources(CommandArgs<RPGBotConfig> args)
        {
            DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);
            Player player = args.Config.GetPlayer(member.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"**{member.DisplayName}'s Resources:**");
            builder.WithThumbnail(member.AvatarUrl);
            builder.WithColor(member.Color);

            string healthBar = CreateBar("🟥", "⬛", player.Resources.Health, player.Resources.MaxHealth);
            builder.AddField($"Health: {player.Resources.Health}/{player.Resources.MaxHealth}", healthBar);

            string manaBar = CreateBar("🟦", "⬛", player.Resources.Mana, player.Resources.MaxMana);
            builder.AddField($"Mana: {player.Resources.Mana}/{player.Resources.MaxMana}", manaBar);

            string staminaBar = CreateBar("🟩", "⬛", player.Resources.Stamina, player.Resources.MaxStamina);
            builder.AddField($"Stamina: {player.Resources.Stamina}/{player.Resources.MaxStamina}", staminaBar);

            await args.Channel.SendMessageAsync(null, false, builder.Build());
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
                await args.Channel.SendMessageAsync($"Nice try, {args.User.Mention}...");
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