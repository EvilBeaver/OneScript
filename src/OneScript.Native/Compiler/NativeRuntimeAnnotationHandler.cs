/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Localization;

namespace OneScript.Native.Compiler
{
    public class NativeRuntimeAnnotationHandler : SingleWordModuleAnnotationHandler
    {
        public static string NativeDirectiveName => "native";
        public static string StackRuntimeDirectiveName => "stack";

        private static readonly HashSet<string> Directives = new HashSet<string>
        {
            NativeDirectiveName,
            StackRuntimeDirectiveName
        };
        
        public NativeRuntimeAnnotationHandler(IErrorSink errorSink) : base(Directives, errorSink)
        {
        }
    }
}