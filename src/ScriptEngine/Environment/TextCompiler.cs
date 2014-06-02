using ScriptEngine.Compiler;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Environment
{
    class TextCompiler
    {
        CompilerContext _context;

        public TextCompiler(CompilerContext context)
        {
            _context = context;
        }

        public ModuleImage Load(string source)
        {
            return CreateModule(source);
        }

        private ModuleImage CreateModule(string source)
        {
            var moduleScope = new SymbolScope();
            const string THIS_PROPERTY = "ЭтотОбъект";

            var thisIdx = moduleScope.DefineVariable(new VariableDescriptor()
            {
                Identifier = THIS_PROPERTY,
                Type = SymbolType.ContextProperty
            });

            _context.PushScope(moduleScope);
            var parser = new Parser();
            parser.Code = source;

            var compiler = new Compiler.Compiler();
            ModuleImage compiledImage;
            try
            {
                compiledImage = compiler.Compile(parser, _context);
                compiledImage.ExportedProperties.Add(new ExportedSymbol()
                    {
                        SymbolicName = THIS_PROPERTY,
                        Index = thisIdx
                    });
            }
            finally
            {
                _context.PopScope();
            }

            return compiledImage;
        }

    }

}
