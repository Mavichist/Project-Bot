# Project Bot
## About
The project bot and its related scaffolding are designed to handle the creation, deletion and access of private channels for those participating in ComSSA projects. It is not necessarily specific to ComSSA, and has many other applications.
## Components
### BotScaffold
The bot scaffold project defines a base class for bots, as well as a system for implementing commands.

The bot scaffold consists of the `Bot` base class, which future bots should inherit from. It provides core functionality and simplifies the underlying DSharpPlus calls.

The `Command` and `CommandAttribute` classes are used by the `Bot` class and form the basis of new bot commands. Classes inheriting from `Bot` can automate the registration of commands using the `CommandAttribute` system.

