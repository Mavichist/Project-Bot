using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace RPGBot
{
    /// <summary>
    /// Represents a configuration for the AwardManagerBot.
    /// </summary>
    public class RPGBotConfig : BotConfig
    {
        /// <summary>
        /// A set of emoji names and their associated point values.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, int> EmojiPoints
        {
            get;
            private set;
        } = new Dictionary<string, int>();
        /// <summary>
        /// A set of awards currently registered with the bot.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, Title> Titles
        {
            get;
            private set;
        } = new Dictionary<string, Title>();
        /// <summary>
        /// A set of user statistics for all members of the current server.
        /// </summary>
        /// <value></value>
        [JsonInclude]
        public Dictionary<ulong, UserEmojiStats> UserStats
        {
            get;
            private set;
        } = new Dictionary<ulong, UserEmojiStats>();
        /// <summary>
        /// A set of abilities registered to this server.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, Ability> Abilities
        {
            get;
            private set;
        } = new Dictionary<string, Ability>();

        /// <summary>
        /// Instantiates a new instance of an award bot config.
        /// </summary>
        /// <param name="indicator">The character each command must start with.</param>
        /// <returns></returns>
        public RPGBotConfig(char indicator) : base(indicator)
        {

        }

        /// <summary>
        /// Retrieves the statistics object for a user, creating one if one does not already exist.
        /// </summary>
        /// <param name="userID">The Discord identifier for the user.</param>
        /// <returns>A statistics object for the user.</returns>
        public UserEmojiStats GetStats(ulong userID)
        {
            if (!UserStats.TryGetValue(userID, out UserEmojiStats stats))
            {
                stats = new UserEmojiStats();
                UserStats.Add(userID, stats);
            }
            return stats;
        }
        /// <summary>
        /// Returns the number of points an emoji is worth.
        /// Emojis not registered have a point value of 0.
        /// </summary>
        /// <param name="emojiName">The name of the emoji.</param>
        /// <returns>The number of points the emoji is worth.</returns>
        public int GetPoints(string emojiName)
        {
            if (EmojiPoints.TryGetValue(emojiName, out int points))
            {
                return points;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Determines if a user meets the requirements for a title.
        /// </summary>
        /// <param name="userID">The ID of the user to check.</param>
        /// <param name="titleName">The name of the title to check requirements for.</param>
        /// <returns>True if the user is eligible for the title, false if not.
        /// Returns false if either the user or the title do not exist.</returns>
        public bool HasTitle(ulong userID, string titleName)
        {
            if (Titles.TryGetValue(titleName, out Title award))
            {
                if (UserStats.TryGetValue(userID, out UserEmojiStats stats))
                {
                    return stats.EligibleFor(award);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Retrieves the total points for a user.
        /// </summary>
        /// <param name="userID">The ID for the user.</param>
        /// <returns>An integer denoting the total number of points the user has.</returns>
        public int GetTotalPoints(ulong userID)
        {
            if (UserStats.TryGetValue(userID, out UserEmojiStats stats))
            {
                int total = 0;

                foreach (var emoji in stats.EmojiCounts)
                {
                    total += GetPoints(emoji.Key) * emoji.Value;
                }

                return total;
            }
            else
            {
                return 0;
            }
        }
    }
}