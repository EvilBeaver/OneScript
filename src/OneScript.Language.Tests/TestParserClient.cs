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
            else if (kind == NodeKind.Identifier)
            {
                node.Value = startLexem.Content;
            }
            else
            {
                RootNode ??= node;                
            }

            return node;
        }

        public IAstNode AddChild(IAstNode parent, NodeKind kind, in Lexem startLexem)
        {
            var cast = (TestAstNode) parent;
            var child = TestAstNode.New(kind);
            bool checkAnnotations = true;
            if (kind == NodeKind.Identifier 
                || kind == NodeKind.AnnotationParameterName 
                || kind == NodeKind.AnnotationParameterValue
                || kind == NodeKind.ParameterDefaultValue)
            {
                child.Value = startLexem.Content;
                checkAnnotations = false;
            }
            else if (kind == NodeKind.AnnotationParameter)
            {
                checkAnnotations = false;
            }

            if (kind == NodeKind.Annotation)
            {
                child.Value = startLexem.Content;
                _annotations.Add(child);
            }
            else if(kind == NodeKind.Method || kind == NodeKind.VariableDefinition || kind == NodeKind.MethodParameter)
            {
                ApplyAnnotations(child);
            }
            else if(checkAnnotations && _annotations.Count > 0)
                throw new Exception($"Node {kind} cannot have annotations");
            
            cast.Children.Add(child);
            return child;
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