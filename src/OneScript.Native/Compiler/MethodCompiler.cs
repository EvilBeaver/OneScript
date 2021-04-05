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
    public class MethodCompiler : ExpressionTreeGeneratorBase
    {
        public MethodCompiler(BslWalkerContext walkContext) : base(walkContext)
        {
        }

        public BslMethodInfo CreateMethodInfo(MethodNode methodNode)
        {
            throw new NotImplementedException();
            Visit(methodNode);
        }
        
        /// <summary>
        /// Создает тело модуля (процедуру без параметров с сигнатурой void(void)
        /// </summary>
        /// <param name="batch"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public BslMethodInfo CreateMethodInfo(string methodName, CodeBatchNode batch)
        {
            throw new NotImplementedException();
            VisitCodeBlock(batch);
        }
    }
}