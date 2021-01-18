using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace BotScaffold
{
    public delegate void BotStartupCallback();
    public delegate void BotShutdownCallback();
    public delegate void BotConnectedCallback();

    /// <summary>
    /// An instance that runs several bots (really bot extensions) at once.
    /// </summary>
    public class BotInstance
    {
        /// <summary>
        /// The client ID and token for interacting with the Discord API.
        /// </summary>
        public ClientDetails Details
        {
            get;
            private set;
        }
        /// <summary>
        /// The client object through which the bot communicates with the Discord API.
        /// </summary>
        public DiscordClient Client
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
        /// Fires when the bot is first run.
        /// </summary>
        private BotStartupCallback onStartup;
        /// <summary>
        /// Fires when the bot shuts down.
        /// </summary>
        private BotShutdownCallback onShutdown;
        /// <summary>
        /// Fires when the discord client connects for the first time.
        /// </summary>
        private BotConnectedCallback onConnected;

        /// <summary>
        /// Creates a new bot instance that can have bot implementations attached to it.
        /// </summary>
        /// <param name="details">The client ID and token with which to interact with the API.</param>
        public BotInstance(ClientDetails details)
        {
            Details = details;
            Client = new DiscordClient(new DiscordConfiguration()
            {
                Token = Details.Token,
                TokenType = TokenType.Bot
            });
        }

        /// <summary>
        /// Runs the bot as a new task that terminates when the bot is shut down.
        /// </summary>
        /// <returns>A task that can be stopped using the Stop() method.</returns>
        public async Task RunAsync()
        {
            try
            {
                onStartup();

                await Client.ConnectAsync();

                onConnected();

                CancellationSource = new CancellationTokenSource();
                await Task.Delay(-1, CancellationSource.Token);

                await Client.DisconnectAsync();

                onShutdown();
            }
            catch (TaskCanceledException tce)
            {
                
            }
            finally
            {
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

        /// <summary>
        /// A single instance of a discord bot, with an ID and a token.
        /// </summary>
        public abstract class Bot<TConfig> where TConfig : BotConfig
        {
            /// <summary>
            /// The name of this bot (used to load config).
            /// </summary>
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
            /// A list of commands, sorted from most specific to least specific.
            /// </summary>
            private List<Command<TConfig>> Commands
            {
                get;
                set;
            }
            /// <summary>
            /// The instance this bot runs on.
            /// </summary>
            protected BotInstance Instance
            {
                get;
                set;
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
                    foreach (Command<TConfig> c in Commands)
                    {
                        // Only if the command is lower or equal to the user level do we attempt to run it.
                        if (c.CommandLevel <= level)
                        {
                            // If the regex matches and the command succeeds, we can skip the rest.
                            CommandState state = await c.AttemptAsync(args, config);
                            if (state == CommandState.Handled)
                            {
                                break;
                            }
                            else if (state == CommandState.ParameterError)
                            {
                                await args.Channel.SendMessageAsync($"Incorrectly formatted parameters. Should match `{c.ParameterRegex}`");
                            }
                        }
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
                // Bots shouldn't handle their own reactions.
                if (args.User.Id != Instance.Details.ID)
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
                // Bots shouldn't handle their own reactions.
                if (args.User.Id != Instance.Details.ID)
                {
                    TConfig config = GetConfig(args.Guild);
                    await ReactionRemoved(new ReactionRemoveArgs<TConfig>(args, config));
                }
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
            /// Fires when the bot instance connects.
            /// </summary>
            protected virtual void OnConnected()
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
            /// Attaches this bot to an instance so it can run.
            /// </summary>
            /// <param name="instance">The instance to attach the bot to.</param>
            public void AttachTo(BotInstance instance)
            {
                if (Instance != null)
                {
                    Detach();
                }

                Instance = instance;

                Instance.Client.MessageCreated += OnMessageCreated;
                Instance.Client.MessageReactionAdded += OnReactionAdded;
                Instance.Client.MessageReactionRemoved += OnReactionRemoved;

                Instance.onStartup += OnStartup;
                Instance.onShutdown += OnShutdown;
                Instance.onConnected += OnConnected;
            }
            /// <summary>
            /// Detaches the bot from its instance.
            /// </summary>
            public void Detach()
            {
                if (Instance != null)
                {
                    Instance.Client.MessageCreated -= OnMessageCreated;
                    Instance.Client.MessageReactionAdded -= OnReactionAdded;
                    Instance.Client.MessageReactionRemoved -= OnReactionRemoved;

                    Instance.onStartup -= OnStartup;
                    Instance.onShutdown -= OnShutdown;
                    Instance.onConnected -= OnConnected;

                    Instance = null;
                }
            }
        }
    }
}