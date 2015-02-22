using OneScript.ComponentModel;
using OneScript.Core;

namespace OneScript.Runtime
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
                instance.State[i] = new ContextPropertyVariable(sourceContext, i);
            }

            instance.Methods = MethodSignatureExtractor.Extract(sourceContext);

            return instance;
        }
    }
}
