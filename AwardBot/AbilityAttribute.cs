using System;

namespace AwardBot
{
    [AttributeUsage(System.AttributeTargets.All, Inherited = false, AllowMultiple = true)]
    sealed class AbilityAttribute : System.Attribute
    {
        private readonly string name;
        public string Name
        {
            get;
        }
        private readonly string description;
        public string Description
        {
            get;
        }
        public string ParameterRegex
        {
            get;
            set;
        }

        public AbilityAttribute(string name, string description)
        {
            this.name = name;
            this.description = description;
        }
    }
}