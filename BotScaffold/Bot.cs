using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BotScaffold
{
    /// <summary>
    /// A single instance of a discord bot, with an ID and a token.
    /// </summary>
    public abstract class Bot<TConfig> where TConfig : BotConfig
    { 
        public string Name
        {
            get;
            private set;
        }
        /// <summary>
        /// Contains configuration information for this bot.
        /// </summary>
        private Dictionary<ulong, TConfig> ServerConfigs
        {
            get;
            set;
        }
        /// <summary>
        /// A cancellation token for stopping the client worker thread.
        /// </summary>
        private CancellationTokenSource CancellationSource
        {
            get;
            set;
        }
        /// <summary>
        /// The client object through which the bot communicates with the Discord API.
        /// </summary>
        protected DiscordClient Client
        {
            get;
            set;
        }
        /// <summary>
        /// A list of commands, sorted from most specific to least specific.
        /// </summary>
        private List<Command<TConfig>> Commands
        {
            get;
            set;
        }
        /// <summary>
        /// The client ID and token for interacting with the Discord API.
        /// </summary>
        public ClientDetails Details
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new instance of a Discord bot, with the specified details.
        /// </summary>
        /// <param name="details">The client details object containing the bot user information.</param>
        public Bot(string name)
        {
            Name = name;
            Commands = Command<TConfig>.GetCommands<TConfig>(this);
        }

        /// <summary>
        /// Loads the config file associated with this bot.
        /// </summary>
        private void LoadConfig()
        {
            ServerConfigs = BotConfig.LoadAll<TConfig>(Name);
        }
        /// <summary>
        /// Saves the bot's config data structure to the config folder.
        /// </summary>
        private void SaveConfig()
        {
            foreach (var serverConfig in ServerConfigs)
            {
                BotConfig.Save(serverConfig.Value, Name, serverConfig.Key);
            }
        }
        /// <summary>
        /// Retrieves the configuration for a specific guild.
        /// </summary>
        /// <param name="guild"></param>
        /// <returns>The configuration data structure for this guild.</returns>
        private TConfig GetConfig(DiscordGuild guild)
        {
            if (!ServerConfigs.TryGetValue(guild.Id, out TConfig config))
            {
                config = CreateDefaultConfig();
                ServerConfigs.Add(guild.Id, config);
            }
            return config;
        }
        
        /// <summary>
        /// Determines the level of commands the user is capable of executing.
        /// </summary>
        /// <param name="guild">The guild the command was used in.</param>
        /// <param name="userID">The ID of the user attempting a command.</param>
        /// <returns>The command level the user can access.</returns>
        private async Task<CommandLevel> GetCommandLevelAsync(DiscordGuild guild, ulong userID)
        {
            TConfig config = GetConfig(guild);
            DiscordMember member = await guild.GetMemberAsync(userID);
            
            // If the member is the owner of the server, they can execute any command.
            if (member.IsOwner)
            {
                return CommandLevel.Owner;
            }
            else
            {
                // We need to find an admin role in the member's role list for them to be considered
                // an admin.
                foreach (var role in member.Roles)
                {
                    if (config.AdminRoleIDs.Contains(role.Id))
                    {
                        return CommandLevel.Admin;
                    }
                }
                // In all other cases, the user is not an admin and not the server owner, so they
                // only have base-level access.
                return CommandLevel.Unrestricted;
            }
        }
        /// <summary>
        /// When a message is created on a server the bot is part of, the bot identifies commands
        /// intended for it by using the indicator character. It then goes through each of its known
        /// commands and attempts to execute them.
        /// The first command to succeed is the only one that runs, which is why sorting the command
        /// list in order of specificity is important.
        /// </summary>
        /// <param name="client">The discord client sending the message.</param>
        /// <param name="args">An object describing the context of the message sent.</param>
        /// <returns>A task to handle processing of the command.</returns>
        private async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs args)
        {
            TConfig config = GetConfig(args.Guild);
            
            // Check to see if the post starts with our indicator and the author wasn't a bot.
            if (args.Message.Content.StartsWith(config.Indicator) && !args.Author.IsBot)
            {
                // Get the command level so we can filter out commands this user can't access.
                CommandLevel level = await GetCommandLevelAsync(args.Guild, args.Author.Id);
                bool commandFound = false;
                foreach (Command<TConfig> c in Commands)
                {
                    // Only if the command is lower or equal to the user level do we attempt to run it.
                    if (c.CommandLevel <= level)
                    {
                        // If the regex matches and the command succeeds, we can skip the rest.
                        if (await c.AttemptAsync(args, config))
                        {
                            commandFound = true;
                            break;
                        }
                    }
                }
                if (!commandFound)
                {
                    await args.Channel.SendMessageAsync("Invalid command/auth.");
                }
            }
        }
        /// <summary>
        /// When an emoji is added to a post the bot will identify whether the relevant message is
        /// one of its own, then react accordingly by firing applicable reaction methods.
        /// </summary>
        /// <param name="client">The discord client sending the message.</param>
        /// <param name="args">An object describing the context of the message sent.</param>
        /// <returns>A task to handle processing of the command.</returns>
        private async Task OnReactionAdded(DiscordClient client, MessageReactionAddEventArgs args)
        {
            if (args.Message.Author.Id == Details.ID)
            {
                TConfig config = GetConfig(args.Guild);

                await ReactionAdded(new ReactionAddArgs<TConfig>(args, config));
            }
        }
        /// <summary>
        /// When an emoji is removed from a post the bot will identify whether the relevant message
        /// is one of its own, then react accordingly by firing applicable reaction methods.
        /// </summary>
        /// <param name="client">The discord client sending the message.</param>
        /// <param name="args">An object describing the context of the message sent.</param>
        /// <returns>A task to handle processing of the command.</returns>
        private async Task OnReactionRemoved(DiscordClient client, MessageReactionRemoveEventArgs args)
        {
            if (args.Message.Author.Id == Details.ID)
            {
                TConfig config = GetConfig(args.Guild);

                await ReactionRemoved(new ReactionRemoveArgs<TConfig>(args, config));
            }
        }

        /// <summary>
        /// A simple parameterless command for shutting down the bot.
        /// </summary>
        /// <param name="match">The regex match for command parameters.</param>
        /// <param name="args">The context for the message invoking the command.</param>
        /// <returns>An awaitable task for the command.</returns>
        [CommandAttribute("shutdown", CommandLevel = CommandLevel.Admin)]
        protected async Task Shutdown(CommandArgs<TConfig> args)
        {
            await args.Channel.SendMessageAsync("Shutting down...");
            Stop();
        }
        /// <summary>
        /// Occurs when the bot is first run.
        /// </summary>
        protected virtual void OnStartup()
        {
            LoadConfig();
            Console.WriteLine("Starting up...");
        }
        /// <summary>
        /// Occurs when the bot is shut down and the main worker thread ceases.
        /// </summary>
        protected virtual void OnShutdown()
        {
            SaveConfig();
            Console.WriteLine("Shutting down...");
        }
        /// <summary>
        /// Occurs when the bot connects using a discord client.
        /// </summary>
        /// <param name="client">The client representing the Discord API connection.</param>
        protected virtual void OnConnected(DiscordClient client)
        {
            Console.WriteLine("Connected.");
        }
        /// <summary>
        /// Creates a default config data structure for new servers.
        /// </summary>
        /// <returns>a config data structure.</returns>
        protected abstract TConfig CreateDefaultConfig();
        /// <summary>
        /// Called when one of this bot's posts has a reaction removed.
        /// </summary>
        /// <param name="args">The context for the reaction removal.</param>
        /// <returns>A task for handling the reaction.</returns>
        protected virtual async Task ReactionRemoved(ReactionRemoveArgs<TConfig> args)
        {

        }
        /// <summary>
        /// Called when one of this bot's posts has a reaction added.
        /// </summary>
        /// <param name="args">The context for the reaction addition.</param>
        /// <returns>A task for handling the reaction.</returns>
        protected virtual async Task ReactionAdded(ReactionAddArgs<TConfig> args)
        {

        }

        /// <summary>
        /// Attaches this bot to the client of an existing bot.
        /// Both bots will subsequently use the same client for their operations.
        /// Multiple bots cannot operate with the same client ID and token at the same time, so this
        /// enables multiple bots to run at the same time through the same user.
        /// </summary>
        /// <param name="other">The bot whose client to attach this bot to.</param>
        /// <returns>A task that can be stopped using the Stop() method.</returns>
        public async Task AttachToAsync<TOtherConfig>(Bot<TOtherConfig> other) where TOtherConfig : BotConfig
        {
            try
            {
                OnStartup();

                Details = other.Details;
                
                Client = other.Client;

                Client.MessageCreated += OnMessageCreated;
                Client.MessageReactionAdded += OnReactionAdded;
                Client.MessageReactionRemoved += OnReactionRemoved;

                OnConnected(Client);

                CancellationSource = new CancellationTokenSource();
                await Task.Delay(-1, CancellationSource.Token);

                Client.MessageCreated -= OnMessageCreated;
                Client.MessageReactionAdded -= OnReactionAdded;
                Client.MessageReactionRemoved -= OnReactionRemoved;
            }
            catch (TaskCanceledException tce)
            {
                OnShutdown();
            }
            finally
            {
                Client = null;
                CancellationSource = null;
            }
        }
        /// <summary>
        /// Runs the bot as a new task that terminates when the bot is shut down.
        /// </summary>
        /// <returns>A task that can be stopped using the Stop() method.</returns>
        public async Task RunAsync(ClientDetails details)
        {
            try
            {
                OnStartup();
                
                Details = details;
                
                Client = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Details.Token,
                    TokenType = TokenType.Bot
                });

                await Client.ConnectAsync();

                Client.MessageCreated += OnMessageCreated;
                Client.MessageReactionAdded += OnReactionAdded;
                Client.MessageReactionRemoved += OnReactionRemoved;

                OnConnected(Client);

                CancellationSource = new CancellationTokenSource();
                await Task.Delay(-1, CancellationSource.Token);

                await Client.DisconnectAsync();
            }
            catch (TaskCanceledException tce)
            {
                OnShutdown();
            }
            finally
            {
                Client = null;
                CancellationSource = null;
            }
        }
        /// <summary>
        /// Stops the bot by cancelling the client worker thread.
        /// </summary>
        public void Stop()
        {
            CancellationSource.Cancel();
        }
    }
}