/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine
{
    public class RuntimeEnvironment
    {
        private readonly List<IAttachableContext> _objects = new List<IAttachableContext>();
        private readonly CompilerContext _symbolScopes = new CompilerContext();
        private SymbolScope _globalScope;
        private PropertyBag _injectedProperties;

        private readonly List<ExternalLibraryDef> _externalLibs = new List<ExternalLibraryDef>();

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

        public IEnumerable<ExternalLibraryDef> GetUserAddedScripts()
        { 
            return _externalLibs.ToArray();
        }

        private void RegisterSymbolScope(IRuntimeContextInstance provider, bool asDynamicScope)
        {
            var scope = new SymbolScope();
            scope.IsDynamicScope = asDynamicScope;
            
            _symbolScopes.PushScope(scope);
            foreach (var item in provider.GetProperties())
            {
                _symbolScopes.DefineVariable(item.Identifier, item.Alias);
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

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            var loadedObjects = new ScriptDrivenObject[library.Modules.Count];
            int i = 0;
            foreach (var module in library.Modules)
            {
                var loaded = runtime.LoadModuleImage(module.Image);
                var instance = runtime.CreateUninitializedSDO(loaded);
                SetGlobalProperty(module.Symbol, instance);
                module.InjectOrder = _injectedProperties.FindProperty(module.Symbol);
                loadedObjects[i++] = instance;
            }
            
            _externalLibs.Add(library);
            loadedObjects.ForEach(runtime.InitializeSDO);
        }
    }
}
