using System.Text.Json.Serialization;
using DSharpPlus.Entities;

namespace RPGBot
{
    public struct Item
    {
        /// <summary>
        /// The name of the item.
        /// </summary>
        [JsonInclude]
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// If the item has a damage profile then it can be equipped as a weapon.
        /// </summary>
        [JsonInclude]
        public DamageProfile? Damage
        {
            get;
            set;
        }
        /// <summary>
        /// If the item has an armor profile then it can be equipped as armor.
        /// </summary>
        [JsonInclude]
        public ArmorProfile? Armor
        {
            get;
            set;
        }
        /// <summary>
        /// The consumable properties of this item.
        /// </summary>
        /// <value></value>
        [JsonInclude]
        public ConsumableProfile? Consumable
        {
            get;
            set;
        }
    
        public DiscordEmbed CreateEmbed()
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
            builder.WithTitle($"{Name}");
            builder.WithDescription("*Item properties:*");

            if (Armor.HasValue)
            {
                builder.AddField("Armor Properties:", "-");
                builder.AddField("Description:", $"*...{Armor.Value.Description}...*");
                builder.AddField("🛡 Armor Magnitude", $"{Armor.Value.Magnitude}");
                builder.AddField("📈 Armor Spread", $"{Armor.Value.Spread}");
                builder.AddField("🧱 Protection", $"{Armor.Value.Protection}");
                builder.AddField("🤸 Dodge", $"{Armor.Value.Dodge}");
                builder.AddField("🛑 Resists", $"{Armor.Value.Resists}");
                builder.AddField("💔 Vulnerability", $"{Armor.Value.Vulnerability}");
            }

            if (Damage.HasValue)
            {
                builder.AddField("Weapon Properties:", "-");
                builder.AddField("Description:", $"*...{Damage.Value.Description}...*");
                builder.AddField("⚔ Damage Magnitude", $"{Damage.Value.Magnitude}");
                builder.AddField("📈 Damage Spread", $"{Damage.Value.Spread}");
                builder.AddField("🗡 Critical Strike", $"{Damage.Value.CriticalStrike}");
                builder.AddField("🎯 Accuracy", $"{Damage.Value.Accuracy}");
                builder.AddField("💥 Primary Type", $"{Damage.Value.PrimaryType}");
                builder.AddField("🔥 Secondary Type", $"{Damage.Value.SecondaryType}");
                builder.AddField("💖 Mana Cost", $"{Damage.Value.HealthCost}");
                builder.AddField("🌟 Mana Cost", $"{Damage.Value.ManaCost}");
                builder.AddField("🍖 Stamina Cost", $"{Damage.Value.StaminaCost}");
            }

            if (Consumable.HasValue)
            {
                builder.AddField("Consumable Properties:", "-");
                builder.AddField("Description:", $"*...{Consumable.Value.Description}...*");
                builder.AddField($"💖 Health Gained", $"{Consumable.Value.HealthGained}");
                builder.AddField($"🌟 Mana Gained", $"{Consumable.Value.ManaGained}");
                builder.AddField($"🍖 Stamina Gained", $"{Consumable.Value.StaminaGained}");
            }

            return builder.Build();
        }
    }
}