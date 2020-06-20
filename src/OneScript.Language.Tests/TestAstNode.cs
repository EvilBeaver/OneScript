/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.SyntaxAnalysis;

namespace OneScript.Language.Tests
{
    public class TestAstNode : IAstNode
    {
        public string Type { get; set; }
        
        public string Value { get; set; }
        
        public List<TestAstNode> Children { get; set; } = new List<TestAstNode>();

        public static TestAstNode New(NodeKind kind)
        {
            return new TestAstNode
            {
                Type = kind.ToString(),
                Kind = (int)kind
            };
        }

        public override string ToString()
        {
            return Type + (Value != default? $": {Value}" : "");
        }

        public int Kind { get; set; }
    }
}