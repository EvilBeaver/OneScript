/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    class PropertyBag : DynamicPropertiesAccessor, IAttachableContext
    {
        private struct PropertyAccessFlags
        {
            public bool CanRead;
            public bool CanWrite;
        }

        private readonly List<IValue> _values = new List<IValue>();
        private readonly List<PropertyAccessFlags> _accessFlags = new List<PropertyAccessFlags>();

        public void Insert(IValue value, string identifier)
        {
            Insert(value, identifier, true, true);
        }

        public void Insert(IValue value, string identifier, bool canRead, bool canWrite)
        {
            var num = RegisterProperty(identifier);

            if (num == _values.Count)
            {
                _values.Add(null);
                _accessFlags.Add(new PropertyAccessFlags() { CanRead = canRead, CanWrite = canWrite });
            }

            if (value == null)
            {
                value = ValueFactory.Create();
            }

            SetPropValue(num, value);

        }

        public override bool IsPropReadable(int propNum)
        {
            return _accessFlags[propNum].CanRead;
        }

        public override bool IsPropWritable(int propNum)
        {
            return _accessFlags[propNum].CanWrite;
        }

        public override IValue GetPropValue(int propNum)
        {
            return _values[propNum];
        }

        public override void SetPropValue(int propNum, IValue newVal)
        {
            _values[propNum] = newVal;
        }

        public int Count
        {
            get
            {
                return _values.Count;
            }
        }

        #region IAttachableContext Members

        public void OnAttach(MachineInstance machine, out IVariable[] variables, out MethodInfo[] methods)
        {
            variables = new IVariable[this.Count];
            var props = GetProperties().OrderBy(x => x.Value).Select(x=>x.Key).ToArray();
            Debug.Assert(props.Length == variables.Length);

            for (var i = 0; i < variables.Length; i++)
            {
                variables[i] = Variable.CreateContextPropertyReference(this, i, props[i]);
            }

            methods = new MethodInfo[0];
        }

        #endregion
    }
}
