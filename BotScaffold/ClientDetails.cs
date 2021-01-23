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
        /// Contains a set of role IDs that are considered "admins" by the bot.
        /// </summary>
        [JsonInclude]
        public Dictionary<ulong, HashSet<ulong>> ServerAdminRoleIDs
        {
            get;
            set;
        } = new Dictionary<ulong, HashSet<ulong>>();

        /// <summary>
        /// Constructs a new config object.
        /// </summary>
        /// <param name="id">The bot client ID.</param>
        /// <param name="token">The token for interacting with the Discord API.</param>
        public ClientDetails(ulong id, string token)
        {
            ID = id;
            Token = token;
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
        /// Retrieves a set of server admin roles.
        /// Creates a new set if none are present.
        /// </summary>
        /// <param name="serverID">The server ID for the roles list.</param>
        /// <returns>A set of role IDs.</returns>
        public HashSet<ulong> GetAdminRoleIDs(ulong serverID)
        {
            if (!ServerAdminRoleIDs.TryGetValue(serverID, out HashSet<ulong> roleIDs))
            {
                roleIDs = new HashSet<ulong>();
                ServerAdminRoleIDs[serverID] = roleIDs;
            }
            return roleIDs;
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