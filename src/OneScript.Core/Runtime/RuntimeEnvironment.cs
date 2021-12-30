/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Runtime.Binding;
using OneScript.Values;

namespace OneScript.Runtime
{
    /// <summary>
    /// Программное окружение процесса. Содержит условную "память" виртуальной машины
    /// И предоставляет биндинги символов на реальные объекты
    /// </summary>
    public class RuntimeEnvironment
    {
        private readonly SymbolTable _symbols = new SymbolTable();
        private readonly Lazy<SymbolScope> _scopeOfGlobalProperties;

        public RuntimeEnvironment()
        {
            _scopeOfGlobalProperties = new Lazy<SymbolScope>(() =>
            {
                var scope = new SymbolScope();
                _symbols.PushScope(scope);
                return scope;
            });
        }

        public void InjectObject(IContext context)
        {
            _symbols.PushScope(SymbolScope.FromContext(context));
        }
        
        private SymbolScope ScopeOfGlobalProps => _scopeOfGlobalProperties.Value;
    }
}