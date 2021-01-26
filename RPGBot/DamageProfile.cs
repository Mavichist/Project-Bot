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

    /// <summary>
    /// Represents the damage done by a weapon and peripheral information like attack range.
    /// </summary>
    public class DamageProfile
    {
        private static Random random = new Random();

        /// <summary>
        /// A description of the weapon.
        /// </summary>
        [JsonInclude]
        public string Description
        {
            get;
            private set;
        } = "Fists";
        /// <summary>
        /// The total magnitude of this weapon's offences.
        /// </summary>
        [JsonInclude]
        public int Magnitude
        {
            get;
            set;
        } = 3;
        /// <summary>
        /// Indicates the point spread of the weapon's attack damage.
        /// Higher values indicate an unpredictable weapon.
        /// </summary>
        [JsonInclude]
        public int Spread
        {
            get;
            set;
        } = 2;
        /// <summary>
        /// Represents the likelihood of a critical strike.
        /// </summary>
        [JsonInclude]
        public int CriticalStrike
        {
            get;
            set;
        } = 1;
        /// <summary>
        /// Represents the likelihood that attacks with this weapon will hit.
        /// </summary>
        [JsonInclude]
        public int Accuracy
        {
            get;
            set;
        } = 4;
        /// <summary>
        /// The primary damage type dealt by this weapon.
        /// </summary>
        [JsonInclude]
        public DamageType PrimaryType
        {
            get;
            set;
        } = DamageType.Bludgeoning;
        /// <summary>
        /// The secondary damage type dealt by this weapon.
        /// </summary>
        [JsonInclude]
        public DamageType SecondaryType
        {
            get;
            set;
        } = DamageType.None;
        /// <summary>
        /// The stamina cost for using this weapon.
        /// </summary>
        [JsonInclude]
        public int StaminaCost
        {
            get;
            set;
        } = 5;
        /// <summary>
        /// The mana cost for using this weapon.
        /// </summary>
        [JsonInclude]
        public int ManaCost
        {
            get;
            set;
        } = 0;
        /// <summary>
        /// The maximum number of posts a target can be from the attack command message.
        /// Attacks are considered to occur within a channel.
        /// </summary>
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
        
        /// <summary>
        /// Calculates the resultant damage for an attack using a damage and armor profile.
        /// </summary>
        /// <param name="damage">The attack profile.</param>
        /// <param name="armor">The defence profile.</param>
        /// <returns>The result of the attack.</returns>
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

        /// <summary>
        /// Represents the outcome of a single attack.
        /// </summary>
        public struct Result
        {
            /// <summary>
            /// The raw damage to health dealt by the attack.
            /// </summary>
            public int Damage
            {
                get;
                set;
            }
            /// <summary>
            /// Indicates whether the attack was a critical hit.
            /// </summary>
            public bool CriticalHit
            {
                get;
                set;
            }
            /// <summary>
            /// Indicates whether the primary stat was resisted by the target's armor.
            /// </summary>
            public bool PrimaryResisted
            {
                get;
                set;
            }
            /// <summary>
            /// Indicates whether the secondary stat was resisted by the target's armor.
            /// </summary>
            public bool SecondaryResisted
            {
                get;
                set;
            }
            /// <summary>
            /// Indicates whether the armor was vulnerable to the attack's primary type.
            /// </summary>
            public bool PrimaryVulnerable
            {
                get;
                set;
            }
            /// <summary>
            /// Indicates whether the armor was vulnerable to the attack's secondary type.
            /// </summary>
            public bool SecondaryVulnerable
            {
                get;
                set;
            }
            /// <summary>
            /// Indicates whether the attack missed.
            /// </summary>
            public bool Miss
            {
                get;
                set;
            }
        }
    }
}