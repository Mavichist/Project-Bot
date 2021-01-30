using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using BotScaffold;

namespace HangoutBot
{
    /// <summary>
    /// A bot for managing hangouts, in the form of hidden role-accessed channels.
    /// </summary>
    public class HangoutManagerBot : BotInstance.Bot<HangoutBotConfig>
    {
        /// <summary>
        /// Creates a new instance of the hangout bot.
        /// </summary>
        /// <param name="details">The client details to construct the bot with.</param>
        public HangoutManagerBot(string name) : base(name)
        {

        }

        /// <summary>
        /// Defines the functionality for adding a hangout to this server's list.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [Usage("This command creates a hangout channel with the specified name. Member roles are created at the same time.")]
        [Argument("Hangout Name", "The name of the hangout channel to create.")]
        [CommandAttribute("create hangout", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"")]
        private async Task CreateHangout(CommandArgs<HangoutBotConfig> args)
        {
            string name = args["name"];

            // Create the server hangout category, or retrieve them if they exist already.
            DiscordChannel category;
            if (args.Config.HangoutCategoryID == 0)
            {
                category = await args.Guild.CreateChannelCategoryAsync("Hangouts");
                args.Config.HangoutCategoryID = category.Id;
            }
            else
            {
                category = args.Guild.GetChannel(args.Config.HangoutCategoryID);
            }

            // Create the hangout roles and channel if they don't exist, then set up permissions.
            if (!args.Config.Hangouts.ContainsKey(name))
            {
                var memberRole = await args.Guild.CreateRoleAsync($"{name} Hangout Member");
                var channel = await args.Guild.CreateChannelAsync($"{name} Chat", ChannelType.Text, category);

                // We need to set up the channel so that everyone else can't see it, but people with
                // the relevant roles can. We also need to make the bot itself a member of the channel,
                // otherwise any attempts to edit the channel will fail (authentication error). This
                // happens because the bot technically can't "see" the channel after we set it invisible.
                await channel.AddOverwriteAsync(await args.Guild.GetMemberAsync(Instance.Details.ID), Permissions.AccessChannels);
                await channel.AddOverwriteAsync(args.Guild.EveryoneRole, Permissions.None, Permissions.AccessChannels);
                await channel.AddOverwriteAsync(memberRole, Permissions.AccessChannels);

                args.Config.Hangouts.Add(name, new Hangout(name, channel.Id, memberRole.Id));

                await args.Channel.SendMessageAsync($"Added \"{name}\" to this server.");
            }
            else
            {
                await args.Channel.SendMessageAsync($"A hangout named \"{name}\" already exists on this server.");
            }
        }
        /// <summary>
        /// Defines functionality for removing a hangout from this server's list.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [Usage("This command removes the specified hangout and all of its associated roles.")]
        [Argument("Hangout Name", "The name of the hangout channel to remove.")]
        [CommandAttribute("remove hangout", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<name>[a-zA-Z0-9\\s]+)\"")]
        private async Task RemoveHangout(CommandArgs<HangoutBotConfig> args)
        {
            string name = args["name"];

            // If the project exists, retrieve and delete the associated roles and channels.
            if (args.Config.Hangouts.TryGetValue(name, out Hangout hangout))
            {
                var memberRole = args.Guild.GetRole(hangout.MemberRoleID);
                var channel = args.Guild.GetChannel(hangout.ChannelID);

                await memberRole.DeleteAsync();
                await channel.DeleteAsync();

                args.Config.Hangouts.Remove(name);
                await args.Channel.SendMessageAsync($"Removed the **{name}** hangout from this server.");
            }
            else
            {
                await args.Channel.SendMessageAsync($"No hangout titled **{name}** exists on this server.");
            }
        }
        /// <summary>
        /// Defines a command for manually setting the hangout category channel.
        /// </summary>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [Usage("This command sets the category that hangout channels will be organized under. Existing hangouts will be pulled into this category on use.")]
        [Argument("Channel", "Mention the category you would like to designate as the hangout category.")]
        [CommandAttribute("set hangout category", CommandLevel = CommandLevel.Admin, ParameterRegex = "<#(?<channelID>\\d+)>")]
        private async Task SetHangoutCategory(CommandArgs<HangoutBotConfig> args)
        {
            ulong categoryID = ulong.Parse(args["channelID"]);
            DiscordChannel category = args.Guild.GetChannel(categoryID);
            if (category != null)
            {
                if (category.IsCategory)
                {
                    args.Config.HangoutCategoryID = categoryID;
                    await args.Channel.SendMessageAsync($"**{category.Name}** is now the hangout category.");

                    foreach (var hangout in args.Config.Hangouts)
                    {
                        DiscordChannel channel = args.Guild.GetChannel(hangout.Value.ChannelID);
                        await channel.ModifyAsync((e) =>
                        {
                            e.Parent = category;
                        });
                    }
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
        protected override HangoutBotConfig CreateDefaultConfig()
        {
            return new HangoutBotConfig('!', 0);
        }
    }
}