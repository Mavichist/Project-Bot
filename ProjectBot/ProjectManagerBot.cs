using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using BotScaffold;
using System.IO;

namespace ProjectBot
{
    /// <summary>
    /// A bot for managing projects, in the form of hidden role-accessed channels.
    /// </summary>
    public class ProjectManagerBot : Bot<BotConfig>
    {
        public static readonly string SAVE_DIRECTORY = "Project_Servers";

        /// <summary>
        /// A dictionary of project servers.
        /// Each server has its own entry, so the bot can handle multiple servers.
        /// Each server is identified by an unsigned, 64-bit integer.
        /// </summary>
        public Dictionary<ulong, ProjectServer> ProjectServers
        {
            get;
            private set;
        } = new Dictionary<ulong, ProjectServer>();

        /// <summary>
        /// Creates a new instance of the project bot.
        /// </summary>
        /// <param name="details">The client details to construct the bot with.</param>
        public ProjectManagerBot(string name) : base(name)
        {

        }

        /// <summary>
        /// A simple parameterless command for shutting down the bot.
        /// </summary>
        /// <param name="match">The regex match for command parameters.</param>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("shutdown", CommandLevel = CommandLevel.Admin)]
        private async Task Shutdown(Match match, MessageCreateEventArgs args)
        {
            await args.Channel.SendMessageAsync("Shutting down...");
            Stop();
        }
        /// <summary>
        /// Defines the functionality for adding a project to this server's list.
        /// </summary>
        /// <param name="match">The regex match for command parameters.</param>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("project add", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"")]
        private async Task AddProject(Match match, MessageCreateEventArgs args)
        {
            string name = match.Groups["name"].Value;
            await args.Channel.SendMessageAsync($"Adding \"{name}\" to project list...");

            // Create the server and project category, or retrieve them if they exist already.
            ProjectServer server;
            DiscordChannel category;
            if (!ProjectServers.ContainsKey(args.Guild.Id))
            {
                category = await args.Guild.CreateChannelCategoryAsync("Projects");
                server = new ProjectServer(args.Guild.Id, category.Id);
                ProjectServers.Add(args.Guild.Id, server);
            }
            else
            {
                server = ProjectServers[args.Guild.Id];
                category = args.Guild.GetChannel(server.ProjectCategoryID);
            }

            // Create the project roles and channel if they don't exist, then set up permissions.
            if (!server.Projects.ContainsKey(name))
            {
                var curatorRole = await args.Guild.CreateRoleAsync($"{name} Curator");
                var memberRole = await args.Guild.CreateRoleAsync($"{name} Member");
                var channel = await args.Guild.CreateChannelAsync($"{name} Chat", ChannelType.Text, category);

                // We need to set up the channel so that everyone else can't see it, but people with
                // the relevant roles can. We also need to make the bot itself a member of the channel,
                // otherwise any attempts to edit the channel will fail (authentication error). This
                // happens because the bot technically can't "see" the channel after we set it invisible.
                await channel.AddOverwriteAsync(await args.Guild.GetMemberAsync(Config.ID), Permissions.AccessChannels);
                await channel.AddOverwriteAsync(args.Guild.EveryoneRole, Permissions.None, Permissions.AccessChannels);
                await channel.AddOverwriteAsync(curatorRole, Permissions.AccessChannels);
                await channel.AddOverwriteAsync(memberRole, Permissions.AccessChannels);

                server.Projects.Add(name, new Project(name, channel.Id, curatorRole.Id, memberRole.Id));

                await args.Channel.SendMessageAsync($"Added \"{name}\" to this server.");
            }
            else
            {
                await args.Channel.SendMessageAsync($"A project named \"{name}\" already exists on this server.");
            }
        }
        /// <summary>
        /// Defines functionality for removing a project from this server's list.
        /// </summary>
        /// <param name="match">The regex match for command parameters.</param>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("project remove", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"")]
        private async Task RemoveProject(Match match, MessageCreateEventArgs args)
        {
            string name = match.Groups["name"].Value;
            await args.Channel.SendMessageAsync($"Removing \"{name}\" from project list...");

            // If the project exists, retrieve and delete the associated roles and channels.
            if (ProjectServers.TryGetValue(args.Guild.Id, out ProjectServer server))
            {
                if (server.Projects.TryGetValue(name, out Project project))
                {
                    var curatorRole = args.Guild.GetRole(project.CuratorRoleID);
                    var memberRole = args.Guild.GetRole(project.MemberRoleID);
                    var channel = args.Guild.GetChannel(project.ChannelID);

                    await curatorRole.DeleteAsync();
                    await memberRole.DeleteAsync();
                    await channel.DeleteAsync();

                    server.Projects.Remove(name);
                    await args.Channel.SendMessageAsync($"Removed \"{name}\" from this server.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"No project titled \"{name}\" exists on this server.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("No projects exist for this server.");
            }
        }
        /// <summary>
        /// Defines a command for listing existing projects on this server.
        /// </summary>
        /// <param name="match">The regex match for command parameters.</param>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("project list", CommandLevel = CommandLevel.Unrestricted)]
        private async Task ListProjects(Match match, MessageCreateEventArgs args)
        {
            if (ProjectServers.TryGetValue(args.Guild.Id, out ProjectServer server))
            {
                StringBuilder list = new StringBuilder();
                list.Append("```\nProjects:\n");
                foreach (var project in server.Projects.Values)
                {
                    list.Append($"\t{project.Name}\n");
                }
                list.Append("```");
                await args.Channel.SendMessageAsync(list.ToString());
            }
            else
            {
                await args.Channel.SendMessageAsync("No projects exist for this server.");
            }
        }

        /// <summary>
        /// Occurs when the bot starts.
        /// </summary>
        protected override void OnStartup()
        {
            // If the save directory exists, we can load all of the servers present in it and add
            // them to our dictionary of known project servers.
            if (Directory.Exists(SAVE_DIRECTORY))
            {
                foreach (var serv in ProjectServer.LoadServers(SAVE_DIRECTORY))
                {
                    ProjectServers.Add(serv.ServerID, serv);
                }
            }
        }
        /// <summary>
        /// Occurs when the bot is shut down.
        /// </summary>
        protected override void OnShutdown()
        {
            // If the save directory exists then we need to create it.
            if (!Directory.Exists(SAVE_DIRECTORY))
            {
                Directory.CreateDirectory(SAVE_DIRECTORY);
            }
            // We then save every server to that directory using the server ID as a file name.
            foreach (var serv in ProjectServers.Values)
            {
                serv.Save($"{SAVE_DIRECTORY}/{serv.ServerID}.json");
            }
        }
    }
}