using System.Text.RegularExpressions;
using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace RoleBot
{
    public class RoleManagerBot : Bot<RoleBotConfig>
    {
        public RoleManagerBot(string name) : base(name)
        {

        }

        private async Task ReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
        {
            RoleBotConfig config = GetConfig(args.Guild);

            if (args.Message.Id == config.RolePostID)
            {
                if (config.EmojiRoles.TryGetValue(args.Emoji.GetDiscordName(), out ulong roleID))
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
        private async Task ReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs args)
        {
            RoleBotConfig config = GetConfig(args.Guild);

            if (args.Message.Id == config.RolePostID)
            {
                if (config.EmojiRoles.TryGetValue(args.Emoji.GetDiscordName(), out ulong roleID))
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
        [CommandAttribute("register", CommandLevel = CommandLevel.Admin, ParameterRegex = ":(?<emoji>[\\w\\d-_]+):\\s+\\<@(?<roleID>\\d+)\\>")]
        private async Task Register(Match match, MessageCreateEventArgs args)
        {
            RoleBotConfig config = GetConfig(args.Guild);

            string emoji = match.Groups["emoji"].Value;
            if (!config.EmojiRoles.TryGetValue(emoji, out ulong roleID))
            {
                roleID = ulong.Parse(match.Groups["roleID"].Value);
                config.EmojiRoles.Add(emoji, roleID);
                DiscordRole role = args.Guild.GetRole(roleID);
                await args.Channel.SendMessageAsync($":{emoji}: has been mapped to {role.Name}.");
            }
            else
            {
                DiscordRole role = args.Guild.GetRole(roleID);
                await args.Channel.SendMessageAsync($":{emoji}: is already mapped to {role?.Name ?? "a role that no longer exists."}");
            }
        }
        [CommandAttribute("deregister", CommandLevel = CommandLevel.Admin, ParameterRegex = ":(?<emoji>[\\w\\d-_]+):\\s+\\<@(?<roleID>\\d+)\\>")]
        private async Task Deregister(Match match, MessageCreateEventArgs args)
        {
            RoleBotConfig config = GetConfig(args.Guild);

            string emoji = match.Groups["emoji"].Value;
            if (config.EmojiRoles.TryGetValue(emoji, out ulong roleID))
            {
                config.EmojiRoles.Remove(emoji);
                DiscordRole role = args.Guild.GetRole(roleID);
                await args.Channel.SendMessageAsync($"{role.Name} is no longer associated with :{emoji}:");
            }
            else
            {
                await args.Channel.SendMessageAsync($"No role is mapped to :{emoji}:");
            }
        }
        [CommandAttribute("make role post", CommandLevel = CommandLevel.Admin, ParameterRegex = "(?<postID>\\d+)")]
        private async Task SetRolePost(Match match, MessageCreateEventArgs args)
        {
            RoleBotConfig config = GetConfig(args.Guild);

            ulong postID = ulong.Parse(match.Groups["postID"].Value);
            DiscordMessage message = await args.Channel.GetMessageAsync(postID);
            if (message != null)
            {
                config.RolePostID = postID;
                await args.Channel.SendMessageAsync("I will now monitor this post for reactions.");
            }
            else
            {
                await args.Channel.SendMessageAsync("That post doesn't exist in this channel.");
            }
        }

        protected override void OnConnected(DiscordClient client)
        {
            client.MessageReactionAdded += ReactionAdded;
            client.MessageReactionRemoved += ReactionRemoved;
        }

        public override RoleBotConfig CreateDefaultConfig()
        {
            return new RoleBotConfig('!');
        }
    }
}