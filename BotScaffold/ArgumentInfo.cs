namespace BotScaffold
{
    /// <summary>
    /// A simple data structure for representing argument help information.
    /// </summary>
    public class ArgumentInfo
    {
        /// <summary>
        /// The name of the argument.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }
        /// <summary>
        /// A description of the argument.
        /// </summary>
        public string Info
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates a new instance of an argument info object.
        /// </summary>
        /// <param name="name">The name of the argument.</param>
        /// <param name="info">A description of the argument.</param>
        public ArgumentInfo(string name, string info)
        {
            Name = name;
            Info = info;
        }
    }
}