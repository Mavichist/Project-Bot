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
            private set;
        }
        [JsonInclude]
        public string Token
        {
            get;
            private set;
        }
        [JsonInclude]
        public char Indicator
        {
            get;
            private set;
        }

        public ClientDetails(ulong id, string token, char indicator)
        {
            ID = id;
            Token = token;
            Indicator = indicator;
        }

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