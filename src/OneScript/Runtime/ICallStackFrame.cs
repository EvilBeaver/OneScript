using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public interface ICallStackFrame
    {
        ICompiledModule Module { get; }
        int CurrentLine { get; }
        string CurrentMethod { get; } //TODO: возможно, нужен какой-то объект для отладчика?
    }
}
