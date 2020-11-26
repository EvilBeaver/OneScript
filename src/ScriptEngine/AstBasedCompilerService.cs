/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Compiler;
using ScriptEngine.Compiler.ByteCode;
using ScriptEngine.Environment;

namespace ScriptEngine
{
    internal class AstBasedCompilerService : CompilerServiceBase
    {
        internal AstBasedCompilerService(ICompilerContext outerContext) 
            : base(outerContext)
        {
        }

        private DefaultBslParser Parser { get; set; }
        
        protected override ModuleImage CompileInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context)
        {
            var codeGen = new AstBasedCodeGenerator(context)
            {
                ProduceExtraCode = ProduceExtraCode,
                ThrowErrors = true
            };
            
            var astBuilder = new DefaultAstBuilder()
            {
                ThrowOnError = true
            };

            var lexer = new DefaultLexer()
            {
                Code = source.Code
            };

            Parser = new DefaultBslParser(astBuilder, lexer);
            
            var conditionalCompilation = new ConditionalDirectiveHandler();
            foreach (var constant in preprocessorConstants)
            {
                conditionalCompilation.Define(constant);
            }
            
            Parser.DirectiveHandlers.Add(new RegionDirectiveHandler());
            Parser.DirectiveHandlers.Add(conditionalCompilation);
            
            var moduleNode = Parser.ParseStatefulModule();
            var mi = CreateModuleInformation(source, lexer);

            return codeGen.CreateImage(moduleNode, mi);
        }
    }
}