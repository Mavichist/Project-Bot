namespace AwardBot
{
    public delegate void AbilityCallback(AbilityArgs args);
    
    public class Ability
    {
        public string Description
        {
            get;
            private set;
        }
        public AbilityCallback Callback
        {
            get;
            private set;
        }
        public string ParameterRegex
        {
            get;
            private set;
        }

        public Ability(string description, AbilityCallback callback, string parameterRegex)
        {
            Description = description;
            Callback = callback;
            ParameterRegex = parameterRegex;
        }
    }
}