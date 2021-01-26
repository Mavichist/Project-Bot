using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RPGBot
{
    /// <summary>
    /// Represents the clothing and armor worn by a player.
    /// </summary>
    public class ArmorProfile
    {
        /// <summary>
        /// A description of the armor.
        /// </summary>
        [JsonInclude]
        public string Description
        {
            get;
            private set;
        } = "Rags";
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
        } = 19;
        /// <summary>
        /// Indicates how likely the wearer is to dodge an attack.
        /// </summary>
        [JsonInclude]
        public int Dodge
        {
            get;
            set;
        } = 1;
        /// <summary>
        /// A list of damage types that this armor resists.
        /// </summary>
        [JsonInclude]
        public List<DamageType> Resistances
        {
            get;
            set;
        } = new List<DamageType>();
        /// <summary>
        /// A list of damage types this armor is vulnerable to.
        /// </summary>
        [JsonInclude]
        public List<DamageType> Vulnerabilities
        {
            get;
            set;
        } = new List<DamageType>();
        
        /// <summary>
        /// This is the preferred way of copying the stats of an armor profile to a player because
        /// it guarantees immutability even though the class itself is not immutable.
        /// </summary>
        /// <param name="other">The other armor profile to copy from.</param>
        public void CopyFrom(ArmorProfile other)
        {
            Description = other.Description;
            Magnitude = other.Magnitude;
            Spread = other.Spread;
            Protection = other.Protection;
            Dodge = other.Dodge;
            Resistances = other.Resistances;
            Resistances.Clear();
            Resistances.AddRange(other.Resistances);
            Vulnerabilities.Clear();
            Vulnerabilities.AddRange(other.Vulnerabilities);
        }
    }
}