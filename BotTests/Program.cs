﻿using System.Threading.Tasks;
using BotScaffold;
using ProjectBot;

namespace BotTests
{
    /// <summary>
    /// Entrypoint for the program.
    /// Sample invite link: https://discord.com/oauth2/authorize?client_id=798470738868043786&scope=bot&permissions=268511312
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Serves as the main entrypoint for the program.
        /// Launches the bot and then hangs until the bot is shut down.
        /// </summary>
        /// <param name="args">Command-line arguments supplied to the program.</param>
        public static void Main(string[] args)
        {
            ClientDetails details = ClientDetails.LoadFrom("clientdetails.json");
            
            ProjectManagerBot bot = new ProjectManagerBot(details);
            Task b1 = bot.RunAsync();

            ProjectManagerBot bot2 = new ProjectManagerBot(details);
            Task b2 = bot2.AttachToAsync(bot);

            Task.WaitAll(b1, b2);
            System.Console.WriteLine("Finished.");
        }
    }
}