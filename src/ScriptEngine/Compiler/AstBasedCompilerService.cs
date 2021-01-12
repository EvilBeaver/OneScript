/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using ScriptEngine.Environment;

namespace ScriptEngine.Compiler
{
    internal class AstBasedCompilerService : CompilerServiceBase
    {
        private readonly CompilerOptions _сompilerOptions;

        internal AstBasedCompilerService(CompilerOptions сompilerOptions, ICompilerContext outerContext) 
            : base(outerContext)
        {
            _сompilerOptions = сompilerOptions;
            ProduceExtraCode = _сompilerOptions.ProduceExtraCode;
        }
        
        protected override ModuleImage CompileInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context)
        {
            var codeGen = new AstBasedCodeGenerator(context)
            {
                ProduceExtraCode = ProduceExtraCode,
                ThrowErrors = true,
                DependencyResolver = _сompilerOptions.DependencyResolver
            };
            
            var astBuilder = new DefaultAstBuilder();

            var handlers = _сompilerOptions.PreprocessorFactory.Create(_сompilerOptions);
            
            var conditionals = handlers.Get<ConditionalDirectiveHandler>();
            if (conditionals != default)
            {
                foreach (var constant in preprocessorConstants)
                {
                    conditionals.Define(constant);
                }
            }
            
            var lexer = new PreprocessingLexer
            {
                Iterator = new SourceCodeIterator(source.Code),
                Handlers = handlers,
                ErrorSink = _сompilerOptions.ErrorSink
            };

            var parseContext = new ParserContext(lexer, astBuilder, handlers);
            var parser = new DefaultBslParser(parseContext);

            ModuleNode moduleNode;
            var mi = CreateModuleInformation(source, lexer);
            try
            {
                moduleNode = (ModuleNode)parser.ParseStatefulModule();
            }
            catch (SyntaxErrorException e)
            {
                if (e.ModuleName == default)
                {
                    e.ModuleName = mi.ModuleName;
                }
                throw;
            }

            return codeGen.CreateImage(moduleNode, mi);
        }
    }
}