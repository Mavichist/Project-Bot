using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectBot
{
    /// <summary>
    /// A simple class for modelling a single Discord server and keeping track of its projects.
    /// </summary>
    public class ProjectServer
    {
        /// <summary>
        /// The identifying number for the Discord server.
        /// </summary>
        [JsonInclude]
        public ulong ServerID
        {
            get;
            private set;
        }
        /// <summary>
        /// A dictionary of projects identified by their names.
        /// </summary>
        [JsonInclude]
        public Dictionary<string, Project> Projects
        {
            get;
            private set;
        } = new Dictionary<string, Project>();
        /// <summary>
        /// An identifying number specifying which channel group project channels belong to.
        /// </summary>
        [JsonInclude]
        public ulong ProjectCategoryID
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new instance of a project server with the specified ID and project category
        /// channel ID.
        /// </summary>
        /// <param name="serverID">The identifying number for the Discord server.</param>
        /// <param name="projectCategoryID">The identifying number for the project channel category.</param>
        public ProjectServer(ulong serverID, ulong projectCategoryID)
        {
            ServerID = serverID;
            ProjectCategoryID = projectCategoryID;
        }
    
        //WIP
        public void Save()
        {
            if (!Directory.Exists("Servers/"))
            {
                Directory.CreateDirectory("Servers/");
            }

            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string data = JsonSerializer.Serialize(this, options);
            File.WriteAllText($"Servers/{ServerID}.json", data);
        }

        //WIP
        public static IEnumerable<ProjectServer> LoadServers()
        {
            if (Directory.Exists("Servers/"))
            {
                foreach (string fn in Directory.EnumerateFiles("Servers/"))
                {
                    string data = File.ReadAllText(fn);
                    var options = new JsonSerializerOptions()
                    {
                        WriteIndented = true
                    };
                    yield return JsonSerializer.Deserialize<ProjectServer>(data, options);
                }
            }
        }
    }
}