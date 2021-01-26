using System;
using System.Text.Json.Serialization;

namespace RPGBot
{
    /// <summary>
    /// Represents the personal resources of a player.
    /// </summary>
    public class ResourceProfile
    {
        public const int MAX_HEALTH_MINIMUM = 10;
        public const int MAX_MANA_MINIMUM = 10;
        public const int MAX_STAMINA_MINIMUM = 10;

        /// <summary>
        /// The hitpoints the player can endure before dying.
        /// </summary>
        [JsonInclude]
        public int Health
        {
            get;
            private set;
        }
        /// <summary>
        /// The maximum hitpoints this player can have.
        /// </summary>
        [JsonInclude]
        public int MaxHealth
        {
            get;
            private set;
        }
        /// <summary>
        /// The mana the player can expend.
        /// </summary>
        [JsonInclude]
        public int Mana
        {
            get;
            private set;
        }
        /// <summary>
        /// The maximum amount of mana this player can have.
        /// </summary>
        [JsonInclude]
        public int MaxMana
        {
            get;
            private set;
        }
        /// <summary>
        /// The stamina the player can expend.
        /// </summary>
        [JsonInclude]
        public int Stamina
        {
            get;
            private set;
        }
        /// <summary>
        /// The maximum amount of stamina this player can have.
        /// </summary>
        [JsonInclude]
        public int MaxStamina
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new resource profile using the health, mana and stamina values.
        /// </summary>
        /// <param name="maxHealth">The maximum amount of health the player can have.</param>
        /// <param name="maxMana">The maximum amount of mana the player can have.</param>
        /// <param name="maxStamina">The maximum amount of stamina the player can have.</param>
        public ResourceProfile(int maxHealth = 100, int maxMana = 100, int maxStamina = 100)
        {
            Health = MaxHealth = maxHealth;
            Mana = MaxMana = maxMana;
            Stamina = MaxStamina = maxStamina;
        }
    
        /// <summary>
        /// Alters the hitpoints of this profile and returns information about overflow.
        /// </summary>
        /// <param name="amount">The number of hitpoints to modify.</param>
        /// <returns>A number indicating overflow.
        /// Positive numbers indicate how many points above max hp the alteration would have been.
        /// Negative numbers indicate how many points below zero the alteration would have been.</returns>
        public int AlterHealth(int amount)
        {
            int newHP = Health + amount;
            if (newHP < 0)
            {
                Health = 0;
                return newHP;
            }
            else if (newHP > MaxHealth)
            {
                Health = MaxHealth;
                return newHP - MaxHealth;
            }
            else
            {
                Health = newHP;
                return 0;
            }
        }
        /// <summary>
        /// Alters the mana of this profile and returns information about overflow.
        /// </summary>
        /// <param name="amount">The amount of mana to change by.</param>
        /// <returns>A number indicating overflow.
        /// Positive numbers indicate how many points above max mana the alteration would have been.
        /// Negative numbers indicate how many points below zero the alteration would have been.</returns>
        public int AlterMana(int amount)
        {
            int newMana = Mana + amount;
            if (newMana < 0)
            {
                Mana = 0;
                return newMana;
            }
            else if (newMana > MaxMana)
            {
                Mana = MaxMana;
                return newMana - MaxMana;
            }
            else
            {
                Mana = newMana;
                return 0;
            }
        }
        /// <summary>
        /// Alters the stamina of this profile and returns information about overflow.
        /// </summary>
        /// <param name="amount">The amount of stamina to change by.</param>
        /// <returns>A number indicating overflow.
        /// Positive numbers indicate how many points above max stamina the alteration would have been.
        /// Negative numbers indicate how many points below zero the alteration would have been.</returns>
        public int AlterStamina(int amount)
        {
            int newStamina = Stamina + amount;
            if (newStamina < 0)
            {
                Stamina = 0;
                return newStamina;
            }
            else if (newStamina > MaxStamina)
            {
                Stamina = MaxStamina;
                return newStamina - MaxStamina;
            }
            else
            {
                Stamina = newStamina;
                return 0;
            }
        }
        /// <summary>
        /// Sets the maximum health for the profile.
        /// Maximum health cannot be modified below the minimum value.
        /// </summary>
        /// <param name="max">The new maximum.</param>
        /// <returns>A value describing the relative difference between current health and the new
        /// maximum.
        /// Negative values indicate no mana truncation.
        /// Positive values indicate the amount of mana that was truncated.</returns>
        public int SetMaxHealth(int max)
        {
            MaxHealth = Math.Max(max, MAX_HEALTH_MINIMUM);
            int overflow = Health - MaxHealth;
            Health = Math.Min(Health, MaxHealth);
            return overflow;
        }
        /// <summary>
        /// Sets the maximum mana for the profile.
        /// Maximum mana cannot be modified below the minimum value.
        /// </summary>
        /// <param name="max">The new maximum.</param>
        /// <returns>A value describing the relative difference between current mana and the new
        /// maximum.
        /// Negative values indicate no mana truncation.
        /// Positive values indicate the amount of mana that was truncated.</returns>
        public int SetMaxMana(int max)
        {
            MaxHealth = Math.Max(max, MAX_HEALTH_MINIMUM);
            int overflow = Health - MaxHealth;
            Health = Math.Min(Health, MaxHealth);
            return overflow;
        }
        /// <summary>
        /// Sets the maximum stamina for the profile.
        /// Maximum stamina cannot be modified below the minimum value.
        /// </summary>
        /// <param name="max">The new maximum.</param>
        /// <returns>A value describing the relative difference between current stamina and the new
        /// maximum.
        /// Negative values indicate no stamina truncation.
        /// Positive values indicate the amount of stamina that was truncated.</returns>
        public int SetMaxStamina(int max)
        {
            MaxStamina = Math.Max(max, MAX_STAMINA_MINIMUM);
            int overflow = Stamina - MaxStamina;
            Stamina = Math.Min(Stamina, MaxStamina);
            return overflow;
        }
    }
}