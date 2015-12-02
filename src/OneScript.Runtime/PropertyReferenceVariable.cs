using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    class PropertyReferenceVariable : IVariable
    {
        IRuntimeContextInstance _target;
        int _propertyIndex;

        public PropertyReferenceVariable(IRuntimeContextInstance target, int propertyIndex)
        {
            _target = target;
            _propertyIndex = propertyIndex;
        }

        public IValue Value
        {
            get
            {
                return _target.GetPropertyValue(_propertyIndex);
            }
            set
            {
                _target.SetPropertyValue(_propertyIndex, value);
            }
        }
    }
}
