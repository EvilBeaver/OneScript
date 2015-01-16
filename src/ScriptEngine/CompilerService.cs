using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
using ScriptEngine.Machine;

namespace ScriptEngine
{
    public class CompilerService
    {
        SymbolScope _scope;
        ModuleCompilerContext _currentContext;
        List<int> _predefinedVariables = new List<int>();

        internal CompilerService(CompilerContext outerContext)
        {
            _currentContext = new ModuleCompilerContext(outerContext);
        }

        public int DefineVariable(string name, SymbolType type)
        {
            RegisterScopeIfNeeded();

            try
            {
                int varIdx;
                if (type == SymbolType.Variable)
                    varIdx = _currentContext.DefineVariable(name).CodeIndex;
                else
                    varIdx = _currentContext.DefineProperty(name).CodeIndex;

                _predefinedVariables.Add(varIdx);
                return varIdx;
            }
            catch
            {
                _currentContext.PopScope();
                _scope = null;
                throw;
            }
        }

        public int DefineMethod(MethodInfo methodInfo)
        {
            RegisterScopeIfNeeded();

            return _currentContext.DefineMethod(methodInfo).CodeIndex;
        }

        private void RegisterScopeIfNeeded()
        {
            if (_scope == null)
            {
                _scope = new SymbolScope();
                _currentContext.PushScope(_scope);
            }
        }

        public ScriptModuleHandle CreateModule(ICodeSource source)
        {
            try
            {
                return Compile(source);
            }
            finally
            {
                _currentContext.PopScope();
                _scope = null;
            }
        }

        private ScriptModuleHandle Compile(ICodeSource source)
        {
            RegisterScopeIfNeeded();

            var parser = new Parser();
            parser.Code = source.Code;

            var compiler = new Compiler.Compiler();
            ModuleImage compiledImage;
            try
            {
                compiledImage = compiler.Compile(parser, _currentContext);
            }
            catch (ScriptException e)
            {
                e.ModuleName = source.SourceDescription;
                throw;
            }

            foreach (var item in _predefinedVariables)
            {
                var varDef = _scope.GetVariable(item);
                if (varDef.Type == SymbolType.ContextProperty)
                {
                    compiledImage.ExportedProperties.Add(new ExportedSymbol()
                    {
                        SymbolicName = varDef.Identifier,
                        Index = varDef.Index
                    });
                }
            }

            var mi = new ModuleInformation();
            mi.CodeIndexer = parser.GetCodeIndexer();
            // пока у модулей нет собственных имен, будет совпадать с источником модуля
            mi.ModuleName = source.SourceDescription;
            mi.Origin = source.SourceDescription;
            compiledImage.ModuleInfo = mi;

            return new ScriptModuleHandle()
            {
                Module = compiledImage
            };
        }
    }
}
