/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class RegionDirectiveHandler : IDirectiveHandler
    {
        private readonly LexemTrie<bool> _preprocRegion = new LexemTrie<bool>();
        private readonly LexemTrie<bool> _preprocEndRegion = new LexemTrie<bool>();

        private int _regionsNesting = 0;
        
        public RegionDirectiveHandler()
        {
            _preprocRegion.Add("Область",true);
            _preprocRegion.Add("Region", true);
            _preprocEndRegion.Add("КонецОбласти", true);
            _preprocEndRegion.Add("EndRegion", true);
        }

        public void OnModuleEnter(ILexer lexemStream)
        {
        }

        public void OnModuleLeave(ILexer lexemStream)
        {
            if (_regionsNesting != 0)
                throw new SyntaxErrorException(lexemStream.GetErrorPosition(), "Ожидается завершение директивы препроцессора #Область");
        }

        public BslSyntaxNode HandleDirective(BslSyntaxNode parent, ILexer lexemStream, ref Lexem lastExtractedLexem)
        {
            BslSyntaxNode result = default;
            var directive = lastExtractedLexem;
            if (IsPreprocRegion(directive.Content))
            {
                var regionName = lexemStream.NextLexemOnSameLine();
                if (regionName.Type == LexemType.EndOfText)
                    throw new SyntaxErrorException(lexemStream.GetErrorPosition(), "Ожидается имя области");

                if (!LanguageDef.IsIdentifier(ref directive))
                    throw new SyntaxErrorException(lexemStream.GetErrorPosition(), $"Недопустимое имя Области: {directive.Content}");

                _regionsNesting++;

                lastExtractedLexem = LexemFromNewLine(lexemStream);
                result = parent;
            }
            else if (IsPreprocEndRegion(directive.Content))
            {
                if (_regionsNesting == 0)
                    throw new SyntaxErrorException(lexemStream.GetErrorPosition(), "Пропущена директива препроцессора #Область");

                _regionsNesting--;

                lastExtractedLexem = LexemFromNewLine(lexemStream);
                result = parent;
            }

            return result;
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