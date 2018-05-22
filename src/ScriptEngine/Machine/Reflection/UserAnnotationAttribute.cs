using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptEngine.Machine.Reflection
{
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Parameter, AllowMultiple = true)]
    public class UserAnnotationAttribute : Attribute
    {
        public AnnotationDefinition Annotation { get; set; }
    }
}
