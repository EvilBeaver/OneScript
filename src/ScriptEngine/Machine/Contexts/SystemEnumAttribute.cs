using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SystemEnumAttribute : Attribute
    {
        private string _name;
        private string _alias;

        public SystemEnumAttribute(string name, string alias = "")
        {
            _name = name;
            _alias = alias;
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
