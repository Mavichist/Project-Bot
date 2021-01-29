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
        /// <summary>
        /// This player's title, such as 'Adventurer' or 'Mother of Dragons'.
        /// </summary>
        [JsonInclude]
        public string Title
        {
            get;
            set;
        } = "Adventurer";
        /// <summary>
        /// The personal resources of the player, such as mana, health and stamina.
        /// </summary>
        [JsonInclude]
        public ResourceProfile Resources
        {
            get;
            private set;
        } = new ResourceProfile();
        /// <summary>
        /// Indicates whether the player is alive (their health is above 0).
        /// </summary>
        [JsonIgnore]
        public bool IsAlive
        {
            get
            {
                return Resources.Health > 0;
            }
        }
        /// <summary>
        /// Indicates whether the player can attack (they are alive and have the mana/stamina to do so).
        /// </summary>
        [JsonIgnore]
        public bool CanAttack
        {
            get
            {
                return IsAlive &&
                    Resources.Mana >= Weapon.ManaCost && 
                    Resources.Stamina >= Weapon.StaminaCost;
            }
        }
        /// <summary>
        /// The inventory index of this player's weapon.
        /// If the item at this position in the inventory isn't a weapon, the player will use fists.
        /// </summary>
        [JsonInclude]
        public int WeaponIndex
        {
            get;
            set;
        }
        /// <summary>
        /// The damage dealt by this player's weapon.
        /// </summary>
        [JsonIgnore]
        public DamageProfile Weapon
        {
            get
            {
                return GetItem(WeaponIndex)?.Damage ?? DamageProfile.Fists;
            }
        }
        /// <summary>
        /// The inventory index of this player's armor.
        /// If the item at this position in the inventory isn't armor, the player will use rags.
        /// </summary>
        [JsonInclude]
        public int ArmorIndex
        {
            get;
            set;
        }
        /// <summary>
        /// The defences of the armor this player is wearing.
        /// </summary>
        [JsonIgnore]
        public ArmorProfile Armor
        {
            get
            {
                return GetItem(ArmorIndex)?.Armor ?? ArmorProfile.Rags;
            }
        }
        /// <summary>
        /// Contains a list of all the player's items.
        /// </summary>
        [JsonInclude]
        public Item?[] Inventory
        {
            get;
            private set;
        } = new Item?[8];

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
            }
            else
            {
                newAmount = Math.Max(amount, 0);
            }

            if (newAmount == 0)
            {
                Currency.Remove(emojiName);
            }
            else
            {
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
        /// <summary>
        /// Retrieves an item from the user's inventory at the specified location.
        /// If the index is out of bounds, null is returned.
        /// </summary>
        /// <param name="inventoryIndex">The index within the player's inventory.</param>
        /// <returns>The item at the specified location in the inventory.</returns>
        public Item? GetItem(int inventoryIndex)
        {
            if (inventoryIndex >= 0 && inventoryIndex < Inventory.Length)
            {
                return Inventory[inventoryIndex];
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Resizes the player's inventory and returns any items that couldn't fit in the new one.
        /// </summary>
        /// <param name="newSize">The size of the player's new inventory.</param>
        /// <returns>A collection of items that were removed due to space restrictions.</returns>
        public IEnumerable<Item> ResizeInventory(int newSize)
        {
            // Create a stack and fill it with items in the current inventory.
            Stack<Item> overflow = new Stack<Item>();
            for (int i = 0; i < Inventory.Length; i++)
            {
                if (Inventory[i].HasValue)
                {
                    overflow.Push(Inventory[i].Value);
                }
            }
            // Create the new inventory and copy over all the items in the stack.
            Inventory = new Item?[newSize];
            for (int i = 0; i < newSize && overflow.Count > 0; i++)
            {
                Inventory[i] = overflow.Pop();
            }
            // Yield every item left in the stack as a collection.
            foreach (Item i in overflow)
            {
                yield return i;
            }
        }
    }
}