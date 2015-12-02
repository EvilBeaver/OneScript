using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public interface IVariable
    {
        IValue Value { get; set; }
    }
}
