/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Types;

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

        public IValue this[string propName]
        {
            get
            {
                var propIdx = FindProperty(propName);
                return GetPropValue(propIdx);
            }
            set
            {
                var propIdx = FindProperty(propName);
                SetPropValue(propIdx, value);
            }
        }
        
        public override IValue GetIndexedValue(IValue index)
        {
            if (index.DataType != DataType.String)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var n = FindProperty(index.AsString());
            if (IsPropReadable(n))
                return GetPropValue(n);
            else
                throw RuntimeException.PropIsNotReadableException(index.AsString());
        }

        public override void SetIndexedValue(IValue index, IValue val)
        {
            if (index.DataType != DataType.String)
            {
                throw RuntimeException.InvalidArgumentType();
            }

            var n = FindProperty(index.AsString());
            if (IsPropWritable(n))
                SetPropValue(n, val);
            else
                throw RuntimeException.PropIsNotWritableException(index.AsString());
        }
    }
}
