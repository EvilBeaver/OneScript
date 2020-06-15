/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using Xunit;

namespace OneScript.Language.Tests
{
    public static class TreeValidatorExtensions
    {
        public static SyntaxTreeValidator Is(this SyntaxTreeValidator validator, string type)
        {
            Assert.Equal(type, validator.CurrentNode.Type);
            return validator;
        }

        public static TestAstNode FirstChild(this SyntaxTreeValidator validator)
        {
            if (validator.CurrentNode.Children.Count < 0)
            {
                throw new Exception("No more children");
            }

            return validator.CurrentNode.Children[0];
        }
        public static TestAstNode WithNode(this SyntaxTreeValidator validator, string type)
        {
            var child = validator.FirstChild();
            Assert.Equal(type, child.Type);

            return child;
        }
        
        public static SyntaxTreeValidator HasNode(this SyntaxTreeValidator validator, string type)
        {
            var child = validator.CurrentNode.Children.FirstOrDefault(x => x.Type == type);
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
    }
}