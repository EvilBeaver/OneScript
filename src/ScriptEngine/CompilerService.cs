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
        CompilerContext _currentContext;
        List<int> _predefinedVariables = new List<int>();

        internal CompilerService(CompilerContext context)
        {
            _currentContext = context;
        }

        public int AddVariable(string name, SymbolType type)
        {
            if (_scope == null)
            {
                _scope = new SymbolScope();
                _currentContext.PushScope(_scope);
            }

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

        public ModuleHandle CreateModule(ICodeSource source)
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

        private ModuleHandle Compile(ICodeSource source)
        {
            var parser = new Parser();
            parser.Code = source.Code;

            var compiler = new Compiler.Compiler();
            ModuleImage compiledImage;
            compiledImage = compiler.Compile(parser, _currentContext);
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

            return new ModuleHandle()
            {
                Module = compiledImage
            };
        }
    }
}
