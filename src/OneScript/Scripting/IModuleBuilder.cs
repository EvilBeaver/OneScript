using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public interface IModuleBuilder
    {
        void BeginModule(ICompilerContext context);
        void CompleteModule();
        void SnapToCodeLine(int line);
        void BuildVariable(string name);
        void BuildExportVariable(string name);
        void BuildLoadVariable(SymbolBinding binding);
        void BuildReadVariable(SymbolBinding binding);
        void BuildReadConstant(ConstDefinition constDef);
        void BuildGetReference(ConstDefinition constDef);
        void WriteReference();
        void BeginExpression();
        void AddOperation(Token operatorToken);
        void EndExpression();
        void BeginMethodCall(string methodName, bool asFunction);
        void AddArgument();
        void EndMethodCall();
        void BeginIndexAccess();
        void EndIndexAccess();
        void OnError(CompilerErrorEventArgs errorInfo);
    }
}
