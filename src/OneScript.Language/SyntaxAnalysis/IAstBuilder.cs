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
        IAstNode CreateAnnotation(string content);
        void AddAnnotationParameter(IAstNode annotation, string id);
        void AddAnnotationParameter(IAstNode annotation, string id, in Lexem lastExtractedLexem);
        void AddAnnotationParameter(IAstNode annotation, in Lexem lastExtractedLexem);
        void CreateVarDefinition(string symbolicName, bool isExported);
        void HandleParseError(ParseError err);
        void StartVariablesSection();
    }
}