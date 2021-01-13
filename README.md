# Project Bot
## About
The project bot and its related scaffolding are designed to handle the creation, deletion and access of private channels for those participating in ComSSA projects. It is not necessarily specific to ComSSA, and has many other applications.
## Components
### BotScaffold
The bot scaffold project defines a base class for bots, as well as a system for implementing commands.

The bot scaffold consists of the `Bot` base class, which future bots should inherit from. It provides core functionality and simplifies the underlying DSharpPlus calls.

The `Command` and `CommandAttribute` classes are used by the `Bot` class and form the basis of new bot commands. Classes inheriting from `Bot` can automate the registration of commands using the `CommandAttribute` system.

When creating a new bot using the scaffold, create a new class and inherit from `Bot`. To register commands and add functionality to the bot, create either public or private methods that match the `CommandCallback` delegate and tag them with the `CommandAttribute` attribute. You can register the command string, as well as a regular expression for extracting parameters, in the constructor of the attribute.

The `Bot` base class will construct a command list when a new instance of your bot class is created. The command methods will only be called if a valid command is entered by a user (the command is properly formatted and the regular expression matches).

An example of a simple *Echo* bot is shown below:

```csharp
public class EchoBot : Bot
{
    public EchoBot(ClientDetails details) : base(details)
    {
        
    }

    [CommandAttribute("echo", ParameterRegex = "\"(?<phrase>\\w+)\"")]
    private async Task EchoPhrase(Match match, MessageCreateEventArgs args)
    {
        string phrase = match.Groups["phrase"].Value;
        await args.Channel.SendMessageAsync($"Echoing: {phrase}");
    }
}
```