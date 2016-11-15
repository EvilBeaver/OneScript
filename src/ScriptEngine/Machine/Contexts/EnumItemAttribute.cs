using System;
namespace ScriptEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumItemAttribute : Attribute
    {
        public EnumItemAttribute (string name, string alias = null)
        {
            if (!Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier");

            if (!string.IsNullOrEmpty(alias) && !Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Alias must be a valid identifier");

            Name = name;
            Alias = alias;
        }

        public string Name { get; }
        public string Alias { get; }
    }
}
