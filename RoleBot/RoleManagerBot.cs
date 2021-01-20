using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus.Entities;

namespace RoleBot
{
    public class RoleManagerBot : BotInstance.Bot<RoleBotConfig>
    {
        /// <summary>
        /// Creates a new instance of a role manager bot with the specified name.
        /// </summary>
        /// <param name="name">The name of the bot (used to load config files).</param>
        public RoleManagerBot(string name) : base(name)
        {

        }

        /// <summary>
        /// Formats a role message using the roles and emojis in the specified config file.
        /// </summary>
        /// <param name="guild">The guild where the post should appear.</param>
        /// <param name="config">The config object for the bot and server/guild.</param>
        /// <returns>The message string.</returns>
        private string FormatRolePost(DiscordGuild guild, RoleBotConfig config)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("> React to this post with the following to receive roles:\n");
            foreach (var entries in config.EmojiRoles)
            {
                DiscordRole role = guild.GetRole(entries.Value);
                builder.Append($">\t{entries.Key}\t:\t**{role.Name}**\n");
            }
            return builder.ToString();
        }

        /// <summary>
        /// A command for mapping an emoji to a role.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("register emoji", CommandLevel = CommandLevel.Admin, ParameterRegex = "`(?<emojiName>:[\\w\\d-_]+:)`\\s+<@&(?<roleID>\\d+)>")]
        private async Task Register(CommandArgs<RoleBotConfig> args)
        {
            string emojiName = args["emojiName"];
            
            if (emojiName != null)
            {
                if (!args.Config.EmojiRoles.TryGetValue(emojiName, out ulong roleID))
                {
                    roleID = ulong.Parse(args["roleID"]);
                    args.Config.EmojiRoles.Add(emojiName, roleID);
                    DiscordRole role = args.Guild.GetRole(roleID);
                    await args.Channel.SendMessageAsync($"{emojiName} has been mapped to *{role.Name}*.");
                }
                else
                {
                    DiscordRole role = args.Guild.GetRole(roleID);
                    await args.Channel.SendMessageAsync($"{emojiName} is already mapped to *{role?.Name ?? "a role that no longer exists."}*");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync($"The emoji does not exist.");
            }
        }
        /// <summary>
        /// A command for removing an emoji and role combination from the bot's registry.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("deregister emoji", CommandLevel = CommandLevel.Admin, ParameterRegex = "`(?<emojiName>:[\\w\\d-_]+:)`")]
        private async Task Deregister(CommandArgs<RoleBotConfig> args)
        {
            string emojiName = args["emojiName"];
            
            if (args.Config.EmojiRoles.TryGetValue(emojiName, out ulong roleID))
            {
                args.Config.EmojiRoles.Remove(emojiName);
                DiscordRole role = args.Guild.GetRole(roleID);
                await args.Channel.SendMessageAsync($"*{role.Name}* is no longer associated with {emojiName}");
            }
            else
            {
                await args.Channel.SendMessageAsync($"No role is mapped to {emojiName}");
            }
        }
        /// <summary>
        /// A command for creating the post that will be monitored for reactions.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("create reaction post", CommandLevel = CommandLevel.Admin)]
        private async Task CreateRolePost(CommandArgs<RoleBotConfig> args)
        {
            DiscordMessage message = await args.Channel.SendMessageAsync(FormatRolePost(args.Guild, args.Config));
            foreach (var emojiRole in args.Config.EmojiRoles)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(Instance.Client, emojiRole.Key);
                await message.CreateReactionAsync(emoji);
            }
            args.Config.RolePostID = message.Id;
            args.Config.RolePostChannelID = args.Channel.Id;
        }
        /// <summary>
        /// A command for updating the current reaction post with newly listed roles and information.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("update reaction post", CommandLevel = CommandLevel.Admin)]
        private async Task UpdateRolePost(CommandArgs<RoleBotConfig> args)
        {
            if (args.Config.RolePostChannelID != 0 && args.Config.RolePostID != 0)
            {
                DiscordChannel channel = args.Guild.GetChannel(args.Config.RolePostChannelID);
                DiscordMessage message = await channel?.GetMessageAsync(args.Config.RolePostID);
                if (message != null)
                {
                    await message.ModifyAsync(FormatRolePost(args.Guild, args.Config));
                    await message.DeleteAllReactionsAsync();
                    foreach (var emojiRole in args.Config.EmojiRoles)
                    {
                        DiscordEmoji emoji = DiscordEmoji.FromName(Instance.Client, emojiRole.Key);
                        await message.CreateReactionAsync(emoji);
                    }
                }
            }
        }
        /// <summary>
        /// A command for creating a managed role.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("create managed role", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<roleName>[\\w\\d_\\-\\s]+)\"\\s+color\\((?<red>\\d+),(?<green>\\d+),(?<blue>\\d+)\\)")]
        private async Task CreateManagedRole(CommandArgs<RoleBotConfig> args)
        {
            string roleName = args["roleName"];
            byte red = (byte)Math.Clamp(int.Parse(args["red"]), 0, 255);
            byte green = (byte)Math.Clamp(int.Parse(args["green"]), 0, 255);
            byte blue = (byte)Math.Clamp(int.Parse(args["blue"]), 0, 255);

            DiscordRole role = await args.Guild.CreateRoleAsync(roleName, null, new DiscordColor(red, green, blue));

            if (role != null)
            {
                args.Config.ManagedRoles.Add(role.Id);
                await args.Channel.SendMessageAsync($"The <@&{role.Id}> role is being managed.");
            }
            else
            {
                await args.Channel.SendMessageAsync("Failed to create the role.");
            }
        }
        /// <summary>
        /// A command for removing a managed role.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("remove managed role", CommandLevel = CommandLevel.Admin, ParameterRegex = "<@&(?<roleID>\\d+)>")]
        private async Task RemoveManagedRole(CommandArgs<RoleBotConfig> args)
        {
            ulong roleID = ulong.Parse(args["roleID"]);

            if (args.Config.ManagedRoles.Remove(roleID))
            {
                DiscordRole role = args.Guild.GetRole(roleID);
                if (role != null)
                {
                    await role.DeleteAsync();

                    await args.Channel.SendMessageAsync($"**{role.Name}** has been deleted.");

                    // Remove all emojis associated with the managed role.
                    Stack<string> associatedEmojis = new Stack<string>();
                    foreach (var emojiRole in args.Config.EmojiRoles)
                    {
                        if (emojiRole.Value == roleID)
                        {
                            associatedEmojis.Push(emojiRole.Key);
                        }
                    }
                    foreach (string emojiName in associatedEmojis)
                    {
                        args.Config.EmojiRoles.Remove(emojiName);
                    }
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("This role is not managed by this bot.");
            }
        }
        /// <summary>
        /// A command for adding an existing role to the management list.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("manage role", CommandLevel = CommandLevel.Admin, ParameterRegex = "<@&(?<roleID>\\d+)>")]
        private async Task ManageRole(CommandArgs<RoleBotConfig> args)
        {
            ulong roleID = ulong.Parse(args["roleID"]);
            if (!args.Config.ManagedRoles.Contains(roleID))
            {
                DiscordRole role = args.Guild.GetRole(roleID);
                if (role != null)
                {
                    args.Config.ManagedRoles.Add(roleID);
                    await args.Channel.SendMessageAsync($"The **{role.Name}** role is now managed.");
                }
                else
                {
                    await args.Channel.SendMessageAsync("The role does not exist on this server.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("I already manage that role.");
            }
        }
        /// <summary>
        /// A command for listing all roles on the server that are not managed by this bot.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("list unmanaged roles", CommandLevel = CommandLevel.Admin)]
        private async Task ListUnmanagedRoles(CommandArgs<RoleBotConfig> args)
        {
            Dictionary<ulong, DiscordRole> roles = new Dictionary<ulong, DiscordRole>();
            foreach (var role in args.Guild.Roles)
            {
                if (role.Key != args.Guild.EveryoneRole.Id)
                {
                    roles.Add(role.Key, role.Value);
                }
            }

            foreach (var roleID in args.Config.ManagedRoles)
            {
                roles.Remove(roleID);
            }

            StringBuilder builder = new StringBuilder();
            builder.Append("The roles I do not manage on this server are as follows:\n");
            foreach (var role in roles)
            {
                builder.Append($">\t**{role.Value.Name}**\n");
            }
            await args.Channel.SendMessageAsync(builder.ToString());
        }

        /// <summary>
        /// Called when one of this bot's posts has a reaction removed.
        /// </summary>
        /// <param name="args">The context for the reaction removal.</param>
        /// <returns>A task for handling the reaction.</returns>
        protected override async Task ReactionAdded(ReactionAddArgs<RoleBotConfig> args)
        {
            if (args.Message.Id == args.Config.RolePostID)
            {
                string emojiName = args.Emoji.GetDiscordName();
                
                if (args.Config.EmojiRoles.TryGetValue(emojiName, out ulong roleID))
                {
                    DiscordRole role = args.Guild.GetRole(roleID);
                    if (role != null)
                    {
                        DiscordMember member = await args.Guild.GetMemberAsync(args.User.Id);
                        await member.GrantRoleAsync(role, "Role bot granted.");
                    }
                }
            }
        }
        /// <summary>
        /// Called when one of this bot's posts has a reaction added.
        /// </summary>
        /// <param name="args">The context for the reaction addition.</param>
        /// <returns>A task for handling the reaction.</returns>
        protected override async Task ReactionRemoved(ReactionRemoveArgs<RoleBotConfig> args)
        {
            if (args.Message.Id == args.Config.RolePostID)
            {
                string emojiName = args.Emoji.GetDiscordName();
                if (args.Config.EmojiRoles.TryGetValue(emojiName, out ulong roleID))
                {
                    DiscordRole role = args.Guild.GetRole(roleID);
                    if (role != null)
                    {
                        DiscordMember member = await args.Guild.GetMemberAsync(args.User.Id);
                        await member.RevokeRoleAsync(role, "Role bot revoked.");
                    }
                }
            }
        }

        /// <summary>
        /// Creates a default config data structure for new servers.
        /// </summary>
        /// <returns>a config data structure.</returns>
        protected override RoleBotConfig CreateDefaultConfig()
        {
            return new RoleBotConfig('!');
        }
    }
}