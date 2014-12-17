using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GlobalContextAttribute : Attribute
    {
        public string Category { get; set; }
        public bool ManualRegistration { get; set; }
    }
}
