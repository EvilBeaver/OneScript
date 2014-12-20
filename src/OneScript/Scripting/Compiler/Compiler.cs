using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Scripting.Compiler.Lexics;

namespace OneScript.Scripting.Compiler
{
    public class Compiler
    {
        private readonly IModuleBuilder _builder;
        private Lexer _lexer;

        public Compiler(IModuleBuilder builder)
        {
            _builder = builder;
        }

        public void SetCode(string code)
        {
            _lexer = new Lexer {Code = code};
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
