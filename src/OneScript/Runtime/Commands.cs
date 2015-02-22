using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public struct Command
    {
        public OperationCode Code;
        public int Argument;
    }

    public enum OperationCode
    {
        Nop,
        PushVar,
        PushConst,
        PushLocal,
        Assign
    }
}
