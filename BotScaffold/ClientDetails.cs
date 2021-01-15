using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BotScaffold
{
    public class ClientDetails
    {
        [JsonInclude]
        public ulong ID
        {
            get;
            set;
        }
        [JsonInclude]
        public string Token
        {
            get;
            set;
        }
        [JsonInclude]
        public char Indicator
        {
            get;
            set;
        }
        [JsonInclude]
        public List<ulong> AdminRoleIDs
        {
            get;
            set;
        } = new List<ulong>();

        public static ClientDetails LoadFrom(string fileName)
        {
            string json = File.ReadAllText(fileName);
            JsonSerializerOptions options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            return JsonSerializer.Deserialize<ClientDetails>(json, options);
        }
    }
}