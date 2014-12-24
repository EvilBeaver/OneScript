using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Scripting.Compiler;

namespace OneScript.Scripting.Runtime.CodeGeneration
{
    public class ByteCodeModuleBuilder : IModuleBuilder
    {
        ModuleImage _module;

        public ByteCodeModuleBuilder()
        {
            _module = new ModuleImage();
        }

        public ModuleImage Module
        {
            get
            {
                return _module;
            }
        }

        public CompilerContext SymbolsContext { get; set; }

        public SymbolScope NewScope()
        {
            CheckEmptyScope();

            var newScope = new SymbolScope();
            SymbolsContext.PushScope(newScope);

            return newScope;
        }

        public SymbolScope ExitScope()
        {
            CheckEmptyScope();

            return SymbolsContext.PopScope();
        }

        private void CheckEmptyScope()
        {
            if (SymbolsContext == null)
                throw new InvalidOperationException("Symbol scope is not defined");
        }

        #region IModuleBuilder members

        public void BeginModule()
        {
            throw new NotImplementedException();
        }

        public void CompleteModule()
        {
            throw new NotImplementedException();
        }

        public void DefineExportVariable(string symbolicName)
        {
            throw new NotImplementedException();
        }

        public void DefineVariable(string symbolicName)
        {
            throw new NotImplementedException();
        }

        public IASTNode SelectOrUseVariable(string identifier)
        {
            throw new NotImplementedException();
        }

        public void BuildAssignment(IASTNode acceptor, IASTNode source)
        {
            throw new NotImplementedException();
        }

        public IASTNode ReadLiteral(Lexem lexem)
        {
            throw new NotImplementedException();
        }

        public IASTNode ReadVariable(string identifier)
        {
            throw new NotImplementedException();
        }

        public IASTNode BinaryOperation(Token operationToken, IASTNode leftHandedNode, IASTNode rightHandedNode)
        {
            throw new NotImplementedException();
        }

        public IASTNode UnaryOperation(Token token, IASTNode operandNode)
        {
            throw new NotImplementedException();
        }

        public IASTNode BuildFunctionCall(IASTNode target, string identifier, IASTNode[] args)
        {
            throw new NotImplementedException();
        }

        public IASTNode ResolveProperty(IASTNode target, string propertyName)
        {
            throw new NotImplementedException();
        }

        public IASTNode BuildIndexedAccess(IASTNode target, IASTNode expression)
        {
            throw new NotImplementedException();
        }

        public void BuildProcedureCall(IASTNode target, string ident, IASTNode[] args)
        {
            throw new NotImplementedException();
        }

        public IASTMethodNode BeginMethod(string identifier, bool isFunction)
        {
            throw new NotImplementedException();
        }

        public void EndMethod(IASTMethodNode methodNode)
        {
            throw new NotImplementedException();
        }

        public IASTNode BeginBatch()
        {
            throw new NotImplementedException();
        }

        public void EndBatch(IASTNode batch)
        {
            throw new NotImplementedException();
        }

        public IASTConditionNode BeginConditionStatement()
        {
            throw new NotImplementedException();
        }

        public void EndConditionStatement(IASTConditionNode node)
        {
            throw new NotImplementedException();
        } 
        #endregion

        
    }
}
