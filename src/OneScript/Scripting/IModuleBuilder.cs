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
        void BuildReadConstant(ConstDefinition constDef);
        void BuildGetReference(ConstDefinition constDef);
        void WriteReference();
        void BuildBinaryOperation(Token operationToken);
        void BuildUnaryOperation(Token operationToken);
        void BuildMethodCall(string methodName, int argumentCount, bool asFunction);
        void BuildAssignment();
        void OnError(CompilerErrorEventArgs errorInfo);
    }
}
