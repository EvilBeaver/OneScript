using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.ComponentModel
{
    public abstract class ComponentBase : GenericValue
    {
        internal void SetDataType(DataType type)
        {
            System.Diagnostics.Debug.Assert(Type == null);
            this.Type = type;
        }

    }
}
