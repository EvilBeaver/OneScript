using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.ComponentModel
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ImportedClassAttribute : Attribute
    {
        public ImportedClassAttribute()
        {
        }

        public string Name { get; set; }
        public string Alias { get; set; }

    }
}
