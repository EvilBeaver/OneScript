/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public interface IAstBuilder
    {
        IAstNode CreateNode(NodeKind kind, in Lexem startLexem);

        IAstNode AddChild(IAstNode parent, NodeKind kind, in Lexem startLexem);
        
        void AddChild(IAstNode parent, IAstNode child);

        void HandleParseError(in ParseError error, in Lexem lexem, ILexemGenerator lexer);
        void PreprocessorDirective(ILexemGenerator lexer, ref Lexem lastExtractedLexem);
    }
}