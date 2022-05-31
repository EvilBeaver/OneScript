/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Types;

namespace ScriptEngine.Machine.Contexts
{
    public abstract class AutoCollectionContext<T, TItem> : AutoContext<T>, ICollectionContext<TItem>
        where T : AutoContext<T>
        where TItem : IValue
    {
        protected AutoCollectionContext()
        {
        }

        protected AutoCollectionContext(TypeDescriptor type) : base(type)
        {
        }
        
        public abstract int Count();
        
        public abstract IEnumerator<TItem> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}