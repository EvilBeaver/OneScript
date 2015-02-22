using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Compiler.Lexics;

namespace OneScript.Compiler
{
    public class CompilerEngine
    {
        private readonly IModuleBuilder _builder;
        private ILexemGenerator _lexer;

        public CompilerEngine(IModuleBuilder builder)
        {
            _builder = builder;
        }

        public void SetCode(string code)
        {
            _lexer = new Preprocessor() {Code = code};
        }

        public bool Compile()
        {
            var parser = new Parser(_builder);
            return parser.Build(_lexer);
        }

        public IModuleBuilder GetModuleBuilder()
        {
            return _builder;
        }
    }
}
