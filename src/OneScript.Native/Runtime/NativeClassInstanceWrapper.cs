using OneScript.Contexts;

namespace OneScript.Native.Runtime
{
    internal class NativeClassInstanceWrapper
    {
        public IAttachableContext Context { get; set; }
        
        public IVariable[] State { get; set; }
        
        public BslMethodInfo[] Methods { get; set; }
    }
}