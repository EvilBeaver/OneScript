using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public interface IModuleBuilder
    {

        void BeginModule();

        void CompleteModule();

        void OnError(CompilerErrorEventArgs eventArgs);

        void DefineExportVariable(string symbolicName);

        void DefineVariable(string symbolicName);

        IASTNode SelectOrUseVariable(string identifier);

        void BuildAssignment(IASTNode acceptor, IASTNode source);

        IASTNode ReadLiteral(Lexem lexem);

        IASTNode ReadVariable(string identifier);

        IASTNode BinaryOperation(Token operationToken, IASTNode leftHandedNode, IASTNode rightHandedNode);
    }
}
