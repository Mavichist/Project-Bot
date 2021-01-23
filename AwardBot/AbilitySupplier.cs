using System;
using System.Collections.Generic;
using System.Reflection;

namespace AwardBot
{
    public class AbilitySupplier
    {
        public string Name
        {
            get;
            private set;
        }
        public Dictionary<string, Ability> Abilities
        {
            get;
            private set;
        } = new Dictionary<string, Ability>();
    
        public AbilitySupplier(string name)
        {
            Name = name;
            GetAbilities();
        }

        private void GetAbilities()
        {
            Abilities.Clear();

            Type type = GetType();
            
            foreach (MethodInfo m in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                AbilityAttribute attr = m.GetCustomAttribute<AbilityAttribute>();
                if (attr != null)
                {
                    AbilityCallback callback = m.CreateDelegate<AbilityCallback>();
                    Ability ability = new Ability(attr.Description, callback, attr.ParameterRegex);
                    Abilities.Add(attr.Name, ability);
                }
            }
        }
    }
}