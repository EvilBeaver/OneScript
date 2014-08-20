using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    public static class Reference
    {
        public static IVariable Create(IVariable var)
        {
            return new SimpleReference(var);
        }

        public static IVariable Create(IRuntimeContextInstance context, int propIndex)
        {
            return new ContextPropertyReference(context, propIndex);
        }
    }
}
