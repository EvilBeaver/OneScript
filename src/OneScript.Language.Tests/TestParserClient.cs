/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;

namespace OneScript.Language.Tests
{
    public class TestParserClient : IAstBuilder
    {
        public TestAstNode RootNode { get; set; }
        
        private List<TestAstNode> _annotations = new List<TestAstNode>();
        
        public IAstNode CreateNode(NodeKind kind, in Lexem startLexem)
        {
            var node = TestAstNode.New(kind);
            if (kind == NodeKind.Annotation)
            {
                node.Value = startLexem.Content;
                _annotations.Add(node);
            }
            else if (kind == NodeKind.Identifier
                     || kind == NodeKind.AnnotationParameterName 
                     || kind == NodeKind.AnnotationParameterValue
                     || kind == NodeKind.ParameterDefaultValue)
            {
                node.Value = startLexem.Content;
            }
            else
            {
                RootNode ??= node;
            }

            if(kind == NodeKind.Method || kind == NodeKind.VariableDefinition || kind == NodeKind.MethodParameter)
            {
                ApplyAnnotations(node);
            }
            
            return node;
        }

        public void AddChild(IAstNode parent, IAstNode child)
        {
            var cast = (TestAstNode) parent;
            cast.Children.Add((TestAstNode)child);
        }

        public void HandleParseError(in ParseError error, in Lexem lexem, ILexemGenerator lexer)
        {
            //throw new System.NotImplementedException();
        }

        public void PreprocessorDirective(ILexemGenerator lexer, ref Lexem lastExtractedLexem)
        {
            var currentLine = lexer.CurrentLine;
            
            do
            {
                lastExtractedLexem = lexer.NextLexem();
            } while (currentLine == lexer.CurrentLine);
        }

        private void ApplyAnnotations(TestAstNode node)
        {
            foreach (var annotation in _annotations)
            {
                node.Children.Add(annotation);
            }
            _annotations.Clear();
        }
    }
}