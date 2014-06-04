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
        private List<IAttachableContext> _objects = new List<IAttachableContext>();
        private CompilerContext _symbolScopes = new CompilerContext();
        private SymbolScope _globalScope;
        private PropertyBag _injectedProperties;

        public void InjectObject(IAttachableContext context)
        {
            RegisterSymbolScope(context);
            RegisterObject(context);
        }

        public void InjectGlobalProperty(IValue value, string identifier, bool readOnly)
        {
            if (_globalScope == null)
            {
                _globalScope = new SymbolScope();
                _injectedProperties = new PropertyBag();
                _symbolScopes.PushScope(_globalScope);
                RegisterObject(_injectedProperties);
            }
            
            _globalScope.DefineVariable(identifier, SymbolType.ContextProperty);
            _injectedProperties.Insert(value, identifier, true, !readOnly);
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

        private void RegisterSymbolScope(IReflectableContext provider)
        {
            _symbolScopes.PushScope(new SymbolScope());
            foreach (var item in provider.GetProperties())
            {
                if (item.Type == SymbolType.Variable)
                {
                    _symbolScopes.DefineVariable(item.Identifier);
                }
                else
                {
                    _symbolScopes.DefineProperty(item.Identifier);
                }
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


    }
}
