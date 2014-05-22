using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    interface ICollectionContext : IEnumerable<IValue>
    {
        int Count();
        void Clear();
        CollectionEnumerator GetManagedIterator();
    }
}
