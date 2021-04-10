/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Commons;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine
{
    public class RuntimeEnvironment
    {
        private readonly CompilerContext _symbolScopes = new CompilerContext();
        private SymbolScope _globalScope;
        private PropertyBag _injectedProperties;

        private readonly List<AttachedContext> _contexts = new List<AttachedContext>();
        private readonly List<ExternalLibraryDef> _externalLibs = new List<ExternalLibraryDef>();

        public void InjectObject(IAttachableContext context, bool asDynamicScope = false)
        {
            var injectedContext =
                asDynamicScope ? AttachedContext.CreateDynamic(context) : AttachedContext.Create(context);
            
            RegisterObject(injectedContext);
        }

        public void InjectGlobalProperty(IValue value, string identifier, string alias, bool readOnly)
        {
            if(!Utils.IsValidIdentifier(identifier))
            {
                throw new ArgumentException("Invalid identifier", nameof(identifier));
            }

            if (alias != default && !Utils.IsValidIdentifier(alias))
            {
                throw new ArgumentException("Invalid identifier", nameof(alias));
            }

            EnsureGlobalScopeExist();
            
            if(readOnly)
                _globalScope.DefineProperty(identifier, alias);
            else
                _globalScope.DefineVariable(identifier, alias);
            
            _injectedProperties.Insert(value, identifier, true, !readOnly);
        }
        
        public void InjectGlobalProperty(IValue value, string identifier, bool readOnly)
        {
            InjectGlobalProperty(value, identifier, default, readOnly);
        }

        private void EnsureGlobalScopeExist()
        {
            if (_globalScope == null)
            {
                lock (_symbolScopes)
                {
                    if (_globalScope == null)
                    {
                        _injectedProperties = new PropertyBag();
                        var injected = AttachedContext.Create(_injectedProperties);
                        _globalScope = injected.Symbols;
                        RegisterObject(injected);
                    }
                }
            }
        }

        private void RegisterObject(AttachedContext context)
        {
            _symbolScopes.PushScope(context.Symbols);
            _contexts.Add(context);
        }
        
        public void SetGlobalProperty(string propertyName, IValue value)
        {
            var binding = SymbolsContext.GetVariable(propertyName).binding;

            var context = _contexts[binding.ContextIndex];
            context.Instance.SetPropValue(binding.CodeIndex, value);
        }

        public IValue GetGlobalProperty(string propertyName)
        {
            var binding = SymbolsContext.GetVariable(propertyName).binding;

            var context = _contexts[binding.ContextIndex];
            return context.Instance.GetPropValue(binding.CodeIndex);
        }

        internal CompilerContext SymbolsContext => _symbolScopes;

        internal IList<AttachedContext> AttachedContexts => _contexts;

        public IEnumerable<ExternalLibraryDef> GetUserAddedScripts()
        { 
            return _externalLibs.ToArray();
        }

        [Obsolete]
        public void LoadMemory(MachineInstance machine)
        {
            machine.Cleanup();
            foreach (var item in AttachedContexts)
            {
                machine.AttachContext(item.Instance);
            }
            machine.ContextsAttached();
        }

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            var loadedObjects = new ScriptDrivenObject[library.Modules.Count];
            int i = 0;
            foreach (var module in library.Modules)
            {
                var loaded = runtime.LoadModuleImage(module.Image);
                var instance = runtime.CreateUninitializedSDO(loaded);
                
                var propId = _injectedProperties.FindProperty(module.Symbol);
                _injectedProperties.SetPropValue(propId, instance);
                module.InjectOrder = propId;
                loadedObjects[i++] = instance;
            }
            
            _externalLibs.Add(library);
            loadedObjects.ForEach(runtime.InitializeSDO);
        }
    }
}
