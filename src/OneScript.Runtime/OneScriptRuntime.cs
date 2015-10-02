using OneScript.Language;
using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class OneScriptRuntime : AbstractScriptRuntime
    {
        private List<InjectedValue> _externalProperties = new List<InjectedValue>();
        private CompilerContext _ctx = new CompilerContext();
        private SymbolScope _topScope;

        public OneScriptRuntime()
        {
            _topScope = new SymbolScope();
            _ctx.PushScope(_topScope);
        }

        public override void InjectSymbol(string name, IValue value)
        {
            _externalProperties.Add(new InjectedValue()
                {
                    Name = name,
                    Object = value
                });
            
            _topScope.DefineVariable(name);
        }

        public override void InjectObject(IRuntimeContextInstance context)
        {
            var scope = SymbolScope.ExtractFromContext(context);

            CheckVariablesConflicts(scope);
            CheckMethodsConflicts(scope);

            _ctx.PushScope(scope);
        }

        private void CheckVariablesConflicts(SymbolScope scope)
        {
            if (scope.VariableCount == 0)
                return;

            foreach (var name in scope.GetVariableSymbols())
            {
                if (_ctx.IsVarDefined(name))
                    throw new ArgumentException("Переменная (" + name + ") уже определена");
            }
        }

        private void CheckMethodsConflicts(SymbolScope scope)
        {
            if (scope.MethodCount == 0)
                return;

            foreach (var name in scope.GetMethodSymbols())
            {
                if (_ctx.IsMethodDefined(name))
                    throw new ArgumentException("Метод (" + name + ") уже определен");
            }
        }

        public override IValue Eval(string expression)
        {
            throw new NotImplementedException();
        }

        public override void Execute(ILoadedModule module)
        {
            throw new NotImplementedException();
        }
    }
}
