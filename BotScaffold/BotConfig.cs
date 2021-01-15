using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BotScaffold
{
    public class BotConfig
    {
        public static readonly string CONFIG_FOLDER = "Config";
        
        /// <summary>
        /// The identifying number for this client on Discord.
        /// </summary>
        [JsonInclude]
        public ulong ID
        {
            get;
            private set;
        }
        /// <summary>
        /// The token for this client on Discord.
        /// </summary>
        [JsonInclude]
        public string Token
        {
            get;
            private set;
        }
        /// <summary>
        /// All commands processed by this bot must begin with this character.
        /// </summary>
        [JsonInclude]
        public char Indicator
        {
            get;
            private set;
        }
        /// <summary>
        /// Contains a list of role IDs that are considered "admins" by the bot.
        /// </summary>
        [JsonInclude]
        public List<ulong> AdminRoleIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Constructs an empty bot config object.
        /// </summary>
        public BotConfig()
        {
            AdminRoleIDs = new List<ulong>();
        }
        /// <summary>
        /// Constructs a new config object.
        /// </summary>
        /// <param name="id">The bot client ID.</param>
        /// <param name="token">The token for interacting with the Discord API.</param>
        /// <param name="indicator">The character that indicates the start of a command.</param>
        public BotConfig(ulong id, string token, char indicator) : this()
        {
            ID = id;
            Token = token;
            Indicator = indicator;
        }

        /// <summary>
        /// Saves the config object to the specified file within the shared config folder.
        /// </summary>
        /// <param name="fileName">The name of the json file within the config folder.</param>
        public void Save(string fileName)
        {
            if (!Directory.Exists(CONFIG_FOLDER))
            {
                Directory.CreateDirectory(CONFIG_FOLDER);
            }

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText($"{CONFIG_FOLDER}/{fileName}.json", json);
        }
    
        /// <summary>
        /// Loads a config object from the specified file within the 
        /// </summary>
        /// <param name="fileName">The name of the json file within the config folder.</param>
        /// <typeparam name="TConfig">The type of the config object.</typeparam>
        /// <returns>The config object located at the file location.</returns>
        public static TConfig Load<TConfig>(string fileName) where TConfig : BotConfig
        {
            if (File.Exists($"{CONFIG_FOLDER}/{fileName}.json"))
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                string json = File.ReadAllText($"{CONFIG_FOLDER}/{fileName}.json");
                return JsonSerializer.Deserialize<TConfig>(json, options);
            }
            else
            {
                return null;
            }
        }
    }
}