using System.Text.Json.Serialization;

namespace HangoutBot
{
    /// <summary>
    /// A simple class for modelling a project, its properties and its members.
    /// </summary>
    public class Hangout
    {
        /// <summary>
        /// The name of the project.
        /// </summary>
        [JsonInclude]
        public string Name
        {
            get;
            set;
        }
        /// <summary>
        /// Each project has a dedicated channel, identified by this number.
        /// </summary>
        [JsonInclude]
        public ulong ChannelID
        {
            get;
            set;
        }
        /// <summary>
        /// Each project has a curator role, identified by this number.
        /// </summary>
        [JsonInclude]
        public ulong MemberRoleID
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new hangout, storing information about names, channels and roles.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="channelID">The channel identifying number.</param>
        /// <param name="memberRoleID">The member role identifying number.</param>
        public Hangout(string name, ulong channelID, ulong memberRoleID)
        {
            Name = name;
            ChannelID = channelID;
            MemberRoleID = memberRoleID;
        }
    }
}