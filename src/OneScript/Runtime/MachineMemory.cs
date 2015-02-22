using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class MachineMemory
    {
        List<MemoryAttachedContext> _attachedContexts = new List<MemoryAttachedContext>();

        public int Count
        {
            get
            {
                return _attachedContexts.Count;
            }
        }

        public MemoryAttachedContext this[int index]
        {
            get { return _attachedContexts[index]; }
        }

        public void Attach(MemoryAttachedContext memBlock)
        {
            _attachedContexts.Add(memBlock);
        }

        public MemoryAttachedContext Detach()
        {
            int lastIdx = Count - 1;
            var item = _attachedContexts[Count-1];
            _attachedContexts.RemoveAt(lastIdx);
            return item;
        }
    }
}
