using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace HangoutBot
{
    /// <summary>
    /// A simple class for modelling a single Discord server and keeping track of its projects.
    /// </summary>
    public class HangoutBotConfig : BotConfig
    {
        /// <summary>
        /// A dictionary of projects identified by their names.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, Hangout> Hangouts
        {
            get;
            private set;
        } = new Dictionary<string, Hangout>();
        /// <summary>
        /// An identifying number specifying which channel group project channels belong to.
        /// </summary>
        [JsonInclude]
        public ulong HangoutCategoryID
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of a project server with the specified ID and project category
        /// channel ID.
        /// </summary>
        /// <param name="serverID">The identifying number for the Discord server.</param>
        /// <param name="hangoutCategoryID">The identifying number for the project channel category.</param>
        public HangoutBotConfig(char indicator, ulong hangoutCategoryID) : base(indicator)
        {
            HangoutCategoryID = hangoutCategoryID;
        }
    }
}