/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class RegionDirectiveHandler : DirectiveHandlerBase
    {
        private readonly LexemTrie<bool> _preprocRegion = new LexemTrie<bool>();
        private readonly LexemTrie<bool> _preprocEndRegion = new LexemTrie<bool>();

        private int _regionsNesting = 0;
        
        public RegionDirectiveHandler(IErrorSink errorSink) : base(errorSink)
        {
            _preprocRegion.Add("Область",true);
            _preprocRegion.Add("Region", true);
            _preprocEndRegion.Add("КонецОбласти", true);
            _preprocEndRegion.Add("EndRegion", true);
        }

        public override void OnModuleLeave()
        {
            if (_regionsNesting != 0)
            {
                AddError(LocalizedErrors.EndOfDirectiveExpected("Область"), new ErrorPositionInfo());
            }
        }

        public override bool HandleDirective(ref Lexem lastExtractedLexem, ILexer lexer)
        {
            var result = false;
            if (IsPreprocRegion(lastExtractedLexem.Content))
            {
                var regionName = lexer.NextLexemOnSameLine();
                if (regionName.Type == LexemType.EndOfText)
                {
                    AddError(LocalizedErrors.RegionNameExpected(), lexer.GetErrorPosition());
                    return true;
                }

                if (!LanguageDef.IsValidIdentifier(regionName.Content))
                {
                    AddError(LocalizedErrors.InvalidRegionName(regionName.Content), lexer.GetErrorPosition());
                    return true;
                }

                _regionsNesting++;

                lastExtractedLexem = LexemFromNewLine(lexer);
                result = true;
            }
            else if (IsPreprocEndRegion(lastExtractedLexem.Content))
            {
                if (_regionsNesting == 0)
                {
                    AddError(LocalizedErrors.DirectiveIsMissing("Область"), lexer.GetErrorPosition());
                    return true;
                }

                _regionsNesting--;

                lastExtractedLexem = LexemFromNewLine(lexer);
                result = true;
            }

            return result;
        }
        
        private void AddError(ParseError err, ErrorPositionInfo position)
        {
            err.Position ??= position;
            ErrorSink.AddError(err);
        }

        private Lexem LexemFromNewLine(ILexer lexer)
        {
            var lex = lexer.NextLexem();

            if (!lexer.Iterator.OnNewLine)
                throw new SyntaxErrorException(lexer.GetErrorPosition(), "Недопустимые символы в директиве");

            return lex;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPreprocRegion(string value)
        {
            return _preprocRegion.TryGetValue(value, out var nodeIsFilled) && nodeIsFilled;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsPreprocEndRegion(string value)
        {
            return _preprocEndRegion.TryGetValue(value, out var nodeIsFilled) && nodeIsFilled;
        }
    }
}