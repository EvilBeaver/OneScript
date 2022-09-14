using System;
using ScriptEngine.Machine;

namespace OneScript.Values
{
    public interface IValueReference : IEquatable<IValueReference>
    {
        IValue Value { get; set; }
    }
}