using DSharpPlus.Entities;

namespace BotScaffold
{
    public static class EmojiUtils
    {
        public static string FormatName(this DiscordEmoji emoji)
        {
            if (emoji.RequiresColons)
            {
                return $"<{emoji.Name}{emoji.Id}>";
            }
            else
            {
                return emoji.Name;
            }
        }
    }
}