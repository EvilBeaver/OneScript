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

        private DefaultBslParser Parser { get; set; }
        
        protected override ModuleImage CreateImage(ICompilerContext context, ICodeSource source, ILexemGenerator lexer)
        {
            var codeGen = new AstBasedCodeGenerator(context)
            {
                ThrowErrors = true,
                DirectiveResolver = DirectiveResolver,
                ProduceExtraCode = ProduceExtraCode
            };
            var astBuilder = new DefaultAstBuilder()
            {
                ThrowOnError = true
            };
            
            Parser = new DefaultBslParser(astBuilder, lexer);
            var node = Parser.ParseStatefulModule();
            return codeGen.CreateImage(node, CreateModuleInformation(source, lexer));
        }

        protected override bool ResolveDirective(ILexemGenerator lexer, ref Lexem lexem)
        {
            var (name, value) = ParsePreprocessorDirective(lexer, ref lexem);
            if (DirectiveResolver != default)
            {
                return DirectiveResolver.Resolve(name, value, Parser.ParsingContext.Count > 1);
            }

            return false;
        }
        
        private (string directiveName, string value) ParsePreprocessorDirective(ILexemGenerator lexer, ref Lexem lastExtractedLexem)
        {
            var directiveName = lastExtractedLexem.Content;
            ReadToLineEnd(lexer);
            
            var value = lexer.Iterator.GetContents().TrimEnd();
            if (string.IsNullOrEmpty(value))
            {
                value = default;
            }

            lastExtractedLexem = lexer.NextLexem();

            return (directiveName, value);
        }
        
        private void ReadToLineEnd(ILexemGenerator lexer)
        {
            char cs;
            do
            {
                cs = lexer.Iterator.CurrentSymbol;
            } while (cs != '\n' && lexer.Iterator.MoveNext());
        }
    }
}