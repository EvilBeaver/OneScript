using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public interface IValue
    {
        DataType Type { get; }
        double AsNumber();
        string AsString();
        DateTime AsDate();
        bool AsBoolean();
        IRuntimeContextInstance AsObject();
    }
}
