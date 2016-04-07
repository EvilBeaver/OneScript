using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime.Compiler
{
    public class CompilerService
    {
        CompilerContext _ctx;
        Preprocessor _lexer;
        OSByteCodeBuilder _builder;

        internal CompilerService(CompilerContext context, Preprocessor lexer, OSByteCodeBuilder parserClient)
        {
            _ctx = context;
            _lexer = lexer;
            _builder = parserClient;
        }

        public CompiledModule CompileModule(IScriptSource src)
        {
            _lexer.Code = src.GetCode();
            var parser = new Parser(_builder);
            parser.ParseModule(_lexer);

            return _builder.GetModule();
        }

        public CompiledModule CompileCodeBatch(IScriptSource src)
        {
            _lexer.Code = src.GetCode();
            var parser = new Parser(_builder);
            parser.ParseCodeBatch(_lexer);

            return _builder.GetModule();
        }

    }
}
