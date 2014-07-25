using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class CounterBasedLifetime : IDisposable
    {
        int _refCount = 0;

        public int AddRef()
        {
            return System.Threading.Interlocked.Increment(ref _refCount);
        }

        public int Release()
        {
            int count = System.Threading.Interlocked.Decrement(ref _refCount);
            if(count == 0)
            {
                Dispose();
            }

            return count;
        }

        public virtual void Dispose()
        {
        }
    }
}
