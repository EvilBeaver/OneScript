using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Core
{
    static class ReferenceFactory
    {
        public static IVariable Create(IRuntimeContextInstance context, int propertyIndex)
        {
            return new ContextPropertyReference(context, propertyIndex);
        }
    }
}
