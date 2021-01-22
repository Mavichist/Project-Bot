using System.Threading.Tasks;
using BotScaffold;
using HangoutBot;
using RoleBot;
using AwardBot;

namespace BotTests
{
    /// <summary>
    /// Entrypoint for the program.
    /// Sample invite link: https://discord.com/oauth2/authorize?client_id=798470738868043786&scope=bot&permissions=268511312
    /// </summary>
    public class Program
    {
        public readonly static string CLIENT_DETAILS_FILE = "ClientDetails.json";

        /// <summary>
        /// Serves as the main entrypoint for the program.
        /// Launches the bot and then hangs until the bot is shut down.
        /// </summary>
        /// <param name="args">Command-line arguments supplied to the program.</param>
        public static void Main(string[] args)
        {
            ClientDetails details = ClientDetails.Load(CLIENT_DETAILS_FILE);
            BotInstance instance = new BotInstance(details);
            
            CoreBot coreBot = new CoreBot("CoreBot");
            HangoutManagerBot hangoutBot = new HangoutManagerBot("HangoutBot");
            RoleManagerBot roleBot = new RoleManagerBot("RoleBot");
            AwardManagerBot awardBot = new AwardManagerBot("AwardBot");

            coreBot.AttachTo(instance);
            hangoutBot.AttachTo(instance);
            roleBot.AttachTo(instance);
            awardBot.AttachTo(instance);

            Task.WaitAll(instance.RunAsync());

            details.Save(CLIENT_DETAILS_FILE);

            System.Console.WriteLine("Finished.");
        }
    }
}