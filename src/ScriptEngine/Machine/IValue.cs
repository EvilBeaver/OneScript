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
        
        double AsNumber();
        DateTime AsDate();
        bool AsBoolean();
        string AsString();
        TypeDescriptor AsType();
        IRuntimeContextInstance AsObject();
        IValue GetRawValue();
        
    }

}