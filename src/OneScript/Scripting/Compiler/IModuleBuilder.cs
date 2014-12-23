using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler
{
    public interface IModuleBuilder
    {

        void BeginModule();

        void CompleteModule();

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

        void BuildProcedureCall(IASTNode target, string ident, IASTNode[] args);

        IASTMethodNode BeginMethod(string identifier, bool isFunction);

        void EndMethod(IASTMethodNode methodNode);

        IASTNode BeginBatch();
        
        void EndBatch(IASTNode batch);

        IASTConditionNode IfStatement();

    }
}
