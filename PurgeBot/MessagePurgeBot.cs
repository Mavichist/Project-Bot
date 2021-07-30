using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus.Entities;

namespace PurgeBot
{
    public class MessagePurgeBot : BotInstance.Bot<PurgeBotConfig>
    {
        public const int MAX_GAP = 100;

        /// <summary>
        /// Creates a new instance of a message purging bot with the specified name.
        /// </summary>
        /// <param name="name">The name of the bot (used to load config files).</param>
        public MessagePurgeBot(string name) : base(name)
        {

        }
        
        private async Task Purge(CommandArgs<PurgeBotConfig> args, CancellationToken token)
        {
            DiscordMember purger = await args.Guild.GetMemberAsync(args.Author.Id);
            int messageCount = int.Parse(args["messageCount"]);
            int purged = 0;
            int gap = 0;
            DiscordMessage lastMessage = args.Message;
            HashSet<ulong> userIDs = new HashSet<ulong>();
            foreach (DiscordUser user in args.MentionedUsers)
            {
                userIDs.Add(user.Id);
            }
            while (lastMessage != null)
            {
                var messageList = await args.Channel.GetMessagesBeforeAsync(lastMessage.Id);
                foreach (DiscordMessage message in messageList)
                {
                    if (userIDs.Count == 0 || userIDs.Contains(message.Author.Id))
                    {
                        await args.Channel.DeleteMessageAsync(message, "Purged on request.");
                        purged++;
                        gap = 0;
                        await Task.Delay(500);
                    }
                    else
                    {
                        gap++;
                    }
                    if (purged >= messageCount || gap >= MAX_GAP)
                    {
                        await purger.SendMessageAsync($"Purge complete. {purged} purged messages, gap was {gap}/{MAX_GAP}.");
                        return;
                    }
                    if (token.IsCancellationRequested)
                    {
                        await purger.SendMessageAsync("Purge cancelled.");
                        return;
                    }
                }
                lastMessage = messageList.Count > 0 ? messageList[messageList.Count - 1] : null;
            }
            await purger.SendMessageAsync($"Purge request hit the end of {args.Channel.Name}'s history.");
        }

        [Command("purge", CommandLevel = CommandLevel.Admin, ParameterRegex ="^(?<messageCount>\\d+)")]
        private async Task Purge(CommandArgs<PurgeBotConfig> args)
        {
            CancellationToken token;
            lock (args.Config.TokenSource)
            {
                token = args.Config.TokenSource.Token;
            }
            Task.Run(() => Purge(args, token));
        }
        [Command("cancel purge", CommandLevel = CommandLevel.Admin)]
        private async Task CancelPurge(CommandArgs<PurgeBotConfig> args)
        {
            lock (args.Config.TokenSource)
            {
                args.Config.TokenSource.Cancel();
                args.Config.TokenSource = new CancellationTokenSource();
            }
            DiscordMember canceller = await args.Guild.GetMemberAsync(args.Author.Id);
            await canceller.SendMessageAsync("All active purge operations are being cancelled.");
        }

        /// <summary>
        /// Creates a default config data structure for new servers.
        /// </summary>
        /// <returns>a config data structure.</returns>
        protected override PurgeBotConfig CreateDefaultConfig()
        {
            return new PurgeBotConfig('!');
        }
    }
}