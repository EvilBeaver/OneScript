/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq.Expressions;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Native.Compiler
{
    public class MethodCompiler : BslSyntaxWalker
    {
        private readonly SymbolTable _symbols;
        private readonly IErrorSink _errors;
        private readonly ModuleInformation _moduleInformation;

        public MethodCompiler(SymbolTable symbols, IErrorSink errors, ModuleInformation moduleInformation)
        {
            _symbols = symbols;
            _errors = errors;
            _moduleInformation = moduleInformation;
        }

        public BslMethodInfo CreateMethodInfo(MethodNode methodNode)
        {
            throw new NotImplementedException();
            Visit(methodNode);
        }
    }
}