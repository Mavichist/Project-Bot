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
    public class ProjectManagerBot : Bot<ProjectBotConfig>
    {
        public static readonly string SAVE_DIRECTORY = "Project_Servers";

        /// <summary>
        /// Creates a new instance of the project bot.
        /// </summary>
        /// <param name="details">The client details to construct the bot with.</param>
        public ProjectManagerBot(string name) : base(name)
        {

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
            ProjectBotConfig config = GetConfig(args.Guild);

            string name = match.Groups["name"].Value;
            await args.Channel.SendMessageAsync($"Adding \"{name}\" to project list...");

            // Create the server and project category, or retrieve them if they exist already.
            DiscordChannel category;
            if (config.ProjectCategoryID == 0)
            {
                category = await args.Guild.CreateChannelCategoryAsync("Projects");
                config.ProjectCategoryID = category.Id;
            }
            else
            {
                category = args.Guild.GetChannel(config.ProjectCategoryID);
            }

            // Create the project roles and channel if they don't exist, then set up permissions.
            if (!config.Projects.ContainsKey(name))
            {
                var curatorRole = await args.Guild.CreateRoleAsync($"{name} Curator");
                var memberRole = await args.Guild.CreateRoleAsync($"{name} Member");
                var channel = await args.Guild.CreateChannelAsync($"{name} Chat", ChannelType.Text, category);

                // We need to set up the channel so that everyone else can't see it, but people with
                // the relevant roles can. We also need to make the bot itself a member of the channel,
                // otherwise any attempts to edit the channel will fail (authentication error). This
                // happens because the bot technically can't "see" the channel after we set it invisible.
                await channel.AddOverwriteAsync(await args.Guild.GetMemberAsync(Details.ID), Permissions.AccessChannels);
                await channel.AddOverwriteAsync(args.Guild.EveryoneRole, Permissions.None, Permissions.AccessChannels);
                await channel.AddOverwriteAsync(curatorRole, Permissions.AccessChannels);
                await channel.AddOverwriteAsync(memberRole, Permissions.AccessChannels);

                config.Projects.Add(name, new Project(name, channel.Id, curatorRole.Id, memberRole.Id));

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
            ProjectBotConfig config = GetConfig(args.Guild);

            string name = match.Groups["name"].Value;
            await args.Channel.SendMessageAsync($"Removing \"{name}\" from project list...");

            // If the project exists, retrieve and delete the associated roles and channels.
            if (config.Projects.TryGetValue(name, out Project project))
            {
                var curatorRole = args.Guild.GetRole(project.CuratorRoleID);
                var memberRole = args.Guild.GetRole(project.MemberRoleID);
                var channel = args.Guild.GetChannel(project.ChannelID);

                await curatorRole.DeleteAsync();
                await memberRole.DeleteAsync();
                await channel.DeleteAsync();

                config.Projects.Remove(name);
                await args.Channel.SendMessageAsync($"Removed \"{name}\" from this server.");
            }
            else
            {
                await args.Channel.SendMessageAsync($"No project titled \"{name}\" exists on this server.");
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
            ProjectBotConfig config = GetConfig(args.Guild);

            StringBuilder list = new StringBuilder();
            list.Append("```\nProjects:\n");
            foreach (var project in config.Projects.Values)
            {
                list.Append($"\t{project.Name}\n");
            }
            list.Append("```");
            await args.Channel.SendMessageAsync(list.ToString());
        }
        /// <summary>
        /// Defines a command for manually setting the project category channel.
        /// </summary>
        /// <param name="match">The regex match for command parameters.</param>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("project set category", CommandLevel = CommandLevel.Admin, ParameterRegex = "<#(?<channelID>\\d+)>")]
        private async Task SetProjectCategory(Match match, MessageCreateEventArgs args)
        {
            ProjectBotConfig config = GetConfig(args.Guild);

            ulong categoryID = ulong.Parse(match.Groups["channelID"].Value);
            DiscordChannel category = args.Guild.GetChannel(categoryID);
            if (category != null)
            {
                if (category.IsCategory)
                {
                    config.ProjectCategoryID = categoryID;
                    await args.Channel.SendMessageAsync($"**{category.Name}** is now the project category.");
                }
                else
                {
                    await args.Channel.SendMessageAsync($"The **{category.Name}** channel is not a category.");
                }
            }
            else
            {
                await args.Channel.SendMessageAsync("The category doesn't exist.");
            }
        }

        /// <summary>
        /// Creates a default config data structure for new servers.
        /// </summary>
        /// <returns>a config data structure.</returns>
        public override ProjectBotConfig CreateDefaultConfig()
        {
            return new ProjectBotConfig('!', 0);
        }
    }
}