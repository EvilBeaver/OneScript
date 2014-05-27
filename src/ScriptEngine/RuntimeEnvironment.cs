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
        
        public void InjectObject(IAttachableContext context, ICompilerSymbolsProvider symbols)
        {
            RegisterSymbolScope(symbols);
            RegisterObject(context);
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

        private void RegisterSymbolScope(ICompilerSymbolsProvider provider)
        {
            _symbolScopes.PushScope(new SymbolScope());
            foreach (var item in provider.GetSymbols())
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
