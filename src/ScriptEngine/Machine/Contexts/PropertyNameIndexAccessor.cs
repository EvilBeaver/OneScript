/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Commons;
using OneScript.Types;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class PropertyNameIndexAccessor : ContextIValueImpl
    {
        protected PropertyNameIndexAccessor()
        {
        }
        
        protected PropertyNameIndexAccessor(TypeDescriptor type):base(type)
        {
        }

        public override bool IsIndexed => true;

        public override bool IsPropReadable(int propNum)
        {
            return false;
        }

        public override bool IsPropWritable(int propNum)
        {
            return false;
        }

        public override IValue GetIndexedValue(IValue index)
        {
            if (index.SystemType != BasicTypes.String)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var n = GetPropertyNumber(index.AsString());
            if (IsPropReadable(n))
                return GetPropValue(n);
            else
                throw PropertyAccessException.PropIsNotReadableException(index.AsString());
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.SystemType != BasicTypes.String)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var n = GetPropertyNumber(index.AsString());
            if (IsPropWritable(n))
                SetPropValue(n, val);
            else
                throw PropertyAccessException.PropIsNotWritableException(index.AsString());
        }
    }
}
