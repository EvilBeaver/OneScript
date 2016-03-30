using OneScript.Language;
using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Runtime.Compiler;

namespace OneScript.Runtime
{
    public class OneScriptRuntime : AbstractScriptRuntime
    {
        private RuntimeValuesHolder _externalProperties = new RuntimeValuesHolder();
        private List<RuntimeScope> _externalContexts = new List<RuntimeScope>();

        private CompilerContext _ctx = new CompilerContext();
        private TypeManager _typeManager;

        public OneScriptRuntime()
        {
            _typeManager = new TypeManager();
            _ctx.PushScope(_externalProperties);
        }

        public override void InjectSymbol(string name, IValue value)
        {
            _externalProperties.DefineVariable(name, value);
        }

        public override void InjectObject(IRuntimeContextInstance context)
        {
            var scope = RuntimeScope.FromContext(context);

            CheckVariablesConflicts(scope);
            CheckMethodsConflicts(scope);

            _ctx.PushScope(scope);
            _externalContexts.Add(scope);
        }

        private void CheckVariablesConflicts(ISymbolScope scope)
        {
            if (scope.VariableCount == 0)
                return;

            foreach (var name in scope.GetVariableSymbols())
            {
                if (_ctx.IsVarDefined(name))
                    throw new ArgumentException("Переменная (" + name + ") уже определена");
            }
        }

        private void CheckMethodsConflicts(ISymbolScope scope)
        {
            if (scope.MethodCount == 0)
                return;

            foreach (var name in scope.GetMethodSymbols())
            {
                if (_ctx.IsMethodDefined(name))
                    throw new ArgumentException("Метод (" + name + ") уже определен");
            }
        }

        public override DataType RegisterType(string name, string alias, DataTypeConstructor constructor = null)
        {
            return _typeManager.RegisterType(name, alias, constructor);
        }

        public override IValue Eval(string expression)
        {
            throw new NotImplementedException();
        }

        public override ILoadedModule Compile(IScriptSource moduleSource)
        {
            var parserClient = new OSByteCodeBuilder();
            parserClient.Context = _ctx;
            var parser = new Parser(parserClient);

            var pp = new Preprocessor();
            pp.Code = moduleSource.GetCode();
            foreach (var item in PreprocessorDirectives)
            {
                pp.Define(item);
            }

            parser.ParseModule(pp);

            return parserClient.GetModule();
        }

        public override void Execute(ILoadedModule module, string entryPointName)
        {
            var engine = new OneScriptEngine(this);
            engine.Execute(module, entryPointName);
        }

        internal TypeManager TypeManager
        {
            get { return _typeManager; }
        }

        public override ISourceCompiler CreateCompiler()
        {
            throw new NotImplementedException();
        }
    }
}
