using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
    [ContextClass("КоллекцияКадровСтекаВызовов", "CallStackFramesCollection")]
    public class StackTraceCollectionContext : AutoContext<StackTraceCollectionContext>, ICollectionContext
    {

        internal StackTraceCollectionContext(IEnumerable<ExecutionFrameInfo> frames)
        {
            throw new NotImplementedException();
        }

        public int Count()
        {
            throw new NotImplementedException();
        }

        public CollectionEnumerator GetManagedIterator()
        {
            throw new NotImplementedException();
        }
    }
}
