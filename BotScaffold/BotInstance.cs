using System;
using System.Collections.Generic;
using System.Text;
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
    public delegate void BotSaveCallback();
    public delegate void BotLoadCallback();

    /// <summary>
    /// An instance that runs several bots (really bot extensions) at once.
    /// </summary>
    public class BotInstance
    {
        /// <summary>
        /// Defines the location of the client details json file.
        /// </summary>
        public readonly static string CLIENT_DETAILS_FILE = "ClientDetails.json";

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
        /// Fires when the bot instance attempts to save data.
        /// </summary>
        private BotSaveCallback onSave;
        /// <summary>
        /// Fires when the bot instance attempts to load data.
        /// </summary>
        private BotLoadCallback onLoad;
        /// <summary>
        /// Periodically saves config and client
        /// </summary>
        private Timer autoSaveTimer;

        /// <summary>
        /// Creates a new bot instance that can have bot implementations attached to it.
        /// </summary>
        public BotInstance()
        {
            
        }

        /// <summary>
        /// Initializes the client using information stored in the client details json file.
        /// This method must be called before bots are attached to the instance.
        /// </summary>
        public void Init()
        {
            Details = ClientDetails.Load(CLIENT_DETAILS_FILE);
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
                onLoad();

                await Client.ConnectAsync();

                onConnected();

                autoSaveTimer = new Timer((o) => { SaveAll(); }, null, Details.AutoSaveInterval, Details.AutoSaveInterval);

                CancellationSource = new CancellationTokenSource();
                await Task.Delay(-1, CancellationSource.Token);
            }
            catch (TaskCanceledException tce)
            {
                await Client.DisconnectAsync();

                await autoSaveTimer.DisposeAsync();
                autoSaveTimer = null;

                onShutdown();
                SaveAll();
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
        /// Saves all config files for all attached bots, as well as client details.
        /// </summary>
        public void SaveAll()
        {
            Details.Save(CLIENT_DETAILS_FILE);
            onSave();
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
                Commands = Command<TConfig>.GetCommands(this);
            }
            
            /// <summary>
            /// Determines the level of commands the user is capable of executing.
            /// </summary>
            /// <param name="guild">The guild the command was used in.</param>
            /// <param name="userID">The ID of the user attempting a command.</param>
            /// <returns>The command level the user can access.</returns>
            private async Task<CommandLevel> GetCommandLevelAsync(DiscordGuild guild, ulong userID)
            {
                DiscordMember member = await guild.GetMemberAsync(userID);
                
                // If the member is the owner of the server, they can execute any command.
                if (member.IsOwner)
                {
                    return CommandLevel.Owner;
                }
                else
                {
                    HashSet<ulong> adminRoleIDs = Instance.Details.GetAdminRoleIDs(guild.Id);

                    // We need to find an admin role in the member's role list for them to be considered
                    // an admin.
                    foreach (var role in member.Roles)
                    {
                        if (adminRoleIDs.Contains(role.Id))
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
                TConfig config = GetConfig(args.Guild.Id);
                
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
                            CommandState state = await c.AttemptAsync(args, config, Instance);
                            if (state == CommandState.Handled)
                            {
                                break;
                            }
                            else if (state == CommandState.ParameterError)
                            {
                                await args.Channel.SendMessageAsync($"Incorrectly formatted parameters. Should match ```{c.ParameterRegex}```");
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
                if (!args.User.IsBot)
                {
                    TConfig config = GetConfig(args.Guild.Id);
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
                if (!args.User.IsBot)
                {
                    TConfig config = GetConfig(args.Guild.Id);
                    await ReactionRemoved(new ReactionRemoveArgs<TConfig>(args, config));
                }
            }

            /// <summary>
            /// Retrieves the configuration for a specific guild.
            /// </summary>
            /// <param name="guild"></param>
            /// <returns>The configuration data structure for this guild.</returns>
            protected TConfig GetConfig(ulong guildID)
            {
                if (!ServerConfigs.TryGetValue(guildID, out TConfig config))
                {
                    config = CreateDefaultConfig();
                    ServerConfigs.Add(guildID, config);
                }
                return config;
            }
            /// <summary>
            /// Enumerates all of the guild IDs for which  this bot currently has configuration
            /// objects.
            /// </summary>
            /// <returns>An enumerable collection of unsigned 64-bit integers.</returns>
            protected IEnumerable<ulong> EnumerateGuildIDs()
            {
                foreach (var k in ServerConfigs)
                {
                    yield return k.Key;
                }
            }
            /// <summary>
            /// Saves the bot's config data structure to the config folder.
            /// </summary>
            protected virtual void OnSave()
            {
                Console.WriteLine($"Saving config for {Name}...");
                foreach (var serverConfig in ServerConfigs)
                {
                    BotConfig.Save(serverConfig.Value, Name, serverConfig.Key);
                }
            }
            /// <summary>
            /// Loads the config file associated with this bot.
            /// </summary>
            protected virtual void OnLoad()
            {
                Console.WriteLine($"Loading config for {Name}...");
                ServerConfigs = BotConfig.LoadAll<TConfig>(Name);
            }
            /// <summary>
            /// Occurs when the bot is first run.
            /// </summary>
            protected virtual void OnStartup()
            {
                Console.WriteLine($"Starting {Name}...");
            }
            /// <summary>
            /// Occurs when the bot is shut down and the main worker thread ceases.
            /// </summary>
            protected virtual void OnShutdown()
            {
                Console.WriteLine($"Shutting down {Name}...");
            }
            /// <summary>
            /// Fires when the bot instance connects.
            /// </summary>
            protected virtual void OnConnected()
            {
                Console.WriteLine($"{Name} connected.");
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
            /// A command for showing help for a given bot.
            /// This command is endemic to all bots and will generate one message for every bot that
            /// is attached to an instance.
            /// </summary>
            /// <param name="args">The context for the message invoking the command.</param>
            /// <returns>An awaitable task for the command.</returns>
            [Usage("Using this command will generate help information for my commands.")]
            [Command("help", CommandLevel = CommandLevel.Unrestricted)]
            protected async Task BotHelp(CommandArgs<TConfig> args)
            {
                if (args.Channel.Id == args.Config.HelpChannelID)
                {
                    DiscordEmbedBuilder builder = new DiscordEmbedBuilder();
                    builder.WithTitle($"Command List for {Name}:");

                    foreach (Command<TConfig> command in Commands)
                    {
                        string fieldName = $"`{args.Config.Indicator}{command.CommandString}`";

                        StringBuilder sb = new StringBuilder();

                        sb.Append($"- **Usage:** *{command.UsageInformation}*\n");
                        sb.Append($"- **Level:** *{command.CommandLevel}*\n");

                        if (command.ArgumentInfo.Count > 0)
                        {
                            sb.Append("- **Arguments:**\n");
                            foreach (var argumentInfo in command.ArgumentInfo)
                            {
                                sb.Append($"- + **{argumentInfo.Name}**: *{argumentInfo.Info}*\n");
                            }
                        }
                        else
                        {
                            sb.Append("- **Takes no arguments**\n");
                        }

                        builder.AddField(fieldName, sb.ToString());
                    }

                    DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);

                    await member.SendMessageAsync(null, false, builder.Build());
                }
            }
            /// <summary>
            /// A command for setting the designated help channel for a bot.
            /// </summary>
            /// <param name="args">The context for the message invoking the command.</param>
            /// <returns>An awaitable task for the command.</returns>
            [Usage("Using this command will set the current channel as the designated help channel, for using help commands in.")]
            [Command("set help channel", CommandLevel = CommandLevel.Admin)]
            protected async Task SetHelpChannel(CommandArgs<TConfig> args)
            {
                args.Config.HelpChannelID = args.Channel.Id;
                await args.Channel.SendMessageAsync($"**[{Name}]** I will now only allow help commands in this channel.");
            }
            /// <summary>
            /// A command for setting the indicator on a bot.
            /// Currently sets the indicator for every bot running on the instance.
            /// </summary>
            /// <param name="args">The context for the message invoking the command.</param>
            /// <returns>An awaitable task for the command.</returns>
            [Usage("Using this command will set the currently active bot indicator character.")]
            [Command("set indicator", CommandLevel = CommandLevel.Admin, ParameterRegex = "\"(?<indicator>[\\W\\S\\D])\"")]
            protected async Task SetIndicator(CommandArgs<TConfig> args)
            {
                args.Config.Indicator = args["indicator"][0];
                await args.Channel.SendMessageAsync($"**[{Name}]** I will now only respond to commands starting with **{args.Config.Indicator}**");
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
                Instance.onSave += OnSave;
                Instance.onLoad += OnLoad;
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
                    Instance.onSave -= OnSave;
                    Instance.onLoad -= OnLoad;

                    Instance = null;
                }
            }
        }
    }
}