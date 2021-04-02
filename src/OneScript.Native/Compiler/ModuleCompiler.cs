/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Dynamic.Compiler;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Native.Compiler
{
    public class ModuleCompiler : BslSyntaxWalker
    {
        private ModuleInformation _moduleInfo;

        public DynamicModule Compile(ModuleInformation moduleInfo, BslSyntaxNode moduleNode)
        {
            throw new NotImplementedException();
        }
    }
}