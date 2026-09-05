/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;

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

        private bool _runtimeDirectiveDefined;
        
        public NativeRuntimeAnnotationHandler(IErrorSink errorSink) : base(Directives, errorSink)
        {
        }

        public override void OnModuleEnter()
        {
            _runtimeDirectiveDefined = false;
            base.OnModuleEnter();
        }

        public override void OnModuleLeave()
        {
            _runtimeDirectiveDefined = false;
            base.OnModuleLeave();
        }

        protected override void ParseAnnotationInternal(
            ref Lexem lastExtractedLexem,
            ILexer lexer,
            ParserContext parserContext)
        {
            if (_runtimeDirectiveDefined)
            {
                var err = LocalizedErrors.RuntimeDirectiveAlreadyDefined();
                err.Position = new ErrorPositionInfo
                {
                    LineNumber = lastExtractedLexem.Location.LineNumber,
                    ColumnNumber = lastExtractedLexem.Location.ColumnNumber,
                    Code = lexer.Iterator.GetCodeLine(lastExtractedLexem.Location.LineNumber),
                    ModuleName = lexer.Iterator.Source.Name
                };
                ErrorSink.AddError(err);
                lastExtractedLexem = lexer.NextLexem();
                return;
            }

            _runtimeDirectiveDefined = true;
            base.ParseAnnotationInternal(ref lastExtractedLexem, lexer, parserContext);
        }
    }
}
