using System;

namespace BotScaffold
{
    /// <summary>
    /// An attribute ascribing help information to a command method.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class UsageAttribute : Attribute
    {
        private readonly string usageInfo;

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
        /// Creates a new instance of a help attribute.
        /// </summary>
        /// <param name="usageInfo">The general usage information for the command.</param>
        /// <param name="arguments">The list of argument information strings.</param>
        public UsageAttribute(string usageInfo)
        {
            this.usageInfo = usageInfo;
        }
    }
}