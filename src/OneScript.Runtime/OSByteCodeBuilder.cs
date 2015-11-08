using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OneScript.Core;
using OneScript.Language;
using OneScript.Runtime.Compiler;

namespace OneScript.Runtime
{
    public class OSByteCodeBuilder : IASTBuilder
    {
        private CompiledModule _module;

        public void BeginModule()
        {
            _module = new CompiledModule();
            PushScope();
        }

        public void BeginModuleBody()
        {
            PushScope();
        }

        public void EndModuleBody()
        {
            PopScope();
        }

        public void CompleteModule()
        {
            PopScope();
        }

        public void DefineExportVariable(string symbolicName)
        {
            throw new NotImplementedException();
        }

        public void DefineVariable(string symbolicName)
        {
            throw new NotImplementedException();
        }

        public IASTNode SelectOrCreateVariable(string identifier)
        {
            SymbolBinding varDef;
            if(!Context.TryGetVariable(identifier, out varDef))
            {
                varDef = Context.DefineVariable(identifier);
            }

            WritePushVariable(varDef);
            return NodeStub();
        }

        public IASTNode BuildAssignment(IASTNode acceptor, IASTNode source)
        {
            AddOperation(OperationCode.Assign);
            return NodeStub();
        }

        public IASTNode ReadLiteral(Lexem lexem)
        {
            var constDef = ConstDefinition.CreateFromLiteral(ref lexem);
            int index = _module.Constants.GetIndex(constDef);
            AddOperation(OperationCode.PushConst, index);
            return NodeStub();
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

        public void BuildProcedureCall(IASTNode target, string identifier, IASTNode[] args)
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

        public IASTMethodDefinitionNode BeginMethod()
        {
            PushScope();
            return new ModuleMethodNode();
        }

        public void EndMethod(IASTMethodDefinitionNode methodNode)
        {
            PopScope();
            var methodNodeImpl = methodNode as ModuleMethodNode;
            System.Diagnostics.Debug.Assert(methodNodeImpl != null);

            _module.Methods.Add(methodNodeImpl.GetMethodForModule(_module));
        }

        public IASTNode BeginBatch()
        {
            //throw new NotImplementedException();
            return NodeStub();
        }

        public void EndBatch(IASTNode batch)
        {
            //throw new NotImplementedException();
        }

        public IASTConditionNode BeginConditionStatement()
        {
            throw new NotImplementedException();
        }

        public void EndConditionStatement(IASTConditionNode node)
        {
            throw new NotImplementedException();
        }

        public IASTWhileNode BeginWhileStatement()
        {
            throw new NotImplementedException();
        }

        public void EndWhileStatement(IASTWhileNode node)
        {
            throw new NotImplementedException();
        }

        public IASTForLoopNode BeginForLoopNode()
        {
            throw new NotImplementedException();
        }

        public void EndForLoopNode(IASTForLoopNode node)
        {
            throw new NotImplementedException();
        }

        public IASTForEachNode BeginForEachNode()
        {
            throw new NotImplementedException();
        }

        public void EndForEachNode(IASTForEachNode node)
        {
            throw new NotImplementedException();
        }

        public IASTTryExceptNode BeginTryExceptNode()
        {
            throw new NotImplementedException();
        }

        public void EndTryBlock(IASTTryExceptNode node)
        {
            throw new NotImplementedException();
        }

        public void EndExceptBlock(IASTTryExceptNode node)
        {
            throw new NotImplementedException();
        }

        public void BuildBreakStatement()
        {
            throw new NotImplementedException();
        }

        public void BuildContinueStatement()
        {
            throw new NotImplementedException();
        }

        public void BuildReturnStatement(IASTNode expression)
        {
            throw new NotImplementedException();
        }

        public void BuildRaiseExceptionStatement(IASTNode expression)
        {
            throw new NotImplementedException();
        }

        public CompilerContext Context { get; set; }

        public CompiledModule GetModule()
        {
            return _module;
        }

        private int AddOperation(OperationCode opCode, int argument)
        {
            int commandIndex = _module.Commands.Count;
            _module.Commands.Add(new Command() { Code = opCode, Argument = argument });
            return commandIndex;
        }

        private void WritePushVariable(SymbolBinding varBinding)
        {
            if(varBinding.Context == Context.TopScopeIndex)
            {
                AddOperation(OperationCode.PushLocal, varBinding.IndexInContext);
            }
            else
            {
                AddOperation(OperationCode.PushVar, _module.VariableTable.GetIndex(varBinding));
            }
        }

        private int AddOperation(OperationCode opCode)
        {
            return AddOperation(opCode, 0);
        }

        private IASTNode NodeStub()
        {
            return null;
        }


        private SymbolScope PushScope()
        {
            var newScope = new SymbolScope();
            Context.PushScope(newScope);
            return newScope;
        }

        private void PushScope(SymbolScope newScope)
        {
            Context.PushScope(newScope);
        }

        private SymbolScope PopScope()
        {
            return Context.PopScope();
        }

    }
}
