using System.Threading.Tasks;
using BotScaffold;

namespace RPGBot
{
    public partial class RPGManagerBot
    {
        /// <summary>
        /// A command for equipping armor from an inventory slot.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns>A task for completing the command.</returns>
        [CommandAttribute("equip armor", CommandLevel = CommandLevel.Unrestricted, ParameterRegex = "(?<index>\\d+)")]
        protected async Task EquipArmor(CommandArgs<RPGBotConfig> args)
        {
            int index = int.Parse(args["index"]);

            Player player = args.Config.GetPlayer(args.Author.Id);

            player.ArmorIndex = index;

            await args.Channel.SendMessageAsync($"{args.Author.Mention} inventory slot **{index}** is now equipped as armor.");
        }
    }
}