/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Language.LexicalAnalysis
{
    public abstract class LexerState
    {
        abstract public Lexem ReadNextLexem(SourceCodeIterator iterator);
        
        public SyntaxErrorException CreateExceptionOnCurrentLine(string message, SourceCodeIterator iterator)
        {
            var cp = iterator.GetPositionInfo();
            return new SyntaxErrorException(cp, message);
        }
    }

    internal class EmptyLexerState : LexerState
    {
        public override Lexem ReadNextLexem(SourceCodeIterator iterator)
        {
            return Lexem.Empty();
        }
    }

}
