/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;

namespace OneScript.Language.Tests
{
    public class TestParserClient : IAstBuilder
    {
        public TestAstNode RootNode { get; set; }

        private TestAstNode _currentParent;
        private List<TestAstNode> _annotations = new List<TestAstNode>();
        
        public IAstNode CreateAnnotation(string content)
        {
            var node = new TestAstNode
            {
                Type = "Annotation",
                Value = content
            };
            
            _annotations.Add(node);
            return node;
        }

        public void AddAnnotationParameter(IAstNode annotation, string id)
        {
            var param = new TestAstNode
            {
                Type = "AnnotationParameter", 
                Value = id
            };

            var testAnno = (TestAstNode) annotation;
            testAnno.Children.Add(param);
        }

        public void AddAnnotationParameter(IAstNode annotation, string id, in Lexem lastExtractedLexem)
        {
            var param = new TestAstNode
            {
                Type = "AnnotationParameter", 
                Value = $"{id}={lastExtractedLexem.Content}"
            };

            var testAnno = (TestAstNode) annotation;
            testAnno.Children.Add(param);
        }

        public void AddAnnotationParameter(IAstNode annotation, in Lexem lastExtractedLexem)
        {
            var param = new TestAstNode
            {
                Type = "AnnotationParameter", 
                Value = lastExtractedLexem.Content
            };

            var testAnno = (TestAstNode) annotation;
            testAnno.Children.Add(param);
        }

        public void CreateVarDefinition(string symbolicName, bool isExported)
        {
            var param = new TestAstNode
            {
                Type = "Variable"
            };

            ApplyAnnotations(param);
            param.Children.Add(new TestAstNode
            {
                Type = "Identifier",
                Value = symbolicName
            });
            
            if (isExported)
            {
                param.Children.Add(new TestAstNode
                {
                    Type = "Export"
                });
            }
            
            _currentParent.Children.Add(param);
        }

        private void ApplyAnnotations(TestAstNode node)
        {
            foreach (var annotation in _annotations)
            {
                node.Children.Add(annotation);
            }
            _annotations.Clear();
        }

        public void HandleParseError(ParseError err)
        {
            
        }

        public void StartVariablesSection()
        {
            RootNode = new TestAstNode
            {
                Type = "ModuleVariables"
            };
            _currentParent = RootNode;
        }
    }
}