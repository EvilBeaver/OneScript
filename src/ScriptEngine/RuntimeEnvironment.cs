/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using SymbolScope = OneScript.Compilation.Binding.SymbolScope;

namespace ScriptEngine
{
    public class RuntimeEnvironment
    {
        private readonly SymbolTable _symbols = new SymbolTable();
        private SymbolScope _scopeOfGlobalProperties;

//***        
        //private readonly ICompilerContext _symbolScopes = new CompilerContext();
        //private SymbolScope _globalScope;
        private readonly PropertyBag _injectedProperties;

        private readonly List<AttachedContext> _contexts = new List<AttachedContext>();

        private readonly List<ExternalLibraryDef> _externalLibs = new List<ExternalLibraryDef>();
//***

        public RuntimeEnvironment()
        {
            _injectedProperties = new PropertyBag();
        }

        private void CreateGlobalScopeIfNeeded()
        {
            if (_scopeOfGlobalProperties != null) 
                return;
            
            lock (_injectedProperties)
            {
                _scopeOfGlobalProperties ??= _symbols.PushContext(_injectedProperties);
                _contexts.Add(AttachedContext.Create(_injectedProperties));
            }
        }

        public void InjectObject(IAttachableContext context)
        {
            // по факту DynamicScope нигде не пригодился, надо спилить
            InjectObject(context, false);
        }

        private void InjectObject(IAttachableContext context, bool asDynamicScope)
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
            CreateGlobalScopeIfNeeded();
            var num = _injectedProperties.Insert(value, identifier, true, !readOnly);

            var symbol = new WrappedPropertySymbol(_injectedProperties.GetPropertyInfo(num))
            {
                Name = identifier,
                Alias = alias
            };

            _scopeOfGlobalProperties.DefineVariable(symbol);
        }
        
        public void InjectGlobalProperty(IValue value, string identifier, bool readOnly)
        {
            InjectGlobalProperty(value, identifier, default, readOnly);
        }

        private void RegisterObject(AttachedContext context)
        {
            _symbols.PushContext(context.Instance);
            _contexts.Add(context);
        }
        
        public void SetGlobalProperty(string propertyName, IValue value)
        {
            _symbols.FindVariable(propertyName, out var binding);

            var context = _contexts[binding.ScopeNumber];
            context.Instance.SetPropValue(binding.MemberNumber, value);
        }

        public IValue GetGlobalProperty(string propertyName)
        {
            _symbols.FindVariable(propertyName, out var binding);

            var context = _contexts[binding.ScopeNumber];
            return context.Instance.GetPropValue(binding.MemberNumber);
        }

        internal SymbolTable Symbols => _symbols;

        internal IList<AttachedContext> AttachedContexts => _contexts;

        public IEnumerable<ExternalLibraryDef> GetLibraries()
        { 
            return _externalLibs.ToArray();
        }

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            var loadedObjects = new ScriptDrivenObject[library.Modules.Count];
            int i = 0;
            foreach (var module in library.Modules)
            {
                var instance = runtime.CreateUninitializedSDO(module.Module);
                
                var propId = _injectedProperties.GetPropertyNumber(module.Symbol);
                _injectedProperties.SetPropValue(propId, instance);
                module.InjectOrder = propId;
                loadedObjects[i++] = instance;
            }
            
            _externalLibs.Add(library);
            loadedObjects.ForEach(runtime.InitializeSDO);
        }

        private class WrappedPropertySymbol : IPropertySymbol
        {
            public WrappedPropertySymbol(BslPropertyInfo propInfo)
            {
                Property = propInfo;
            }

            public string Name { get; set; }
            public string Alias { get; set; }
            public Type Type => Property.PropertyType;
            public BslPropertyInfo Property { get; }
        }
    }
}
