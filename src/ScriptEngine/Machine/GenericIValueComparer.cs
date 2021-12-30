/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    public class GenericIValueComparer : IEqualityComparer<IValue>, IComparer<IValue>
    {

        public bool Equals(IValue x, IValue y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(IValue obj)
        {
            object CLR_obj;
            if (obj is BslUndefinedValue)
                return obj.GetHashCode();

            try
            {
                CLR_obj = ContextValuesMarshaller.ConvertToClrObject(obj);
            }
            catch (ValueMarshallingException)
            {
                CLR_obj = obj;
            }

            return CLR_obj.GetHashCode();
        }

        public int Compare(IValue x, IValue y)
        {
            if (ReferenceEquals(x, default) && ReferenceEquals(y, default))
                return 0;
            
            if (x.SystemType == y.SystemType)
                return x.CompareTo(y);
            else
                return x.AsString().CompareTo(y.AsString());
        }
    }
}
