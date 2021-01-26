using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RPGBot
{
    public class ArmorProfile
    {
        [JsonInclude]
        public string Description
        {
            get;
            private set;
        } = "Rags";
        [JsonInclude]
        public int Magnitude
        {
            get;
            set;
        }
        [JsonInclude]
        public int Spread
        {
            get;
            set;
        }
        [JsonInclude]
        public int Protection
        {
            get;
            set;
        } = 19;
        [JsonInclude]
        public int Dodge
        {
            get;
            set;
        } = 1;
        [JsonInclude]
        public List<DamageType> Resistances
        {
            get;
            set;
        } = new List<DamageType>();
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