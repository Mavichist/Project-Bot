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
            set;
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
            set;
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

        /// <summary>
        /// Saves the project server to the specified path as a Json file.
        /// </summary>
        /// <param name="filePath">The location of the saved file.</param>
        public void Save(string filePath)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            string data = JsonSerializer.Serialize(this, options);
            File.WriteAllText(filePath, data);
        }

        /// <summary>
        /// Loads a series of servers located in the target folder.
        /// </summary>
        /// <param name="directory">The folder to load server files from.</param>
        /// <returns>An enumerable collection of project server objects.</returns>
        public static IEnumerable<ProjectServer> LoadServers(string directory)
        {
            var options = new JsonSerializerOptions()
            {
                WriteIndented = true
            };
            foreach (string fn in Directory.EnumerateFiles(directory, "*.json"))
            {
                string data = File.ReadAllText(fn);
                yield return JsonSerializer.Deserialize<ProjectServer>(data, options);
            }
        }
    }
}