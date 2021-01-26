using DSharpPlus.Entities;

namespace BotScaffold
{
    public static class EmojiUtils
    {
        public static string FormatName(this DiscordEmoji emoji)
        {
            if (emoji.RequiresColons)
            {
                return $"<{emoji.GetDiscordName()}{emoji.Id}>";
            }
            else
            {
                return emoji.Name;
            }
        }
    }
}