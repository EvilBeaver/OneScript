/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Localization;

namespace OneScript.Native.Compiler
{
    public abstract class ExpressionTreeGeneratorBase : BslSyntaxWalker
    {
        private IErrorSink _errors;
        private ModuleInformation _moduleInfo;
        private SymbolTable _ctx;

        protected ExpressionTreeGeneratorBase()
        {
        }
        
        protected ExpressionTreeGeneratorBase(IErrorSink errors)
        {
            _errors = errors;
        }
        
        protected ExpressionTreeGeneratorBase(BslWalkerContext context)
        {
            InitContext(context);
        }
        
        protected void InitContext(BslWalkerContext context)
        {
            InitContext(context.Errors, context.Module, context.Symbols);
        }
        
        protected void InitContext(IErrorSink errors, ModuleInformation moduleInfo, SymbolTable symbols)
        {
            _errors = errors;
            _moduleInfo = moduleInfo;
            _ctx = symbols;
        }
        
        protected IErrorSink Errors => _errors;

        protected ModuleInformation ModuleInfo => _moduleInfo;

        protected SymbolTable Symbols => _ctx;
        
        protected virtual BslWalkerContext MakeContext()
        {
            return new BslWalkerContext
            {
                Symbols = _ctx,
                Errors = _errors,
                Module = _moduleInfo
            };
        }

        protected virtual void AddError(BilingualString errorText, CodeRange location)
        {
            Errors.AddError(new ParseError
            {
                Description = errorText.ToString(),
                Position = ToCodePosition(location)
            });
        }
        
        protected void AddError(ParseError err)
        {
            Errors.AddError(err);
        }
        
        private ErrorPositionInfo ToCodePosition(CodeRange range)
        {
            return new ErrorPositionInfo
            {
                Code = _moduleInfo.CodeIndexer.GetCodeLine(range.LineNumber),
                LineNumber = range.LineNumber,
                ColumnNumber = range.ColumnNumber,
                ModuleName = _moduleInfo.ModuleName
            };
        }
    }
}