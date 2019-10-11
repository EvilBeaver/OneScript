/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    public class RuntimeEnvironment
    {
        private readonly List<IAttachableContext> _objects = new List<IAttachableContext>();
        private readonly CompilerContext _symbolScopes = new CompilerContext();
        private SymbolScope _globalScope;
        private PropertyBag _injectedProperties;

        private readonly List<UserAddedScript> _externalScripts = new List<UserAddedScript>();

        public void InjectObject(IAttachableContext context)
        {
            InjectObject(context, false);
        }

        public void InjectObject(IAttachableContext context, bool asDynamicScope)
        {
            RegisterSymbolScope(context, asDynamicScope);
            RegisterObject(context);
        }

        public void InjectGlobalProperty(IValue value, string identifier, bool readOnly)
        {
            if(!Utils.IsValidIdentifier(identifier))
            {
                throw new ArgumentException("Invalid identifier", "identifier");
            }

            if (_globalScope == null)
            {
                _globalScope = new SymbolScope();
                TypeManager.RegisterType("__globalPropertiesHolder", typeof(PropertyBag));
                _injectedProperties = new PropertyBag();
                _symbolScopes.PushScope(_globalScope);
                RegisterObject(_injectedProperties);
            }
            
            _globalScope.DefineVariable(identifier, SymbolType.ContextProperty);
            _injectedProperties.Insert(value, identifier, true, !readOnly);
        }

        public void SetGlobalProperty(string propertyName, IValue value)
        {
            int propId = _injectedProperties.FindProperty(propertyName);
            _injectedProperties.SetPropValue(propId, value);
        }

        public IValue GetGlobalProperty(string propertyName)
        {
            int propId = _injectedProperties.FindProperty(propertyName);
            return _injectedProperties.GetPropValue(propId);
        }

        internal CompilerContext SymbolsContext
        {
            get
            {
                return _symbolScopes;
            }
        }

        internal IList<IAttachableContext> AttachedContexts
        {
            get
            {
                return _objects;
            }
        }

        [Obsolete]
        public void NotifyClassAdded(ScriptModuleHandle module, string symbol)
        {
            NotifyClassAdded(module.Module, symbol);
        }

        [Obsolete]
        public void NotifyModuleAdded(ScriptModuleHandle module, string symbol)
        {
            NotifyModuleAdded(module.Module, symbol);
        }

        public void NotifyClassAdded(ModuleImage module, string symbol)
        {
            _externalScripts.Add(new UserAddedScript()
                {
                    Type = UserAddedScriptType.Class,
                    Symbol = symbol,
                    Image = module
                });
        }
        
        public void NotifyModuleAdded(ModuleImage module, string symbol)
        {
            var script = new UserAddedScript()
            {
                Type = UserAddedScriptType.Module,
                Symbol = symbol,
                Image = module
            };

            _externalScripts.Add(script);
            SetGlobalProperty(script.Symbol, null);
        }
        
        public IEnumerable<UserAddedScript> GetUserAddedScripts()
        {
            // Костыль. Чтобы скомпилированный EXE загружал модули в правильном порядке,
            // упорядочиваем список в том порядке, в котором добавлялись свойства.
            return _externalScripts.OrderBy(script =>
            {
                try
                {
                    return _injectedProperties.FindProperty(script.Symbol);
                }
                catch
                {
                    return 0;
                }
            });
        }

        private void RegisterSymbolScope(IRuntimeContextInstance provider, bool asDynamicScope)
        {
            var scope = new SymbolScope();
            scope.IsDynamicScope = asDynamicScope;
            
            _symbolScopes.PushScope(scope);
            foreach (var item in provider.GetProperties())
            {
                _symbolScopes.DefineVariable(item.Identifier);
            }

            foreach (var item in provider.GetMethods())
            {
                _symbolScopes.DefineMethod(item);
            }
        }

        private void RegisterObject(IAttachableContext context)
        {
            _objects.Add(context);
        }

        public void LoadMemory(MachineInstance machine)
        {
            machine.Cleanup();
            foreach (var item in AttachedContexts)
            {
                machine.AttachContext(item);
            }
            machine.ContextsAttached();
        }
    }

    [Serializable]
    public struct UserAddedScript
    {
        public UserAddedScriptType Type;
        public ModuleImage Image;
        public string Symbol;

        [Obsolete]
        public ScriptModuleHandle Module {
            get => new ScriptModuleHandle() {Module = Image}; 
            set => Image = value.Module;
        }
    }

    public enum UserAddedScriptType
    {
        Module,
        Class
    }
}
