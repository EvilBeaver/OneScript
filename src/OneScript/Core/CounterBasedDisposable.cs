using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public class CounterBasedLifetimeService : IRefCountable
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
                if (BeforeDisposal != null)
                {
                    lock (BeforeDisposal)
                    {
                        if (BeforeDisposal != null)
                            BeforeDisposal(this, new EventArgs());
                    }
                }
            }

            return count;
        }

        public event EventHandler BeforeDisposal;

    }
}
