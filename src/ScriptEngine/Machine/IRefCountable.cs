using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public interface IRefCountable
    {
        int AddRef();
        int Release();
    }
}
