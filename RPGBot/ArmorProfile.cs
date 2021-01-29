using System.Text.Json.Serialization;

namespace RPGBot
{
    /// <summary>
    /// Represents the clothing and armor worn by a player.
    /// </summary>
    public struct ArmorProfile
    {
        public static ArmorProfile Rags
        {
            get
            {
                return new ArmorProfile()
                {
                    Description = "filthy rags",
                    Magnitude = 4,
                    Spread = 2,
                    Protection = 4,
                    Dodge = 1,
                    Resists = DamageType.None,
                    Vulnerability = DamageType.Fire
                };
            }
        }

        /// <summary>
        /// A description of the armor.
        /// </summary>
        [JsonInclude]
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        /// The total magnitude of this armor's defences.
        /// </summary>
        [JsonInclude]
        public int Magnitude
        {
            get;
            set;
        }
        /// <summary>
        /// The spread of this armor's defence values.
        /// Higher values indicate the armor is unpredictable.
        /// </summary>
        [JsonInclude]
        public int Spread
        {
            get;
            set;
        }
        /// <summary>
        /// A protection value for resisting critical strikes.
        /// </summary>
        [JsonInclude]
        public int Protection
        {
            get;
            set;
        }
        /// <summary>
        /// Indicates how likely the wearer is to dodge an attack.
        /// </summary>
        [JsonInclude]
        public int Dodge
        {
            get;
            set;
        }
        /// <summary>
        /// A list of damage types that this armor resists.
        /// </summary>
        [JsonInclude]
        public DamageType Resists
        {
            get;
            set;
        }
        /// <summary>
        /// A list of damage types this armor is vulnerable to.
        /// </summary>
        [JsonInclude]
        public DamageType Vulnerability
        {
            get;
            set;
        }
    }
}