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
        /// Determines whether the user statistics qualify that user for the specified award.
        /// </summary>
        /// <param name="userStats">The user statistics object.</param>
        /// <param name="awardName">The name of the award.</param>
        /// <returns>A boolean value indicating eligibility. Returns false if the award doesn't exist.</returns>
        public bool EligibleFor(UserEmojiStats userStats, string awardName)
        {
            if (Awards.TryGetValue(awardName, out Award award))
            {
                foreach (var pair in award.EmojiRequirements)
                {
                    if (userStats.GetCount(pair.Key) < pair.Value)
                    {
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}