/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    internal class PropertyBag : DynamicPropertiesAccessor, IAttachableContext
    {
        private readonly List<IValue> _values = new List<IValue>();
        
        public void Insert(IValue value, string identifier)
        {
            Insert(value, identifier, true, true);
        }

        public int Insert(IValue value, string identifier, bool canRead, bool canWrite)
        {
            var num = RegisterProperty(identifier, canRead, canWrite);

            if (num == _values.Count)
            {
                _values.Add(null);
            }

            value ??= ValueFactory.Create();

            SetPropValue(num, value);

            return num;
        }

        public int Insert(IValue value, BslPropertyInfo definition)
        {
            var num = RegisterProperty(definition);
            if (num == _values.Count)
            {
                _values.Add(null);
            }

            value ??= ValueFactory.Create();

            _values[num] = value;

            return num;
        }

        public override bool IsPropReadable(int propNum)
        {
            return GetPropertyInfo(propNum).CanRead;
        }

        public override bool IsPropWritable(int propNum)
        {
            return GetPropertyInfo(propNum).CanWrite;
        }

        public override IValue GetPropValue(int propNum)
        {
            return _values[propNum];
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            _values[propNum] = newVal;
        }
        
        public int Count => _values.Count;

        public override int GetMethodsCount()
        {
            return 0;
        }

        #region IAttachableContext Members

        IVariable IAttachableContext.GetVariable(int index) => 
            Variable.CreateContextPropertyReference(this, index, GetPropertyName(index));
        
        BslMethodInfo IAttachableContext.GetMethod(int index) => throw new ArgumentOutOfRangeException();

        int IAttachableContext.VariablesCount => this.Count;
        
        int IAttachableContext.MethodsCount => 0;

        #endregion
    }
}
