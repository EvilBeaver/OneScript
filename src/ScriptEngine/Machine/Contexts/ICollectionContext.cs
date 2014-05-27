using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public interface ICollectionContext : IEnumerable<IValue>
    {
        int Count();
        CollectionEnumerator GetManagedIterator();
    }
}
