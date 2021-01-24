using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RPGBot
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
        public bool EligibleFor(Title award)
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
        /// <summary>
        /// Increments the current number of specified emojis the user has.
        /// </summary>
        /// <param name="emojiName">The name of the emoji to increment.</param>
        /// <param name="amount">The amount to increment by.</param>
        public void IncrementCount(string emojiName, int amount)
        {
            if (EmojiCounts.TryGetValue(emojiName, out int points))
            {
                EmojiCounts[emojiName] = Math.Max(points + amount, 0);
            }
            else
            {
                EmojiCounts[emojiName] = Math.Max(amount, 0);
            }
        }
        /// <summary>
        /// Sets the emoji count for a specified emoji.
        /// </summary>
        /// <param name="emojiName">The name of the emoji to increment.</param>
        /// <param name="amount">The count to set the value to.</param>
        public void SetCount(string emojiName, int count)
        {
            EmojiCounts[emojiName] = Math.Max(count, 0);
        }
    }
}