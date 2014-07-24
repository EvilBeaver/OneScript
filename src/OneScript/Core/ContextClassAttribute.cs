using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ContextClassAttribute : Attribute
    {
        public ContextClassAttribute()
        {
        }

        public string Name { get; set; }
        public string Alias { get; set; }

    }
}
