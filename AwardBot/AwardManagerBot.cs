using System;
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
        [CommandAttribute("set emoji points", CommandLevel = CommandLevel.Admin, ParameterRegex = "`(?<emojiName>:[\\w\\d-_]+:)`\\s+(?<points>-?\\d+)")]
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

                if (emoji.RequiresColons)
                {
                    await args.Channel.SendMessageAsync($"<{emojiName}{emoji.Id}> is now worth {points} points.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"{emojiName} is now worth {points} points.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("That emoji doesn't seem to exist.");
            }
        }
        /// <summary>
        /// A command for removing an emoji from the award bot tracking system.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("remove emoji points", CommandLevel = CommandLevel.Admin, ParameterRegex = "`(?<emojiName>:[\\w\\d-_]+:)`")]
        private async Task RemoveEmojiPoints(CommandArgs<AwardBotConfig> args)
        {
            string emojiName = args["emojiName"];
            
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
                if (args.Config.EmojiPoints.Remove(emojiName))
                {
                    if (emoji.RequiresColons)
                    {
                        await args.Channel.SendMessageAsync($"I now don't associate <{emojiName}{emoji.Id}> with any points.");
                    }
                    else
                    {
                        await args.Channel.SendMessageAsync($"I now don't associate {emojiName} with any points.");
                    }
                }
                else
                {
                    await args.Channel.SendMessageAsync("I don't track that emoji anyway...");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("That emoji doesn't seem to exist.");
            }
        }
        /// <summary>
        /// A command for creating an award.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("create award", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"\\s+\"(?<description>[ -~]+)\"")]
        private async Task CreateAward(CommandArgs<AwardBotConfig> args)
        {
            string name = args["name"];
            string description = args["description"];

            args.Config.Awards[name] = new Award(description);

            await args.Channel.SendMessageAsync($"The **{name}** award has been created.");
        }
        /// <summary>
        /// A command for removing awards.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("remove award", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"")]
        private async Task RemoveAward(CommandArgs<AwardBotConfig> args)
        {
            string name = args["name"];

            if (args.Config.Awards.Remove(name))
            {
                await args.Channel.SendMessageAsync($"The **{name}** award has been removed.");
            }
            else
            {
                await args.Channel.SendMessageAsync($"The **{name}** award does not exist.");
            }
        }
        /// <summary>
        /// A command for setting the emoji requirements for an award.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("set award emoji requirement", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"\\s+`(?<emojiName>:[\\w\\d-_]+:)`\\s+(?<threshold>\\d+)")]
        private async Task SetAwardEmojiRequirement(CommandArgs<AwardBotConfig> args)
        {
            string name = args["name"];
            string emojiName = args["emojiName"];
            int threshold = int.Parse(args["threshold"]);

            if (args.Config.EmojiPoints.ContainsKey(emojiName))
            {
                if (args.Config.Awards.TryGetValue(name, out Award award))
                {
                    award.EmojiRequirements[emojiName] = threshold;

                    DiscordEmoji emoji = DiscordEmoji.FromName(Instance.Client, emojiName);
                    if (emoji.RequiresColons)
                    {
                        await args.Channel.SendMessageAsync($"The **{name}** award now requires **{threshold}x** <{emojiName}{emoji.Id}>.");
                    }
                    else
                    {
                        await args.Channel.SendMessageAsync($"The **{name}** award now requires **{threshold}x** {emojiName}.");
                    }
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{name}** award does not exist.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("I don't track that emoji yet, so I can't use it for awards.");
            }
        }
        /// <summary>
        /// A command for showing user statistics.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("show my stats", CommandLevel = CommandLevel.Unrestricted)]
        private async Task ShowStats(CommandArgs<AwardBotConfig> args)
        {
            UserEmojiStats stats = args.Config.GetStats(args.Author.Id);
            long totalPoints = 0;

            DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"Stats for **{member.DisplayName}**:\n");
            builder.WithThumbnail(member.AvatarUrl);
            builder.WithColor(member.Color);
            
            int iteration = 0;
            foreach (var pair in args.Config.EmojiPoints)
            {
                int emojiCount = stats.GetCount(pair.Key);
                int emojiPoints = pair.Value * stats.GetCount(pair.Key);

                DiscordEmoji emoji = DiscordEmoji.FromName(Instance.Client, pair.Key);

                if (!emoji.RequiresColons)
                {
                    builder.AddField($"**{emojiCount}x **{pair.Key}", $"{emojiPoints}pts", true);
                }
                else
                {
                    builder.AddField($"**{emojiCount}x **<{pair.Key}{emoji.Id}>", $"{emojiPoints}pts", true);
                }

                totalPoints += emojiPoints;
                iteration++;
            }

            builder.WithDescription($"For a total of **{totalPoints}pts**.");

            await args.Channel.SendMessageAsync(null, false, builder.Build());
        }
        /// <summary>
        /// Shows available awards if the user is eligible for them.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("show my awards", CommandLevel = CommandLevel.Unrestricted)]
        private async Task ShowMyAwards(CommandArgs<AwardBotConfig> args)
        {
            DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);
            UserEmojiStats stats = args.Config.GetStats(member.Id);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"Awards for **{member.DisplayName}**:");
            builder.WithThumbnail(member.AvatarUrl);
            builder.WithColor(member.Color);

            foreach (var awards in args.Config.Awards)
            {
                if (stats.EligibleFor(awards.Value))
                {
                    builder.AddField($"**{awards.Key}**", $"{awards.Value.Description}", true);
                }
            }

            await args.Channel.SendMessageAsync(null, false, builder.Build());
        }
        /// <summary>
        /// Shows available awards.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("show all awards", CommandLevel = CommandLevel.Unrestricted)]
        private async Task ShowAllAwards(CommandArgs<AwardBotConfig> args)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"All available awards:");

            foreach (var awards in args.Config.Awards)
            {
                builder.AddField($"**{awards.Key}**", $"{awards.Value.Description}", true);
            }

            await args.Channel.SendMessageAsync(null, false, builder.Build());
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
            string emojiName = args.Emoji.GetDiscordName();
            if (args.User.Id != message.Author.Id)
            {
                UserEmojiStats userStats = args.Config.GetStats(message.Author.Id);

                userStats.Award(emojiName, 1);
            }
            else if (args.Config.GetPoints(emojiName) > 0)
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