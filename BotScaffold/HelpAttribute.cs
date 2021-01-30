using System;

namespace BotScaffold
{
    /// <summary>
    /// An attribute ascribing help information to a command method.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    sealed class HelpAttribute : Attribute
    {
        private readonly string usageInfo;
        private readonly string[] arguments;

        /// <summary>
        /// A string describing the use-case and general information about a command.
        /// </summary>
        public string UsageInfo
        {
            get
            {
                return usageInfo;
            }
        }
        /// <summary>
        /// An array of strings that each describe the parameters of a command.
        /// </summary>
        /// <value></value>
        public string[] Arguments
        {
            get
            {
                return arguments;
            }
        }

        /// <summary>
        /// Creates a new instance of a help attribute.
        /// </summary>
        /// <param name="usageInfo">The general usage information for the command.</param>
        /// <param name="arguments">The list of argument information strings.</param>
        public HelpAttribute(string usageInfo, params string[] arguments)
        {
            this.usageInfo = usageInfo;
            this.arguments = arguments;
        }
    
        /// <summary>
        /// Creates a new help object using this attribute's information.
        /// </summary>
        /// <returns>A command help object containing the same information as this attribute.</returns>
        public CommandHelp CreateHelp()
        {
            return new CommandHelp(usageInfo, arguments);
        }
    }
}