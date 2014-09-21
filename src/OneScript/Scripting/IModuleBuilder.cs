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

        IASTNode UnaryOperation(Token token, IASTNode operandNode);

        IASTNode BuildFunctionCall(IASTNode target, string identifier, IASTNode[] args);

        IASTNode ResolveProperty(IASTNode target, string propertyName);

        IASTNode BuildIndexedAccess(IASTNode target, IASTNode expression);

        void BuildProcedureCall(IASTNode resolved, string ident, IASTNode[] args);

        IASTNode BeginMethod(string identifier, bool isFunction);

        void SetMethodSignature(IASTNode methodNode, ASTMethodParameter[] parameters, bool isExported);

        void EndMethod(IASTNode methodNode);

    }
}
