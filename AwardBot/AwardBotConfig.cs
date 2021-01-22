using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace AwardBot
{
    /// <summary>
    /// Represents a configuration for the AwardManagerBot.
    /// </summary>
    public class AwardBotConfig : BotConfig
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
        public Dictionary<string, Award> Awards
        {
            get;
            private set;
        } = new Dictionary<string, Award>();
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
        /// Instantiates a new instance of an award bot config.
        /// </summary>
        /// <param name="indicator">The character each command must start with.</param>
        /// <returns></returns>
        public AwardBotConfig(char indicator) : base(indicator)
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
        /// Determines if a user meets the requirements for an award.
        /// </summary>
        /// <param name="userID">The ID of the user to check.</param>
        /// <param name="awardName">The name of the award to check requirements for.</param>
        /// <returns>True if the user is eligible for the award, false if not.
        /// Returns false if either the user or the award do not exist.</returns>
        public bool HasAward(ulong userID, string awardName)
        {
            if (Awards.TryGetValue(awardName, out Award award))
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
    }
}