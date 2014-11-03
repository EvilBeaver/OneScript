using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public interface IValue : IEquatable<IValue>, IComparable<IValue>, IRefCountable
    {
        DataType Type { get; }
        decimal AsNumber();
        string AsString();
        DateTime AsDate();
        bool AsBoolean();
        IRuntimeContextInstance AsObject();
    }
}
