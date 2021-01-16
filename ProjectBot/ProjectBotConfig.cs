using System.Collections.Generic;
using System.Text.Json.Serialization;
using BotScaffold;

namespace ProjectBot
{
    /// <summary>
    /// A simple class for modelling a single Discord server and keeping track of its projects.
    /// </summary>
    public class ProjectBotConfig : BotConfig
    {
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
        public ProjectBotConfig(char indicator, ulong projectCategoryID) : base(indicator)
        {
            ProjectCategoryID = projectCategoryID;
        }
    }
}