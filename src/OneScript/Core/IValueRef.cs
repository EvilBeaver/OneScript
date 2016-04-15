using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public interface IValueRef
    {
        IValue Value { get; set; }
    }
}
