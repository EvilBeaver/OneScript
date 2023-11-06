/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language.SyntaxAnalysis;
using Xunit;

namespace OneScript.Language.Tests
{
    public class SyntaxTreeValidator
    {
        private int _currentChildIndex = -1;
        
        public SyntaxTreeValidator(TestAstNode node)
        {
            CurrentNode = node;
        }

        public TestAstNode CurrentNode { get; set; }

        public SyntaxTreeValidator NextChild()
        {
            MoveToNextChild();

            return new SyntaxTreeValidator(CurrentNode.ChildrenList[_currentChildIndex]);
        }

        private void MoveToNextChild()
        {
            _currentChildIndex++;
            EnsureHasCurrentChild();
        }
        
        public SyntaxTreeValidator NextChildIs(NodeKind nodeType)
        {
            MoveToNextChild();

            var child = CurrentNode.ChildrenList[_currentChildIndex];
            child.Is(nodeType);
            return this;
        }

        public SyntaxTreeValidator DownOneLevel()
        {
            EnsureHasCurrentChild();
            return new SyntaxTreeValidator(CurrentNode.ChildrenList[_currentChildIndex]);
        }

        private void EnsureHasCurrentChild()
        {
            if(_currentChildIndex ==-1)
                MoveToNextChild();
            
            if (_currentChildIndex >= CurrentNode.ChildrenList.Count)
            {
                throw new Exception("No more children");
            }
        }

        public TestAstNode ChildItself()
        {
            EnsureHasCurrentChild();
            return CurrentNode.ChildrenList[_currentChildIndex];
        }
        
        public void NoMoreChildren()
        {
            if (_currentChildIndex == -1)
            {
                Assert.Empty(CurrentNode.ChildrenList);
            }
            else
            {
                _currentChildIndex++;
                Assert.True(_currentChildIndex >= CurrentNode.ChildrenList.Count, "should not have more children");
            }
        }
    }
}