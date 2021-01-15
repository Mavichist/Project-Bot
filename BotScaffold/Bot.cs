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
    public abstract class Bot
    {
        /// <summary>
        /// The identifying number for this client on Discord.
        /// </summary>
        public ulong ID
        {
            get;
            private set;
        }
        /// <summary>
        /// The token for this client on Discord.
        /// </summary>
        public string Token
        {
            get;
            private set;
        }
        /// <summary>
        /// All commands processed by this bot must begin with this character.
        /// </summary>
        public char Indicator
        {
            get;
            private set;
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
        private List<Command> Commands
        {
            get;
            set;
        }
        /// <summary>
        /// A list of IDs for roles with admin permissions.
        /// </summary>
        private List<ulong> AdminRoleIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a new instance of a Discord bot, with the specified details.
        /// </summary>
        /// <param name="details">The client details object containing the bot user information.</param>
        public Bot(ClientDetails details)
        {
            ID = details.ID;
            Token = details.Token;
            Indicator = details.Indicator;
            AdminRoleIDs = new List<ulong>(details.AdminRoleIDs);
            Commands = Command.GetCommands(this);
        }

        /// <summary>
        /// Determines whether a user has the authority to run the specified command.
        /// </summary>
        /// <param name="guild">The guild the command was used in.</param>
        /// <param name="userID">The ID of the user attempting a command.</param>
        /// <returns>The command level the user can access.</returns>
        private async Task<CommandLevel> Auth(DiscordGuild guild, ulong userID)
        {
            DiscordMember member = await guild.GetMemberAsync(userID);
            if (member.IsOwner)
            {
                return CommandLevel.Owner;
            }
            else
            {
                foreach (var role in member.Roles)
                {
                    if (AdminRoleIDs.Contains(role.Id))
                    {
                        return CommandLevel.Admin;
                    }
                }
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
        /// <param name="sender">The discord client sending the message.</param>
        /// <param name="args">An object describing the context of the message sent.</param>
        /// <returns>A task to handle processing of the command.</returns>
        private async Task OnMessageCreated(DiscordClient sender, MessageCreateEventArgs args)
        {
            // Check to see if the post starts with our indicator and the author wasn't a bot.
            if (args.Message.Content.StartsWith(Indicator) && !args.Author.IsBot)
            {
                // Get the command level so we can filter out commands this user can't access.
                CommandLevel level = await Auth(args.Guild, args.Author.Id);
                bool commandFound = false;
                foreach (Command c in Commands)
                {
                    // Only if the command is lower or equal to the user level do we attempt to run it.
                    if (c.CommandLevel <= level)
                    {
                        // If the regex matches and the command succeeds, we can skip the rest.
                        if (await c.AttemptAsync(args))
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
        /// Occurs when the bot is first run.
        /// </summary>
        protected virtual void OnStartup()
        {
            Console.WriteLine("Starting up...");
        }
        /// <summary>
        /// Occurs when the bot is shut down and the main worker thread ceases.
        /// </summary>
        protected virtual void OnShutdown()
        {
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
        /// Attaches this bot to the client of an existing bot.
        /// Both bots will subsequently use the same client for their operations.
        /// Multiple bots cannot operate with the same client ID and token at the same time, so this
        /// enables multiple bots to run at the same time through the same user.
        /// </summary>
        /// <param name="other">The bot whose client to attach this bot to.</param>
        /// <returns>A task that can be stopped using the Stop() method.</returns>
        public async Task AttachToAsync(Bot other)
        {
            try
            {
                OnStartup();
                
                Client = other.Client;

                Client.MessageCreated += OnMessageCreated;

                OnConnected(Client);

                CancellationSource = new CancellationTokenSource();
                await Task.Delay(-1, CancellationSource.Token);

                Client.MessageCreated -= OnMessageCreated;
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
        public async Task RunAsync()
        {
            try
            {
                OnStartup();
                
                Client = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Token,
                    TokenType = TokenType.Bot
                });

                await Client.ConnectAsync();

                Client.MessageCreated += OnMessageCreated;

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