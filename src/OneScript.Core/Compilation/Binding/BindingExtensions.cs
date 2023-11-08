/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Language.SyntaxAnalysis.AstNodes;
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

        public static bool IsUniqueMethod(this SymbolTable table, MethodNode astNode)
        {
            var isKnown = table.TryFindMethodBinding(astNode.Signature.MethodName, out _);
            if (!isKnown)
            {
                return true;
            }

            return astNode.Annotations.Any(a => BslOverrideAttribute.AcceptsIdentifier(a.Name));
        }
    }
}
