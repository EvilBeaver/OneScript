using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class SelfAwareEnumValue<TOwner> : EnumerationValue where TOwner : EnumerationContext
    {
        public SelfAwareEnumValue(TOwner owner) : base(owner)
        {
        }
    }
}
