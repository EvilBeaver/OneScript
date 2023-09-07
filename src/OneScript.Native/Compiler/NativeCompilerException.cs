/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq.Expressions;
using OneScript.Compilation;
using OneScript.Exceptions;
using OneScript.Language;
using OneScript.Localization;

namespace OneScript.Native.Compiler
{
    public class NativeCompilerException : CompilerException
    {
        public NativeCompilerException(string message) : base(message)
        {
        }
        
        public NativeCompilerException(string message, ErrorPositionInfo position) : base(message, position)
        {
        }

        public static NativeCompilerException OperationNotDefined(ExpressionType opCode, Type left, Type right) =>
            new NativeCompilerException(
                BilingualString.Localize(
                    $"Операция {opCode} не определена для типов {left} и {right}",
                    $"Operation {opCode} is not defined for {left} and {right}")
            );
    }
}