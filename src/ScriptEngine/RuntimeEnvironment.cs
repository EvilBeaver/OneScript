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
using ScriptEngine.Libraries;
using ScriptEngine.Machine;
using SymbolScope = OneScript.Compilation.Binding.SymbolScope;

namespace ScriptEngine
{
    [Obsolete("Use interface IRuntimeEnvironment")]
    public class RuntimeEnvironment : IRuntimeEnvironment, ILibraryManager
    {
        private readonly SymbolTable _symbols = new SymbolTable();
        private SymbolScope _scopeOfGlobalProperties;
        
        private readonly PropertyBag _injectedProperties;

        private readonly List<IAttachableContext> _contexts = new List<IAttachableContext>();

        private readonly ILibraryManager _libraryManager;

        public RuntimeEnvironment()
        {
            _injectedProperties = new PropertyBag();
            _libraryManager = new LibraryManager(_injectedProperties);
        }

        private void CreateGlobalScopeIfNeeded()
        {
            if (_scopeOfGlobalProperties != null) 
                return;
            
            lock (_injectedProperties)
            {
                _scopeOfGlobalProperties ??= _symbols.PushContext(_injectedProperties);
                _contexts.Add(_injectedProperties);
            }
        }

        public void InjectObject(IAttachableContext context)
        {
            RegisterObject(context);
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

        private void RegisterObject(IAttachableContext context)
        {
            _symbols.PushContext(context);
            _contexts.Add(context);
        }
        
        public void SetGlobalProperty(string propertyName, IValue value)
        {
            _symbols.FindVariable(propertyName, out var binding);

            var context = _contexts[binding.ScopeNumber];
            context.SetPropValue(binding.MemberNumber, value);
        }

        public IValue GetGlobalProperty(string propertyName)
        {
            _symbols.FindVariable(propertyName, out var binding);

            var context = _contexts[binding.ScopeNumber];
            return context.GetPropValue(binding.MemberNumber);
        }

        public SymbolTable GetSymbolTable() => _symbols;

        public IReadOnlyCollection<IAttachableContext> AttachedContexts => _contexts;

        public IEnumerable<ExternalLibraryDef> GetLibraries()
        { 
            return _libraryManager.GetLibraries();
        }

        public void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library)
        {
            _libraryManager.InitExternalLibrary(runtime, library);
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
