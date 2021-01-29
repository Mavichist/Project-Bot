using System.Text.Json.Serialization;

namespace RPGBot
{
    /// <summary>
    /// Represents the properties of a consumable item.
    /// </summary>
    public struct ConsumableProfile
    {
        /// <summary>
        /// A brief description of the consumable and what it does.
        /// </summary>
        [JsonInclude]
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        /// When consumed, this amount of health will be given to the user.
        /// </summary>
        [JsonInclude]
        public int HealthGained
        {
            get;
            set;
        }
        /// <summary>
        /// When consumed, this amount of mana will be given to the user.
        /// </summary>
        [JsonInclude]
        public int ManaGained
        {
            get;
            set;
        }
        /// <summary>
        /// When consumed, this amount of stamina will be given to the user.
        /// </summary>
        [JsonInclude]
        public int StaminaGained
        {
            get;
            set;
        }
    }
}