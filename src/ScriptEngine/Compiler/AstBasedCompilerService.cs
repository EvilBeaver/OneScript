/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Environment;

namespace ScriptEngine.Compiler
{
    internal class AstBasedCompilerService : CompilerServiceBase
    {
        private readonly CompilerBuildOptions _сompilerOptions;

        internal AstBasedCompilerService(CompilerBuildOptions сompilerOptions, ICompilerContext outerContext) 
            : base(outerContext)
        {
            _сompilerOptions = сompilerOptions;
            ProduceExtraCode = _сompilerOptions.ProduceExtraCode;
        }

        private DefaultBslParser Parser { get; set; }
        
        
        
        protected override ModuleImage CompileInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context)
        {
            var codeGen = new AstBasedCodeGenerator(context)
            {
                ProduceExtraCode = ProduceExtraCode,
                ThrowErrors = true,
                DependencyResolver = _сompilerOptions.DependencyResolver
            };
            
            var astBuilder = new DefaultAstBuilder()
            {
                ThrowOnError = true
            };

            var lexer = new DefaultLexer
            {
                Code = source.Code
            };

            var conditionals = _сompilerOptions.PreprocessorHandlers.Get<ConditionalDirectiveHandler>();
            if (conditionals != default)
            {
                foreach (var constant in preprocessorConstants)
                {
                    conditionals.Define(constant);
                }
            }

            Parser = new DefaultBslParser(astBuilder, lexer, _сompilerOptions.PreprocessorHandlers);
            
            var moduleNode = (ModuleNode)Parser.ParseStatefulModule();
            var mi = CreateModuleInformation(source, lexer);

            return codeGen.CreateImage(moduleNode, mi);
        }
    }
}