using System;

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContextClassAttribute : Attribute
    {
        string _name;
        string _alias;

        public ContextClassAttribute(string typeName, string typeAlias = "")
        {
            _name = typeName;
            _alias = typeAlias;
        }

        public string GetName()
        {
            return _name;
        }

        public string GetAlias()
        {
            return _alias;
        }

    }
}