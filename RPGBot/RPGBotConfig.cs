using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace RPGBot
{
    /// <summary>
    /// Represents a configuration for the AwardManagerBot.
    /// </summary>
    public class RPGBotConfig : BotConfig
    {
        /// <summary>
        /// A set of user statistics for all members of the current server.
        /// </summary>
        /// <value></value>
        [JsonInclude]
        public Dictionary<ulong, Player> Players
        {
            get;
            private set;
        } = new Dictionary<ulong, Player>();
        /// <summary>
        /// A set of available weapons in the game.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, DamageProfile> Weapons
        {
            get;
            private set;
        } = new Dictionary<string, DamageProfile>();
        /// <summary>
        /// A set of available armors.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, ArmorProfile> Armors
        {
            get;
            private set;
        } = new Dictionary<string, ArmorProfile>();

        /// <summary>
        /// Instantiates a new instance of an award bot config.
        /// </summary>
        /// <param name="indicator">The character each command must start with.</param>
        /// <returns></returns>
        public RPGBotConfig(char indicator) : base(indicator)
        {

        }

        /// <summary>
        /// Retrieves the statistics object for a user, creating one if one does not already exist.
        /// </summary>
        /// <param name="userID">The Discord identifier for the user.</param>
        /// <returns>A statistics object for the user.</returns>
        public Player GetPlayer(ulong userID)
        {
            if (!Players.TryGetValue(userID, out Player player))
            {
                player = new Player();
                Players.Add(userID, player);
            }
            return player;
        }
    }
}