using ScriptEngine.Compiler;
using ScriptEngine.Machine.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine
{
    class ScriptLoader
    {
        ICompilerSymbolsProvider _globalCtx;

        public ScriptLoader(ICompilerSymbolsProvider globalCtx)
        {
            _globalCtx = globalCtx;
        }

        public ModuleImage Load(string source)
        {
            var context = new CompilerContext();

            RegisterGlobalContext(context);

            return CreateModule(context, source);
            
        }

        private ModuleImage CreateModule(CompilerContext ctx, string source)
        {
            ctx.PushScope(new SymbolScope());
            var parser = new Parser();
            parser.Code = source;

            var compiler = new Compiler.Compiler();
            try
            {
                return compiler.Compile(parser, ctx);
            }
            catch
            {
                ctx.PopScope();
                throw;
            }
        }

        private void RegisterGlobalContext(CompilerContext context)
        {
            var provider = _globalCtx;

            context.PushScope(new SymbolScope());
            foreach (var item in provider.GetSymbols())
	        {
                if (item.Type == SymbolType.Variable)
                {
                    context.DefineVariable(item.Identifier);
                }
                else
                {
                    context.DefineProperty(item.Identifier);
                }
	        }

            foreach (var item in provider.GetMethods())
            {
                context.DefineMethod(item);
            }
        }

    }

}
