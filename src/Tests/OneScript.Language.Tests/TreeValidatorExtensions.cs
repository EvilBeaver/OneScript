/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using FluentAssertions;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using Xunit;

namespace OneScript.Language.Tests
{
    public static class TreeValidatorExtensions
    {
        public static SyntaxTreeValidator Is(this SyntaxTreeValidator validator, string type)
        {
            Assert.Equal(type, validator.CurrentNode.Kind.ToString());
            return validator;
        }
        
        public static SyntaxTreeValidator Is(this SyntaxTreeValidator validator, NodeKind type)
        {
            validator.CurrentNode.Kind.Should().Be(type);
            return validator;
        }

        public static SyntaxTreeValidator DownTo(this SyntaxTreeValidator validator, NodeKind type)
        {
            var node = DownTo(validator.CurrentNode, type);
            if (node == null)
            {
                throw new Exception("No such child: " + type);
            }

            return new SyntaxTreeValidator(node);
        }
        
        private static TestAstNode DownTo(this TestAstNode node, NodeKind type)
        {
            if (node.Kind == type)
                return node;

            if (node.Children.Count == 0)
                return null;
            
            foreach (var childNode in node.ChildrenList)
            {
                if (childNode.Kind == type)
                    return childNode;

                if (childNode.ChildrenList.Count == 0)
                    continue;
                
                var result = DownTo(childNode, type);
                if (result != null)
                    return result;
            }

            return null;
        }
        
        public static void Is(this TestAstNode node, NodeKind type)
        {
            node.Kind.Should().Be(type, node.Kind + " is unexpected");
        }
        
        public static SyntaxTreeValidator HasChildNodes(this SyntaxTreeValidator validator, int count)
        {
            Assert.Equal(count, validator.CurrentNode.ChildrenList.Count);
            return validator;
        }
        
        public static SyntaxTreeValidator HasChildNodes(this SyntaxTreeValidator validator)
        {
            Assert.NotEmpty(validator.CurrentNode.ChildrenList);
            return validator;
        }
        
        public static TestAstNode FirstChild(this SyntaxTreeValidator validator)
        {
            if (validator.CurrentNode.ChildrenList.Count < 0)
            {
                throw new Exception("No more children");
            }

            return validator.CurrentNode.ChildrenList[0];
        }

        public static TestAstNode WithNode(this SyntaxTreeValidator validator, NodeKind type)
        {
            var child = validator.FirstChild();
            Assert.Equal(type, child.Kind);

            return child;
        }
        
        public static SyntaxTreeValidator HasNode(this SyntaxTreeValidator validator, NodeKind type)
        {
            var child = validator.CurrentNode.ChildrenList.FirstOrDefault(x => x.Kind == type);
            Assert.NotNull(child);

            return new SyntaxTreeValidator(child);
        }
        
        public static void Equal(this TestAstNode node, string value)
        {
            Assert.Equal(value, node.Value);
        }
        
        public static void Equal(this SyntaxTreeValidator node, string value)
        {
            Assert.Equal(value, node.CurrentNode.Value);
        }

        public static T CastTo<T>(this SyntaxTreeValidator node) where T : BslSyntaxNode
        {
            return (T) node.CurrentNode.RealNode;
        }
    }
}