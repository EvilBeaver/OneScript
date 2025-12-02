/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Contexts;
using OneScript.Runtime.Binding;
using OneScript.Values;

namespace OneScript.Compilation.Binding
{
    public static class BindingExtensions
    {
        /// <summary>
        /// Добавляет в таблицу символов область видимости объекта, который может быть присоединён к рантайму.
        /// </summary>
        /// <param name="table">Таблица символов, в которую нужно добавить область.</param>
        /// <param name="target">Объект, чьи методы/свойства требуется отобразить в символы.</param>
        /// <param name="descriptor">
        /// Пользовательский дескриптор привязки. Если не указан, область считается статической и привязанной к самому объекту.
        /// </param>
        /// <exception cref="ArgumentException">Выбрасывается, если объект не реализует <see cref="IAttachableContext"/>.</exception>
        public static SymbolScope PushObject(this SymbolTable table, BslObjectValue target,
            ScopeBindingDescriptor? descriptor = null)
        {
            if (!(target is IAttachableContext attachable))
                throw new ArgumentException("Target must implement IAttachableContext", nameof(target));

            var scope = SymbolScope.FromObject(target);
            table.PushScope(scope, descriptor ?? ScopeBindingDescriptor.Static(attachable));
            return scope;
        }
        
        public static SymbolScope PushContext(this SymbolTable table, IAttachableContext target,
            ScopeBindingDescriptor? descriptor = null)
        {
            var scope = SymbolScope.FromContext(target);
            table.PushScope(scope, descriptor ?? ScopeBindingDescriptor.Static(target));
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
