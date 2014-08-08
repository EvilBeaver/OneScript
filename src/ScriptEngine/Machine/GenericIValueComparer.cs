using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class GenericIValueComparer : IEqualityComparer<IValue>
    {

        public bool Equals(IValue x, IValue y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IValue obj)
        {
            var CLR_obj = Contexts.COMWrapperContext.MarshalIValue(obj);
            return CLR_obj.GetHashCode();
        }
    }
}
