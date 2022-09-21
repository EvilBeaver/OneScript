/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Native.Compiler
{
    public class NativeRuntimeAnnotationHandler : ModuleAnnotationDirectiveHandler
    {
        private readonly ILexer _allLineContentLexer;
        
        public NativeRuntimeAnnotationHandler(IErrorSink errorSink) : base(errorSink)
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new WordLexerState());

            _allLineContentLexer = builder.Build();
        }

        public static string NativeDirectiveName => "native";
        
        protected override bool DirectiveSupported(string directive)
        {
            return string.Equals(directive, NativeDirectiveName, StringComparison.CurrentCultureIgnoreCase);
        }

        protected override void ParseAnnotationInternal(ref Lexem lastExtractedLexem, ILexer lexer, ParserContext parserContext)
        {
            var node = new AnnotationNode(NodeKind.Annotation, lastExtractedLexem);
            _allLineContentLexer.Iterator = lexer.Iterator;

            var child = new TerminalNode(NodeKind.Unknown, lastExtractedLexem);
            node.AddChild(child);
            parserContext.AddChild(node);
            
            // после ничего не должно находиться
            var nextLexem = _allLineContentLexer.NextLexemOnSameLine();
            lastExtractedLexem = lexer.NextLexem(); // сдвиг основного лексера
            if (nextLexem.Type != LexemType.EndOfText)
            {
                var err = LocalizedErrors.ExpressionSyntax();
                err.Position = new ErrorPositionInfo
                {
                    LineNumber = node.Location.LineNumber,
                    ColumnNumber = node.Location.ColumnNumber,
                    Code = lexer.Iterator.GetCodeLine(node.Location.LineNumber),
                    ModuleName = lexer.Iterator.Source.Name
                };
                ErrorSink.AddError(err);
            }
        }
    }
}