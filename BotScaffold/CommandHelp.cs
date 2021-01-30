namespace BotScaffold
{
    /// <summary>
    /// A small class for housing command help information for the user to read.
    /// </summary>
    public class CommandHelp
    {
        /// <summary>
        /// A string describing the use-case and general information about a command.
        /// </summary>
        public string UsageInfo
        {
            get;
            private set;
        }
        /// <summary>
        /// An array of strings that each describe the parameters of a command.
        /// </summary>
        /// <value></value>
        public string[] Arguments
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new instance of a command help object.
        /// </summary>
        /// <param name="usageInfo">The general usage information for the command.</param>
        /// <param name="arguments">The list of argument information strings.</param>
        public CommandHelp(string usageInfo, params string[] arguments)
        {
            UsageInfo = usageInfo;
            Arguments = arguments;
        }
    }
}