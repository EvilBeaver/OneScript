/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Compiler;
using ScriptEngine.Compiler.ByteCode;
using ScriptEngine.Environment;

namespace ScriptEngine
{
    internal class AstBasedCompilerService : CompilerService
    {
        internal AstBasedCompilerService(ICompilerContext outerContext) 
            : base(outerContext)
        {
        }

        protected override ModuleImage CreateImage(ICompilerContext context, ICodeSource source, ILexemGenerator lexer)
        {
            var codeGen = new AstBasedCodeGenerator(context)
            {
                ThrowErrors = true
            };
            var astBuilder = new DefaultAstBuilder()
            {
                ThrowOnError = true
            };
            
            var parser = new DefaultBslParser(astBuilder, lexer);
            var node = (BslSyntaxNode)parser.ParseStatefulModule();
            codeGen.ProduceExtraCode = ProduceExtraCode;
            return codeGen.CreateImage(node, CreateModuleInformation(source, lexer));
        }
    }
}