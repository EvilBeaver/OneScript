using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class SelfAwareEnumValue<TOwner> : EnumerationValue where TOwner : EnumerationContext
    {
        private int _realValue;
        
        public SelfAwareEnumValue(TOwner owner, int value) : base(owner)
        {
            _realValue = value;
        }

        public int UnderlyingValue { get { return _realValue; } }

        public override bool Equals(IValue other)
        {
            var enumCtx = other.GetRawValue() as SelfAwareEnumValue<TOwner>;
            if(enumCtx != null)
            {
                return enumCtx.UnderlyingValue == this.UnderlyingValue;
            }

            return false;
        }

    }
}
