# Project Bot

## About

The project bot and its related scaffolding are designed to handle the creation, deletion and access of private channels for those participating in ComSSA projects. It is not necessarily specific to ComSSA, and has many other applications.

By adding more projects to this solution, additional bots can be created independent of any others. Multiple bots can be grafted together on startup, meaning that you can run more than one set of commands and functionality through a single client if you want to.

## Components

### BotScaffold
The bot scaffold project defines a base class for bots, as well as a system for implementing commands.

The bot scaffold consists of the `Bot` base class, which future bots should inherit from. It provides core functionality and simplifies the underlying DSharpPlus calls.

The `Command` and `CommandAttribute` classes are used by the `Bot` class and form the basis of new bot commands. Classes inheriting from `Bot` can automate the registration of commands using the `CommandAttribute` system.

When creating a new bot using the scaffold, create a new class and inherit from `Bot`. To register commands and add functionality to the bot, create either public or private methods that match the `CommandCallback` delegate type and tag them with the `CommandAttribute` attribute. You can register the command string, as well as a regular expression for extracting parameters, in the constructor of the attribute.

Commands in this framework consist of a `command string`, followed by `command parameters`. To simplify parsing and regex, parameters must always follow the command string, and the command string must be contiguous.

For example; `!start goblin "cheeky"` is a valid command because the command string (`start goblin`) comes before the parameter (`"cheeky"`). An invalid interpretation of the same command would be something like; `!goblin "cheeky" start`, because the command string is broken up by parameters (and is therefore not contiguous).

The `Bot` base class will construct a command list when a new instance of your bot class is created. The command methods will only be called if a valid command is entered by a user (the command is properly formatted and the regular expression matches parameters).

An example of a simple `Echo` bot is shown below:

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
    [CommandAttribute("shutdown")]
    private async Task BotShutdown(Match match, MessageCreateEventArgs args)
    {
        await args.Channel.SendMessageAsync("Shutting down...");
        Stop();
    }
}
```

In the above example, no `CommandLevel` is registered for any of the commands. Command levels are used to identify whether a user should be able to call the command in question. By default, for security, only server owners can use commands unless otherwise specified.

### BotTests
This project simply supplies an application entry-point, instantiates a bot for testing, then runs it.

### ProjectBot
This project contains a bot that handles the creation of projects, managing their associated channels, roles and permissions. It also contains the data structures and Json code necessary to save and load project lists for different servers.

## Walkthrough

In order to work with this repo you will need a Discord bot application to work with. You can create one using the following link:

`https://discord.com/developers/applications`

Once you've created a new application, click on the `Bot` tab and add a bot to the application.

You will also need to invite the bot user to your server using the following link (replacing the placeholder values with the information relevant to your bot):

`https://discord.com/oauth2/authorize?client_id=CLIENT_ID_HERE&scope=bot&permissions=BOT_PERMISSION_NUMBER_HERE`

You can generate a permission number using the Discord development tools site (the same one used to create the bot user in the first place). These permissions control what the bot is allowed to do on your server. The API will throw exceptions if the bot does not have adequate permissions to do what it tries to.

The bot test program requires that you have a `clientdetails.json` file in your working config directory. A template is supplied with this repo that you can fill out. You need to fill in your bot client ID and token so that it can connect to the Discord API and interact with servers. 

The config file needs to be located in a `Config` folder in the working directory. The file needs to have the same name as the one you give the bot at startup. Config files can have different data structure types, and contain different strongly-typed information, because all implementations of the `Bot` base class are generically implemented.

Once the bot is invited, and the client details have been filled in, you can start the bot test project by using the following command:

`dotnet run -p BotTests`

If all went according to plan, your bot should now appear to be online and should respond to commands.

Please note that you may need to use `dotnet restore` and/or NuGet to install the `DSharpPlus` library (upon which this repo is based).