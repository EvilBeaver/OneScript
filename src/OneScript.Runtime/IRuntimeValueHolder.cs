using OneScript.Core;
using System;
namespace OneScript.Runtime
{
    public interface IRuntimeValueHolder
    {
        IValue ValueOf(int index);
        IValue ValueOf(string name);
        IValueRef[] ValueRefs { get; }
    }
}
