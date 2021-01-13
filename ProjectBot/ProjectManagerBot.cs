using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using BotScaffold;

namespace ProjectBot
{
    /// <summary>
    /// A bot for managing projects, in the form of hidden role-accessed channels.
    /// </summary>
    public class ProjectManagerBot : Bot
    {
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
        /// <param name="id">The client ID.</param>
        /// <param name="token">The client token.</param>
        public ProjectManagerBot(ulong id, string token, char indicator) : base(id, token, indicator)
        {

        }
        /// <summary>
        /// Creates a new instance of the project bot.
        /// </summary>
        /// <param name="details">The client details to construct the bot with.</param>
        public ProjectManagerBot(ClientDetails details) : base(details)
        {

        }

        [CommandAttribute("die")]
        private async Task Die(Match match, MessageCreateEventArgs args)
        {
            await args.Channel.SendMessageAsync("Goodbye daddy.");
            Stop();
        }
        [CommandAttribute("project add", ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"")]
        private async Task AddProject(Match match, MessageCreateEventArgs args)
        {
            string name = match.Groups["name"].Value;
            await args.Channel.SendMessageAsync($"Adding {name} to project list...");

            ProjectServer server;
            DiscordChannel category = null;
            if (!ProjectServers.ContainsKey(args.Guild.Id))
            {
                foreach (DiscordChannel c in await args.Guild.GetChannelsAsync())
                {
                    if (c.Name.ToLower() == "projects")
                    {
                        category = c;
                        break;
                    }
                }
                if (category is null)
                {
                    category = await args.Guild.CreateChannelCategoryAsync("Projects");
                }
                ProjectServers.Add(args.Guild.Id, server = new ProjectServer(args.Guild.Id, category.Id));
            }
            else 
            {
                server = ProjectServers[args.Guild.Id];
                category = args.Guild.GetChannel(server.ProjectCategoryID);
            }

            if (!server.Projects.ContainsKey(name))
            {
                await args.Guild.GetChannelsAsync();
                var curatorRole = await args.Guild.CreateRoleAsync($"{name} Curator");
                var memberRole = await args.Guild.CreateRoleAsync($"{name} Member");
                var channel = await args.Guild.CreateChannelAsync($"{name} Chat", ChannelType.Text, category);

                await channel.AddOverwriteAsync(args.Guild.EveryoneRole, Permissions.None, Permissions.AccessChannels);
                await channel.AddOverwriteAsync(curatorRole, Permissions.AccessChannels);
                await channel.AddOverwriteAsync(memberRole, Permissions.AccessChannels);

                server.Projects.Add(name, new Project(name, channel.Id, curatorRole.Id, memberRole.Id));
                await args.Channel.SendMessageAsync($"Added \"{name}\" to this server.");
            }
            else await args.Channel.SendMessageAsync($"A project named \"{name}\" already exists on this server.");
        }
        [CommandAttribute("project remove", ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"")]
        private async Task RemoveProject(Match match, MessageCreateEventArgs args)
        {
            string name = match.Groups["name"].Value;
            await args.Channel.SendMessageAsync($"Removing {name} from project list...");

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
                else await args.Channel.SendMessageAsync($"No project titled {name} exists on this server.");
            }
            else await args.Channel.SendMessageAsync("No projects exist for this server.");
        }
        [CommandAttribute("project list")]
        private async Task ListProjects(Match match, MessageCreateEventArgs args)
        {
            if (ProjectServers.TryGetValue(args.Guild.Id, out ProjectServer server))
            {
                StringBuilder list = new StringBuilder();
                list.Append("```\n");
                foreach (var project in server.Projects.Values)
                {
                    list.Append($"{project.Name}\n");
                }
                list.Append("```");
                await args.Channel.SendMessageAsync(list.ToString());
            }
            else await args.Channel.SendMessageAsync("No projects exist for this server.");
        }

        public override void OnStartup()
        {
            foreach (var serv in ProjectServer.LoadServers())
            {
                ProjectServers.Add(serv.ServerID, serv);
            }
        }
        public override void OnShutdown()
        {
            foreach (var serv in ProjectServers)
            {
                serv.Value.Save();
            }
        }
    }
}