using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class CodeInterpreter
    {
        public CodeInterpreter(ICodeBlockFactory factory)
        {
            Factory = factory;
        }

        public ICodeBlockFactory Factory { get; private set; }

        public CompilerContext Context { get; set; }

        public string Code { get; set; }

        public object Run()
        {
            Lexer lexer = new Lexer();
            lexer.Code = Code;
            Factory.Init(Context, lexer);

            var builder = Factory.GetModuleBuilder();
            builder.Build();

            return builder.GetResult();

        }
    }
}
