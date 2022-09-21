using OneScript.Contexts;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Runtime.Binding;
using OneScript.Values;

namespace OneScript.Compilation.Binding
{
    public static class BindingExtensions
    {
        public static SymbolScope PushObject(this SymbolTable table, BslObjectValue target)
        {
            var scope = SymbolScope.FromObject(target);
            table.PushScope(scope, (IRuntimeContextInstance)target);
            return scope;
        }
        
        public static SymbolScope PushContext(this SymbolTable table, IRuntimeContextInstance target)
        {
            var scope = SymbolScope.FromContext(target);
            table.PushScope(scope, target);
            return scope;
        }

        public static IMethodSymbol ToSymbol(this BslMethodInfo info)
        {
            return new BslMethodSymbol { Method = info };
        }
        
        public static IPropertySymbol ToSymbol(this BslPropertyInfo info)
        {
            return new BslPropertySymbol { Property = info };
        }
        
        public static IFieldSymbol ToSymbol(this BslFieldInfo info)
        {
            return new BslFieldSymbol { Field = info };
        }
    }
}