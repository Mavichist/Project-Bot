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
        [CommandAttribute("emoji role register", CommandLevel = CommandLevel.Admin, ParameterRegex = "`(?<emojiName>:[\\w\\d-_]+:)`\\s+<@&(?<roleID>\\d+)>")]
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
        [CommandAttribute("emoji role deregister", CommandLevel = CommandLevel.Admin, ParameterRegex = "`(?<emojiName>:[\\w\\d-_]+:)`\\s+<@&(?<roleID>\\d+)>")]
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
        [CommandAttribute("emoji role create reaction post", CommandLevel = CommandLevel.Admin)]
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
        [CommandAttribute("emoji role update reaction post", CommandLevel = CommandLevel.Admin)]
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