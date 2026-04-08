/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace ScriptEngine.Compiler
{
    public partial class StackMachineCodeGenerator
    {
        private const int DUMMY_ADDRESS = -1;
        
        private struct ForwardedMethodDecl
        {
            public string identifier;
            public BslSyntaxNode factArguments;
            public bool asFunction;
            public CodeRange location;
            public int commandIndex;
        }
        
        private class NestedLoopInfo
        {
            private NestedLoopInfo(){}
            
            public static NestedLoopInfo New()
            {
                return new NestedLoopInfo()
                {
                    startPoint = DUMMY_ADDRESS,
                    breakStatements = new List<int>(),
                    tryNesting = 0
                };
            }

            public int startPoint;
            public List<int> breakStatements;
            public int tryNesting;
        }

        private class LabelInfo
        {
            public int codeIndex = DUMMY_ADDRESS;
            public List<(string type, int id)> blockStack;
            public int tryNesting;
        }

        private struct PendingGoto
        {
            public int commandIndex;
            public int exitTryIndex;
            public string labelName;
            public List<(string type, int id)> blockStack;
            public List<(int commandIndex, string loopType, int blockId)> loopCleanupSlots;
            public CodeRange location;
            public int tryNesting;
        }
    }
}