using System.Threading.Tasks;
using BotScaffold;
using RoleBot;
using PurgeBot;

namespace BotTests
{
    /// <summary>
    /// Entrypoint for the program.
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
            BotInstance instance = new BotInstance();
            instance.Init();
            
            CoreBot coreBot = new CoreBot("CoreBot");
            RoleManagerBot roleBot = new RoleManagerBot("RoleBot");
            MessagePurgeBot purgeBot = new MessagePurgeBot("PurgeBot");

            coreBot.AttachTo(instance);
            roleBot.AttachTo(instance);
            purgeBot.AttachTo(instance);

            Task.WaitAll(instance.RunAsync());

            System.Console.WriteLine("Finished.");
        }
    }
}