/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Values;

namespace OneScript.Contexts
{
    public abstract class ValueContextBase : IContext
    {
        public virtual IReadOnlyList<BslPropertyInfo> GetProperties() => Array.Empty<BslPropertyInfo>();

        public virtual IReadOnlyList<BslMethodInfo> GetMethods() => Array.Empty<BslMethodInfo>();
        
        public virtual BslMethodInfo FindMethod(string name) => default;

        public virtual BslPropertyInfo FindProperty(string name) => default;
        
        public virtual bool DynamicMethodSignatures => false;
        
        public virtual void SetPropValue(BslPropertyInfo property, BslValue value)
        {
            throw new NotImplementedException();
        }

        public virtual BslValue GetPropValue(BslPropertyInfo property)
        {
            throw new NotImplementedException();
        }

        public virtual void CallAsProcedure(BslMethodInfo method, IReadOnlyList<BslValue> arguments)
        {
            throw new NotImplementedException();
        }

        public virtual BslValue CallAsFunction(BslMethodInfo method, IReadOnlyList<BslValue> arguments)
        {
            throw new NotImplementedException();
        }
    }
}