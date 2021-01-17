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
        public BotConfig(char indicator) : this()
        {
            Indicator = indicator;
        }

        /// <summary>
        /// Saves the config object to the specified file within the shared config folder.
        /// </summary>
        /// <param name="config">The config object to save.</param>
        /// <param name="botName">The name of bot whose config we want to save.</param>
        public static void Save<TConfig>(TConfig config, string botName, ulong guildID)
        {
            if (!Directory.Exists($"{CONFIG_FOLDER}/{botName}"))
            {
                Directory.CreateDirectory($"{CONFIG_FOLDER}/{botName}");
            }

            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize(config, options);
            File.WriteAllText($"{CONFIG_FOLDER}/{botName}/{guildID}.json", json);
        }
        /// <summary>
        /// Loads a config object for the specified bot and guild.
        /// </summary>
        /// <param name="botName">The name of the bot.</param>
        /// <param name="guildID">The ID for the guild.</param>
        /// <typeparam name="TConfig">The config object type.</typeparam>
        /// <returns>The config object for the bot and guild.</returns>
        public static TConfig Load<TConfig>(string botName, ulong guildID) where TConfig : BotConfig
        {
            if (File.Exists($"{CONFIG_FOLDER}/{botName}/{guildID}.json"))
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                string json = File.ReadAllText($"{CONFIG_FOLDER}/{botName}.json");
                return JsonSerializer.Deserialize<TConfig>(json, options);
            }
            else
            {
                return null;
            }
        }
        /// <summary>
        /// Loads a series of config objects for a specified bot.
        /// </summary>
        /// <param name="botName">The name of the bot to load config objects for.</param>
        /// <typeparam name="TConfig">The config object type.</typeparam>
        /// <returns>A dictionary containing all the server/guild configuration objects for that bot.</returns>
        public static Dictionary<ulong, TConfig> LoadAll<TConfig>(string botName) where TConfig : BotConfig
        {
            Dictionary<ulong, TConfig> serverConfig = new Dictionary<ulong, TConfig>();
            if (Directory.Exists($"{CONFIG_FOLDER}/{botName}"))
            {
                foreach (string fileName in Directory.EnumerateFiles($"{CONFIG_FOLDER}/{botName}", "*.json"))
                {
                    FileInfo fi = new FileInfo(fileName);
                    if (ulong.TryParse(Path.GetFileNameWithoutExtension(fileName), out ulong guildID))
                    {
                        string json = File.ReadAllText(fileName);
                        TConfig config = JsonSerializer.Deserialize<TConfig>(json);
                        serverConfig.Add(guildID, config);
                    }
                }
            }
            return serverConfig;
        }
    }
}