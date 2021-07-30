using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BotScaffold
{
    public class ClientDetails
    {
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
        /// Defines the interval, in milliseconds, between automatic saving of config files.
        /// </summary>
        [JsonInclude]
        public int AutoSaveInterval
        {
            get;
            private set;
        }

        /// <summary>
        /// Constructs a new config object.
        /// </summary>
        /// <param name="id">The bot client ID.</param>
        /// <param name="token">The token for interacting with the Discord API.</param>
        /// <param name="autoSaveInterval">The number of milliseconds between automatic saves.</param>
        public ClientDetails(ulong id, string token, int autoSaveInterval = 600000)
        {
            ID = id;
            Token = token;
            AutoSaveInterval = autoSaveInterval;
        }

        /// <summary>
        /// Saves the client details to a file.
        /// </summary>
        /// <param name="fileName">The name of the file to save to.</param>
        public void Save(string fileName)
        {
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string json = JsonSerializer.Serialize<ClientDetails>(this, options);
            File.WriteAllText(fileName, json);
        }

        /// <summary>
        /// Loads client details from the specified file.
        /// </summary>
        /// <param name="fileName">The name of the file to load client details from.</param>
        /// <returns>The config object located at the file location.</returns>
        public static ClientDetails Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                JsonSerializerOptions options = new JsonSerializerOptions()
                {
                    WriteIndented = true
                };
                string json = File.ReadAllText(fileName);
                return JsonSerializer.Deserialize<ClientDetails>(json, options);
            }
            else
            {
                return null;
            }
        }
    }
}