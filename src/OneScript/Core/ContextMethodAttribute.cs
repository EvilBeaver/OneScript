using System;

namespace OneScript.Core
{
    [AttributeUsage(AttributeTargets.Method)]
    public class ContextMethodAttribute : Attribute
    {
        public ContextMethodAttribute()
        {
        }

        public string Name { get; set; }
        
        public string Alias { get; set; }
        
        public bool IsFunction { get; set; }
    }

}