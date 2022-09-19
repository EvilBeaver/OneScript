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
            table.PushScope(scope, target);
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

        private static (IVariableSymbol symbol, SymbolBinding binding) GetKnownVariable(this SymbolTable table, string name)
        {
            if (!table.FindVariable(name, out var binding))
            {
                throw new BindingException(LocalizedErrors.SymbolNotFound(name));
            }

            var variable = table.GetVariable(binding);

            return (variable, binding);
        }
        
        private static (IMethodSymbol symbol, SymbolBinding binding) GetKnownMethod(this SymbolTable table, string name)
        {
            if (!table.FindMethod(name, out var binding))
            {
                throw new BindingException(LocalizedErrors.SymbolNotFound(name));
            }

            var method = table.GetMethod(binding);

            return (method, binding);
        }
    }
}