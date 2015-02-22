using System;
using OneScript.Core;

namespace OneScript.Runtime
{
    public interface IVariable : IEquatable<IValue>
    {
        IValue Value { get; set; }
        IValue Dereference();
    }
}
