using System;
using System.Text;
using System.Threading.Tasks;
using BotScaffold;
using DSharpPlus.Entities;

namespace AwardBot
{
    /// <summary>
    /// Represents a bot for handling point-based awards, based on user reactions.
    /// </summary>
    public class AwardManagerBot : BotInstance.Bot<AwardBotConfig>
    {
        /// <summary>
        /// Creates a new instance of an award manager bot, with the specified name.
        /// </summary>
        /// <param name="name">The name of this bot (used for loading config).</param>
        public AwardManagerBot(string name) : base(name)
        {

        }

        /// <summary>
        /// A command for setting the points an emoji is worth.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("set emoji points", CommandLevel = CommandLevel.Admin, ParameterRegex = "`(?<emojiName>:[\\w\\d-_]+:)`\\s+(?<points>-*\\d+)")]
        private async Task SetEmojiPoints(CommandArgs<AwardBotConfig> args)
        {
            string emojiName = args["emojiName"];
            int points = int.Parse(args["points"]);
            
            DiscordEmoji emoji;
            try
            {
                emoji = DiscordEmoji.FromName(Instance.Client, emojiName);
            }
            catch (ArgumentException e)
            {
                emoji = null;
            }

            if (emoji != null)
            {
                args.Config.EmojiPoints[emojiName] = points;

                await args.Channel.SendMessageAsync($"{emojiName} is now worth {points} points.");
            }
            else
            {
                await args.Channel.SendMessageAsync("That emoji doesn't seem to exist.");
            }
        }
        /// <summary>
        /// A command for showing user statistics.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("show stats", CommandLevel = CommandLevel.Unrestricted)]
        private async Task ShowStats(CommandArgs<AwardBotConfig> args)
        {
            UserEmojiStats stats = args.Config.GetStats(args.Author.Id);
            int totalPoints = 0;

            DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);

            StringBuilder builder = new StringBuilder();
            builder.Append($"Stats for **{member.DisplayName}**:\n");
            
            foreach (var pair in args.Config.EmojiPoints)
            {
                int emojiCount = stats.GetCount(pair.Key);
                int emojiPoints = pair.Value * stats.GetCount(pair.Key);
                
                DiscordEmoji emoji = DiscordEmoji.FromName(Instance.Client, pair.Key);

                if (!emoji.RequiresColons)
                {
                    builder.Append($">\t**{emojiCount}x**{pair.Key}\t:\t{emojiPoints}\n");
                }
                else
                {
                    builder.Append($">\t**{emojiCount}x**<{pair.Key}{emoji.Id}>\t:\t{emojiPoints}\n");
                }

                totalPoints += emojiPoints;
            }
            builder.Append($">\tFor a total of **{totalPoints}** points.");

            await args.Channel.SendMessageAsync(builder.ToString());
        }

        /// <summary>
        /// Fires when a user adds a reaction to a post.
        /// The user and post author cannot be bots.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        protected async override Task ReactionAdded(ReactionAddArgs<AwardBotConfig> args)
        {
            DiscordMessage message = await args.Channel.GetMessageAsync(args.Message.Id);
            if (args.User.Id != message.Author.Id)
            {
                UserEmojiStats userStats = args.Config.GetStats(message.Author.Id);

                string emojiName = args.Emoji.GetDiscordName();
                userStats.Award(emojiName, 1);
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
        protected async override Task ReactionRemoved(ReactionRemoveArgs<AwardBotConfig> args)
        {
            DiscordMessage message = await args.Channel.GetMessageAsync(args.Message.Id);
            if (args.User.Id != message.Author.Id)
            {
                UserEmojiStats userStats = args.Config.GetStats(message.Author.Id);

                string emojiName = args.Emoji.GetDiscordName();
                userStats.Award(emojiName, -1);
            }
        }

        /// <summary>
        /// Generates the default config file for this bot.
        /// </summary>
        /// <returns>The default config object.</returns>
        protected override AwardBotConfig CreateDefaultConfig()
        {
            return new AwardBotConfig('!');
        }
    }
}