/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace ScriptEngine.Compiler.ByteCode
{
    internal partial class AstBasedCodeGenerator
    {
        public const string BODY_METHOD_NAME = "$entry";
        private const int DUMMY_ADDRESS = -1;
        
        private struct ForwardedMethodDecl
        {
            public string identifier;
            public bool[] factArguments;
            public bool asFunction;
            public int codeLine;
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
    }
}