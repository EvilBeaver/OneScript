using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public class CounterBasedLifetimeService : IRefCountable
    {
        int _refCount = 0;
        Action _releaseAction;

        public CounterBasedLifetimeService(Action releaseAction)
        {
            _releaseAction = releaseAction;
        }

        public int AddRef()
        {
            return System.Threading.Interlocked.Increment(ref _refCount);
        }

        public int Release()
        {
            int count = System.Threading.Interlocked.Decrement(ref _refCount);
            if(count == 0)
            {
                _releaseAction();
            }

            return count;
        }

    }
}
