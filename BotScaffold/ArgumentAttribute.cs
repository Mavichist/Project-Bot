namespace BotScaffold
{
    /// <summary>
    /// An attribute for describing command arguments.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    sealed class ArgumentAttribute : System.Attribute
    {
        private readonly string name;
        private readonly string info;
        
        /// <summary>
        /// The name of the attribute for documentation purposes.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }
        /// <summary>
        /// Describes the purpose of the argument and its range.
        /// </summary>
        public string Info
        {
            get
            {
                return info;
            }
        }
        
        /// <summary>
        /// Creates a new instance of an argument attribute.
        /// </summary>
        /// <param name="name">The name of the attribute.</param>
        /// <param name="info"></param>
        public ArgumentAttribute(string name, string info)
        {
            this.name = name;
            this.info = info;
        }
    }
}