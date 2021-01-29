using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus.Entities;

namespace RPGBot
{
    public partial class RPGManagerBot
    {
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
            DamageProfile.Result result = attacker.Weapon + defender.Armor;
            defender.Resources.AlterHealth(-result.Damage);
            attacker.Resources.AlterHealth(-attacker.Weapon.HealthCost);
            attacker.Resources.AlterMana(-attacker.Weapon.ManaCost);
            attacker.Resources.AlterStamina(-attacker.Weapon.StaminaCost);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"{author.DisplayName} attacks {target.DisplayName}!");
            builder.WithDescription($"Using {attacker.Weapon.Description} against {defender.Armor.Description}");
            builder.WithColor(author.Color);

            if (result.Miss)
            {
                builder.AddField("**But misses!**", "*How embarassing.*");
            }
            else
            {
                if (result.CriticalHit)
                {
                    builder.AddField("🗡 **Critical hit!** 🗡", $"{attacker.Weapon.CriticalStrike} vs {defender.Armor.Protection}");
                }
                if (result.PrimaryResisted)
                {
                    builder.AddField("🛡 **Primary stat resisted!** 🛡", $"{attacker.Weapon.PrimaryType}");
                }
                if (result.SecondaryResisted)
                {
                    builder.AddField("✋ **Secondary stat resisted!** ✋", $"{attacker.Weapon.SecondaryType}");
                }
                if (result.PrimaryVulnerable)
                {
                    builder.AddField("⚡ **Supereffective!** ⚡", $"{attacker.Weapon.PrimaryType}");
                }
                if (result.SecondaryVulnerable)
                {
                    builder.AddField("👊 **Effective!** 👊", $"{attacker.Weapon.SecondaryType}");
                }
                builder.AddField($"⚔ **Damage Dealt** 🏹", $"*{result.Damage} Health*");
            }

            DiscordEmbed embed = builder.Build();

            await args.Channel.SendMessageAsync(null, false, embed);
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
                                    if (await IsTargetInRange(attacker.Weapon, target.Id, args))
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
        /// A command for equipping a weapon from an inventory slot.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("equip weapon", CommandLevel = CommandLevel.Unrestricted, ParameterRegex = "(?<index>\\d+)")]
        protected async Task EquipWeapon(CommandArgs<RPGBotConfig> args)
        {
            int index = int.Parse(args["index"]);

            Player player = args.Config.GetPlayer(args.Author.Id);

            player.WeaponIndex = index;

            await args.Channel.SendMessageAsync($"{args.Author.Mention} inventory slot **{index}** is now equipped as a weapon.");
        }
    }
}