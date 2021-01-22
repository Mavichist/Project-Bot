using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AwardBot
{
    /// <summary>
    /// Represents the emoji reaction statistics of a single user.
    /// </summary>
    public class UserEmojiStats
    {
        /// <summary>
        /// A set of emoji names and their corresponding counts.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, int> EmojiCounts
        {
            get;
            private set;
        } = new Dictionary<string, int>();

        /// <summary>
        /// Awards the user with the given number of emojis specified.
        /// </summary>
        /// <param name="emoji">The emoji to award the user.</param>
        /// <param name="count">The number of instances to award them.</param>
        public void Award(string emoji, int count)
        {
            if (!EmojiCounts.ContainsKey(emoji))
            {
                EmojiCounts.Add(emoji, count);
            }
            else
            {
                EmojiCounts[emoji] += count;
            }
        }
        /// <summary>
        /// Retrieves the number of times the bot has witnessed a reaction to the user's posts with
        /// the specified emoji.
        /// </summary>
        /// <param name="emoji">The emoji to check the count of.</param>
        /// <returns>An integer denoting the number of times the emoji has been used as a reaction
        /// to the user's posts.</returns>
        public int GetCount(string emoji)
        {
            if (EmojiCounts.TryGetValue(emoji, out int count))
            {
                return count;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Determines whether the user statistics qualify that user for the specified award.
        /// </summary>
        /// <param name="userStats">The user statistics object.</param>
        /// <param name="awardName">The name of the award.</param>
        /// <returns>A boolean value indicating eligibility. Returns false if the award doesn't exist.</returns>
        public bool EligibleFor(Award award)
        {
            foreach (var pair in award.EmojiRequirements)
            {
                if (GetCount(pair.Key) < pair.Value)
                {
                    return false;
                }
            }
            return true;
        }
    }
}