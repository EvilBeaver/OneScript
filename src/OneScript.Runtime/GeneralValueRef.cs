using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class GeneralValueRef : IValueRef
    {
        public GeneralValueRef(IValue initial)
        {
            Value = initial;
        }

        public GeneralValueRef()
        {
            Value = ValueFactory.Create();
        }

        public IValue Value
        {
            get;
            set;
        }
    }
}
