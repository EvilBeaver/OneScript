/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;

namespace ScriptEngine.Compiler
{
    public class AstBasedCompilerService : CompilerServiceBase
    {
        private readonly CompilerOptions _сompilerOptions;

        public AstBasedCompilerService(CompilerOptions сompilerOptions, ICompilerContext outerContext) 
            : base(outerContext)
        {
            _сompilerOptions = сompilerOptions;
        }
        
        protected override ModuleImage CompileInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context)
        {
            var handlers = _сompilerOptions.PreprocessorHandlers;
            var lexer = CreatePreprocessor(source, preprocessorConstants, handlers);
            var mi = CreateModuleInformation(source, lexer);
            var moduleNode = ParseSyntaxConstruction(
                lexer,
                handlers,
                mi,
                p => p.ParseStatefulModule());

            return BuildModule(context, moduleNode, mi);
        }

        protected override ModuleImage CompileBatchInternal(ICodeSource source, IEnumerable<string> preprocessorConstants, ICompilerContext context)
        {
            var handlers = _сompilerOptions.PreprocessorHandlers;
            var lexer = CreatePreprocessor(source, preprocessorConstants, handlers);
            var mi = CreateModuleInformation(source, lexer);
            var moduleNode = ParseSyntaxConstruction(
                lexer,
                handlers,
                mi,
                p => p.ParseCodeBatch());

            return BuildModule(context, moduleNode, mi);
        }

        protected override ModuleImage CompileExpressionInternal(ICodeSource source, ICompilerContext context)
        {
            var handlers = _сompilerOptions.PreprocessorHandlers;
            var lexer = new DefaultLexer
            {
                Iterator = new SourceCodeIterator(source.GetSourceCode())
            };

            var mi = new ModuleInformation
            {
                Origin = "<expression>",
                ModuleName = "<expression>"
            };
            
            var moduleNode = ParseSyntaxConstruction(
                lexer,
                handlers,
                mi,
                p => p.ParseExpression());

            return BuildModule(context, moduleNode, mi);
        }
        
        private ModuleNode ParseSyntaxConstruction(
            ILexer lexer,
            PreprocessorHandlers handlers,
            ModuleInformation mi,
            Func<DefaultBslParser, BslSyntaxNode> action)
        {
            var parser = new DefaultBslParser(
                lexer,
                _сompilerOptions.NodeBuilder,
                _сompilerOptions.ErrorSink,
                handlers);

            ModuleNode moduleNode;
            
            try
            {
                moduleNode = (ModuleNode) action(parser);
            }
            catch (SyntaxErrorException e)
            {
                if (e.ModuleName == default)
                {
                    e.ModuleName = mi.ModuleName;
                }

                throw;
            }

            return moduleNode;
        }

        private ModuleImage BuildModule(ICompilerContext context, ModuleNode moduleNode, ModuleInformation mi)
        {
            var codeGen = GetCodeGenerator(context);
            codeGen.ThrowErrors = true;
            codeGen.ProduceExtraCode = ProduceExtraCode;
            codeGen.DependencyResolver = _сompilerOptions.DependencyResolver;

            return codeGen.CreateImage(moduleNode, mi);
        }

        protected virtual AstBasedCodeGenerator GetCodeGenerator(ICompilerContext context)
        {
            return new AstBasedCodeGenerator(context);
        }

        private PreprocessingLexer CreatePreprocessor(
            ICodeSource source,
            IEnumerable<string> preprocessorConstants,
            PreprocessorHandlers handlers)
        {
            var baseLexer = new DefaultLexer
            {
                Iterator = new SourceCodeIterator(source.GetSourceCode())
            };

            var conditionals = handlers?.Get<ConditionalDirectiveHandler>();
            if (conditionals != default)
            {
                foreach (var constant in preprocessorConstants)
                {
                    conditionals.Define(constant);
                }
            }

            var lexer = new PreprocessingLexer(baseLexer)
            {
                Handlers = handlers,
                ErrorSink = _сompilerOptions.ErrorSink
            };
            return lexer;
        }
    }
}