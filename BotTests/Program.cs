using System.Threading.Tasks;
using BotScaffold;
using HangoutBot;
using RoleBot;

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
            ClientDetails details = ClientDetails.Load("ClientDetails.json");
            BotInstance instance = new BotInstance(details);
            
            CoreBot coreBot = new CoreBot("CoreBot");
            HangoutManagerBot hangoutBot = new HangoutManagerBot("HangoutBot");
            RoleManagerBot roleBot = new RoleManagerBot("RoleBot");

            coreBot.AttachTo(instance);
            hangoutBot.AttachTo(instance);
            roleBot.AttachTo(instance);

            Task.WaitAll(instance.RunAsync());

            System.Console.WriteLine("Finished.");
        }
    }
}