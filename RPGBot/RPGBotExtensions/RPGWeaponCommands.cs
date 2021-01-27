using System.Text.Json;
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
            builder.WithTitle("Weapon Profile:");
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
                
                await args.Channel.SendMessageAsync("A new weapon has been forged!", false, embed);
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
        /// A command for giving a series of mentioned users weapons.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("gift weapon", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>.+)\"")]
        protected async Task GiftWeapon(CommandArgs<RPGBotConfig> args)
        {
            string name = args["name"];
            
            if (args.Config.Weapons.TryGetValue(name, out DamageProfile profile))
            {
                foreach (DiscordUser user in args.MentionedUsers)
                {
                    DiscordMember member = await args.Guild.GetMemberAsync(user.Id);
                    Player player = args.Config.GetPlayer(user.Id);
                    
                    player.Damage.CopyFrom(profile);

                    await args.Channel.SendMessageAsync($"**{member.DisplayName}** is now armed with **{name}**, *{profile.Description}*!");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("A weapon with that name does not exist on this server.");
            }
        }
    }
}