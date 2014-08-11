using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public interface IVariable : IValue
    {
        IValue Value { get; set; }
        IValue Dereference();
    }
}
