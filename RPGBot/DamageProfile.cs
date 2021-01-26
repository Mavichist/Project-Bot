using System;
using System.Text.Json.Serialization;

namespace RPGBot
{
    public enum DamageType
    {
        None,
        Slashing,
        Piercing,
        Bludgeoning,
        Cold,
        Poison,
        Acid,
        Psychic,
        Fire,
        Necrotic,
        Radiant,
        Force,
        Thunder,
        Lightning
    }

    public class DamageProfile
    {
        private static Random random = new Random();

        [JsonInclude]
        public string Description
        {
            get;
            private set;
        } = "Fists";
        [JsonInclude]
        public int Magnitude
        {
            get;
            set;
        } = 3;
        [JsonInclude]
        public int Spread
        {
            get;
            set;
        } = 2;
        [JsonInclude]
        public int CriticalStrike
        {
            get;
            set;
        } = 1;
        [JsonInclude]
        public int Accuracy
        {
            get;
            set;
        } = 4;
        [JsonInclude]
        public DamageType PrimaryType
        {
            get;
            set;
        } = DamageType.Bludgeoning;
        [JsonInclude]
        public DamageType SecondaryType
        {
            get;
            set;
        } = DamageType.None;
        [JsonInclude]
        public int StaminaCost
        {
            get;
            set;
        } = 5;
        [JsonInclude]
        public int ManaCost
        {
            get;
            set;
        } = 0;
        [JsonInclude]
        public int Range
        {
            get;
            set;
        } = 2;

        /// <summary>
        /// This is the preferred way of copying the stats of a damage profile to a player because
        /// it guarantees immutability even though the class itself is not immutable.
        /// </summary>
        /// <param name="other">The other damage profile to copy from.</param>
        public void CopyFrom(DamageProfile other)
        {
            Description = other.Description;
            Magnitude = other.Magnitude;
            Spread = other.Spread;
            CriticalStrike = other.CriticalStrike;
            Accuracy = other.Accuracy;
            PrimaryType = other.PrimaryType;
            SecondaryType = other.SecondaryType;
            StaminaCost = other.StaminaCost;
            ManaCost = other.ManaCost;
            Range = other.Range;
        }
        
        public static Result CalculateDamage(DamageProfile damage, ArmorProfile armor)
        {
            Result result = new Result();

            // If this check fails, the attack misses and the rest of the calculations are pointless.
            if (random.Next(damage.Accuracy + armor.Dodge) < damage.Accuracy)
            {
                float modifier = 1f;

                // Calculate resistances and vulnerability for the primary type.
                if (armor.Resistances.Contains(damage.PrimaryType))
                {
                    modifier -= 0.5f;
                    result.PrimaryResisted = true;
                }
                else if (armor.Vulnerabilities.Contains(damage.PrimaryType))
                {
                    modifier += 0.5f;
                    result.PrimaryVulnerable = true;
                }

                // Calculate the resistances and vulnerability for the secondary type.
                if (armor.Resistances.Contains(damage.SecondaryType))
                {
                    modifier -= 0.25f;
                    result.SecondaryResisted = true;
                }
                else if (armor.Vulnerabilities.Contains(damage.SecondaryType))
                {
                    modifier += 0.25f;
                    result.SecondaryVulnerable = true;
                }

                // Calculate the crit chance based on a ratio between strike and protection.
                if (random.Next(0, damage.CriticalStrike + armor.Protection) <= damage.CriticalStrike)
                {
                    result.CriticalHit = true;
                    modifier += 1f;
                }
                
                int damageMagnitude = damage.Magnitude + random.Next(-damage.Spread, damage.Spread + 1);
                damageMagnitude = (int)MathF.Round(damageMagnitude * modifier);

                int armorMagnitude = armor.Magnitude + random.Next(-armor.Spread, armor.Spread + 1);

                result.Damage = Math.Max(damageMagnitude - armorMagnitude, 0);
            }
            else
            {
                result.Miss = true;
            }

            return result;
        }
        public static Result operator +(DamageProfile damage, ArmorProfile armor)
        {
            return CalculateDamage(damage, armor);
        }

        public struct Result
        {
            public int Damage
            {
                get;
                set;
            }
            public bool CriticalHit
            {
                get;
                set;
            }
            public bool PrimaryResisted
            {
                get;
                set;
            }
            public bool SecondaryResisted
            {
                get;
                set;
            }
            public bool PrimaryVulnerable
            {
                get;
                set;
            }
            public bool SecondaryVulnerable
            {
                get;
                set;
            }
            public bool Miss
            {
                get;
                set;
            }
        }
    }
}