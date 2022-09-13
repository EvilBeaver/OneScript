/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Localization;
using OneScript.Values;

namespace OneScript.Compilation
{
    /// <summary>
    /// Программное окружение процесса. Содержит условную "память" виртуальной машины
    /// И предоставляет биндинги символов на реальные объекты
    /// </summary>
    public class RuntimeEnvironment
    {
        private readonly SymbolTable _symbols = new SymbolTable();
        private readonly Lazy<SymbolScope> _scopeOfGlobalProperties;

        private readonly GlobalPropertiesHolder _globalPropertiesHolder = new GlobalPropertiesHolder();

        public RuntimeEnvironment()
        {
            _scopeOfGlobalProperties = new Lazy<SymbolScope>(() =>
            {
                var scope = new SymbolScope();
                _symbols.PushScope(scope, _globalPropertiesHolder);
                return scope;
            });
        }

        public void InjectObject(IContext context)
        {
            _symbols.PushScope(SymbolScope.FromContext(context), context);
        }
        
        public void InjectGlobalProperty(BilingualString names, BslValue value)
        {
            if (_globalPropertiesHolder.HasProperty(names))
            {
                throw new InvalidOperationException($"Global Property {names} already registered");
            }
            
            var propInfo = _globalPropertiesHolder.Register(names, value);
            var symbol = new BslPropertySymbol
            {
                Property = propInfo
            };

            ScopeOfGlobalProps.Variables.Add(symbol, propInfo.Name, propInfo.Alias);
        }

        public void SetGlobalProperty(string name, BslValue value)
        {
            var prop = _globalPropertiesHolder.FindProperty(name);
            if (prop == null)
            {
                throw new ArgumentException($"Property {name} is not registered");
            }
            _globalPropertiesHolder.SetPropValue(prop, value);
        }
        
        public BslValue GetGlobalProperty(string name)
        {
            var prop = _globalPropertiesHolder.FindProperty(name);
            if (prop == null)
            {
                throw new ArgumentException($"Property {name} is not registered");
            }
            
            return _globalPropertiesHolder.GetPropValue(prop);
        }

        private SymbolScope ScopeOfGlobalProps => _scopeOfGlobalProperties.Value;
    }
}