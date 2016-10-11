using System;
namespace ScriptEngine
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ContextFieldAttribute : Attribute
    {
        public ContextFieldAttribute (string name, string alias = null)
        {
            Name = name;
            Alias = alias;
        }

        public string Name { get; }
        public string Alias { get; }
    }
}
