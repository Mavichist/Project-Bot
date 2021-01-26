using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RPGBot
{
    /// <summary>
    /// Represents the emoji reaction statistics, health and mana of a single user.
    /// </summary>
    public class Player
    {
        /// <summary>
        /// A set of emoji names and their corresponding counts.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, int> Currency
        {
            get;
            private set;
        } = new Dictionary<string, int>();
        [JsonInclude]
        public string Title
        {
            get;
            set;
        } = "Adventurer";
        [JsonInclude]
        public DamageProfile Damage
        {
            get;
            private set;
        } = new DamageProfile();
        [JsonInclude]
        public ArmorProfile Armor
        {
            get;
            private set;
        } = new ArmorProfile();
        [JsonInclude]
        public ResourceProfile Resources
        {
            get;
            private set;
        } = new ResourceProfile();
        public bool IsAlive
        {
            get
            {
                return Resources.Health > 0;
            }
        }
        public bool CanAttack
        {
            get
            {
                return IsAlive &&
                    Resources.Mana >= Damage.ManaCost && 
                    Resources.Stamina >= Damage.StaminaCost;
            }
        }

        /// <summary>
        /// Retrieves the number of times the bot has witnessed a reaction to the user's posts with
        /// the specified emoji.
        /// </summary>
        /// <param name="emoji">The emoji to check the count of.</param>
        /// <returns>An integer denoting the number of times the emoji has been used as a reaction
        /// to the user's posts.</returns>
        public int GetCurrency(string emoji)
        {
            if (Currency.TryGetValue(emoji, out int count))
            {
                return count;
            }
            else
            {
                return 0;
            }
        }
        /// <summary>
        /// Increments the current number of specified emojis the user has.
        /// </summary>
        /// <param name="emojiName">The name of the emoji to increment.</param>
        /// <param name="amount">The amount to increment by.</param>
        /// <returns>An integer denoting the new value.</returns>
        public int ChangeCurrency(string emojiName, int amount)
        {
            int newAmount = 0;
            if (Currency.TryGetValue(emojiName, out int current))
            {
                newAmount = Math.Max(current + amount, 0);
                Currency[emojiName] = newAmount;
            }
            else
            {
                newAmount = Math.Max(amount, 0);
                Currency[emojiName] = newAmount;
            }
            return newAmount;
        }
        /// <summary>
        /// Sets the emoji count for a specified emoji.
        /// </summary>
        /// <param name="emojiName">The name of the emoji to increment.</param>
        /// <param name="amount">The count to set the value to.</param>
        public void SetCurrency(string emojiName, int count)
        {
            Currency[emojiName] = Math.Max(count, 0);
        }
    }
}