using System;
namespace ScriptEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FieldContextAttribute : Attribute
    {
        public FieldContextAttribute (string name, string alias = null)
        {
            Name = name;
            Alias = alias;
        }

        public string Name { get; }
        public string Alias { get; }
    }
}
