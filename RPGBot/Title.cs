using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RPGBot
{
    /// <summary>
    /// An award represents some kind of accomplishment on a Discord server.z
    /// Said accomplishments are based on user reactions, attempting to approximate a user's
    /// overall contribution using points allocated to different emojis.
    /// </summary>
    public class Title
    {
        /// <summary>
        /// A brief description for this award.
        /// </summary>
        [JsonInclude]
        public string Description
        {
            get;
            set;
        }
        /// <summary>
        /// A set of emojis and the number required for this award to be given to a user.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, int> EmojiRequirements
        {
            get;
            private set;
        } = new Dictionary<string, int>();

        /// <summary>
        /// Creates a new instance of an award using the specified description.
        /// </summary>
        /// <param name="description">The award description.</param>
        public Title(string description)
        {
            Description = description;
        }
    }
}