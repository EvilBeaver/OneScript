using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.ComponentModel
{
    public abstract class ComponentBase : GenericValue
    {
        private DataType _type;

        internal void SetDataType(DataType type)
        {
            SetDataTypeInternal(type);
        }

        protected void SetDataTypeInternal(DataType type)
        {
            System.Diagnostics.Debug.Assert(Type == null);
            _type = type;
        }

        public override DataType Type
        {
            get
            {
                return _type;
            }
        }

    }
}
