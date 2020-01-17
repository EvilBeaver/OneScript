/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
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
            if (obj.DataType == DataType.Undefined)
                return obj.GetHashCode();

            try
            {
                CLR_obj = ContextValuesMarshaller.ConvertToCLRObject(obj);
            }
            catch (ValueMarshallingException)
            {
                CLR_obj = obj;
            }

            return CLR_obj.GetHashCode();
        }

        public int Compare(IValue x, IValue y)
        {
            if (x.SystemType.ID == y.SystemType.ID)
                return x.CompareTo(y);
            else
                return x.AsString().CompareTo(y.AsString());
        }
    }
}
