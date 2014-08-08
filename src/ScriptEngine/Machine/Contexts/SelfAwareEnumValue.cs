using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class SelfAwareEnumValue : EnumerationValue
    {
        public SelfAwareEnumValue(EnumerationContext owner) : base(owner)
        {
        }

    }
}
