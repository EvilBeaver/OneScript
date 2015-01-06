using OneScript.Core;

namespace OneScript.Scripting.Runtime
{
    static class ReferenceFactory
    {
        public static IVariable Create(IRuntimeContextInstance context, int propertyIndex)
        {
            return new ContextPropertyVariable(context, propertyIndex);
        }
    }
}
