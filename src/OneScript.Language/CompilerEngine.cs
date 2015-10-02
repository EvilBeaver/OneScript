using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public class CompilerEngine
    {
        private readonly IASTBuilder _builder;
        private ILexemGenerator _lexer;

        public CompilerEngine(IASTBuilder builder)
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
            return parser.ParseModule(_lexer);
        }

        public IASTBuilder GetModuleBuilder()
        {
            return _builder;
        }
    }
}
