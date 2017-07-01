using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public struct CodeStatData
    {
        public readonly CodeStatEntry Entry;
        public readonly long TimeElapsed;
        public readonly int ExecutionCount;

        public CodeStatData(CodeStatEntry entry, long time, int count)
        {
            Entry = entry;
            TimeElapsed = time;
            ExecutionCount = count;
        }
    }
}
