using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
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
        /// Creates a new instance of a Discord bot, with the specified ID, indicator and token.
        /// </summary>
        /// <param name="id">A 64-bit, unsigned identifying number.</param>
        /// <param name="token">The Discord connection token string.</param>
        public Bot(ulong id, string token, char indicator)
        {
            ID = id;
            Token = token;
            Indicator = indicator;
            Commands = Command.GetCommands(this);
        }
        /// <summary>
        /// Creates a new instance of a Discord bot, with the specified details.
        /// </summary>
        /// <param name="details">The client details object containing the bot user information.</param>
        public Bot(ClientDetails details) : this(details.ID, details.Token, details.Indicator)
        {

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
            if (args.Author.Id != ID && args.Message.Content.StartsWith(Indicator))
            {
                bool success = false;
                foreach (Command c in Commands)
                {
                    success = await c.AttemptAsync(args);
                    if (success)
                    {
                        break;
                    }
                }
                if (!success)
                {
                    await args.Channel.SendMessageAsync("Invalid command.");
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
        /// Connects and runs the bot.
        /// Hangs until the bot is shut down.
        /// </summary>    
        public void Run()
        {
            try
            {
                OnStartup();
                
                Client = new DiscordClient(new DiscordConfiguration()
                {
                    Token = Token,
                    TokenType = TokenType.Bot
                });

                Client.ConnectAsync().GetAwaiter().GetResult();

                Client.MessageCreated += OnMessageCreated;

                OnConnected(Client);

                CancellationSource = new CancellationTokenSource();
                Task.Delay(-1, CancellationSource.Token).GetAwaiter().GetResult();

                Client.DisconnectAsync().GetAwaiter().GetResult();
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