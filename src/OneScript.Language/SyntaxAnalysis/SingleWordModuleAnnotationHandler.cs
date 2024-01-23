/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Localization;

namespace OneScript.Language.SyntaxAnalysis
{
    /// <summary>
    /// Обработчик аннотаций модуля, состоящих из одного слова
    /// </summary>
    public class SingleWordModuleAnnotationHandler : ModuleAnnotationDirectiveHandler
    {
        private readonly ILexer _allLineContentLexer;
        private readonly ISet<string> _knownNames = new HashSet<string>(StringComparer.CurrentCultureIgnoreCase);

        public SingleWordModuleAnnotationHandler(ISet<string> knownNames, IErrorSink errorSink) : base(errorSink)
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new WordLexerState());

            _allLineContentLexer = builder.Build();
            _knownNames = knownNames;
        }
        
        public SingleWordModuleAnnotationHandler(ISet<BilingualString> knownNames, IErrorSink errorSink) : base(errorSink)
        {
            var builder = new LexerBuilder();
            builder.Detect((cs, i) => !char.IsWhiteSpace(cs))
                .HandleWith(new WordLexerState());

            _allLineContentLexer = builder.Build();

            foreach (var twoNames in knownNames)
            {
                _knownNames.Add(twoNames.Russian);
                _knownNames.Add(twoNames.English);
            }
        }

        protected override bool DirectiveSupported(string directive)
        {
            return _knownNames.Contains(directive);
        }

        protected override void ParseAnnotationInternal(ref Lexem lastExtractedLexem, ILexer lexer, ParserContext parserContext)
        {
            var node = new AnnotationNode(NodeKind.Annotation, lastExtractedLexem);
            _allLineContentLexer.Iterator = lexer.Iterator;
            
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