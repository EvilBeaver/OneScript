using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    public interface IValue : IComparable<IValue>, IEquatable<IValue>
    {
        DataType DataType { get; }
        TypeDescriptor SystemType { get; }
        
        decimal AsNumber();
        DateTime AsDate();
        bool AsBoolean();
        string AsString();
        IRuntimeContextInstance AsObject();
        IValue GetRawValue();
        
    }

}