/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using Xunit.Sdk;

namespace OneScript.Language.Tests
{
    public class SyntaxTreeValidator
    {
        private int _currentChildIndex = 0;
        
        public SyntaxTreeValidator(TestAstNode node)
        {
            CurrentNode = node;
        }

        public TestAstNode CurrentNode { get; set; }

        public SyntaxTreeValidator NextChild()
        {
            if (_currentChildIndex >= CurrentNode.Children.Count)
            {
                throw new Exception("No more children");
            }
            
            return new SyntaxTreeValidator(CurrentNode.Children[_currentChildIndex++]);
        }
    }
}