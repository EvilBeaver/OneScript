using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class MemoryAttachedContext
    {
        public IRuntimeContextInstance ContextInstance { get; set; }
        public IVariable[] State { get; set; }
        public MethodSignatureData[] Methods { get; set; }

        public static MemoryAttachedContext CreateFromContext(IRuntimeContextInstance sourceContext)
        {
            var instance = new MemoryAttachedContext();
            instance.ContextInstance = sourceContext;
            instance.State = new IVariable[sourceContext.GetPropCount()];
            for (int i = 0; i < instance.State.Length; i++)
            {
                instance.State[i] = ReferenceFactory.Create(sourceContext, i);
            }

            instance.Methods = MethodSignatureExtractor.Extract(sourceContext);

            return instance;
        }
    }
}
