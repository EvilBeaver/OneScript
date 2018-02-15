using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptEngine.HostedScript.Library
{
    [EnumerationType("ИспользованиеByteOrderMark", "ByteOrderMarkUsage")]
    public enum ByteOrderMarkUsageEnum
    {
        [EnumItem("Авто", "Auto")]
        Auto,

        [EnumItem("Использовать", "Use")]
        Use,

        [EnumItem("НеИспользовать", "DontUse")]
        DontUse
    }
}
