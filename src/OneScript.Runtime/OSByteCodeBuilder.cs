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
        private VariableUsageTable _currentLocalsTable;

        private List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();

        private struct ForwardedMethodDecl
        {
            public string identifier;
            public bool[] factArguments;
            public bool asFunction;
            public int codeLine;
            public int commandIndex;
        }

        public void BeginModule()
        {
            _module = new CompiledModule();
            PushScope();
        }

        public IASTNode BeginModuleBody()
        {
            PushScope();
            var method = new ModuleMethodNode();
            method.Identifier = "$entry";
            method.EntryPoint = _module.Commands.Count;
            System.Diagnostics.Debug.Assert(_currentLocalsTable == null);
            _currentLocalsTable = new VariableUsageTable();
            return method;
        }

        public void EndModuleBody(IASTNode bodyNode)
        {
            PopScope();
            var node = bodyNode as ModuleMethodNode;
            System.Diagnostics.Debug.Assert(node != null);

            var method = node.GetMethodForModule(_module);
            method.VariableTable = _currentLocalsTable;
            _currentLocalsTable = null;
            _module.Methods.Add(method);
            _module.EntryPointName = method.Name;
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
            throw new NotImplementedException();
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
            if(target == null)
            {
                SymbolBinding binding;
                var isKnown = Context.TryGetMethod(identifier, out binding);
                if(!isKnown)
                    throw new NotImplementedException();

                int callIndex = _module.MethodUsageMap.GetIndex(binding);
                //AddOperation(OperationCode.)

            }
            else
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
            var method = new ModuleMethodNode();
            method.EntryPoint = _module.Commands.Count;
            System.Diagnostics.Debug.Assert(_currentLocalsTable == null);
            _currentLocalsTable = new VariableUsageTable();
            return method;
        }

        public void EndMethod(IASTMethodDefinitionNode methodNode)
        {
            PopScope();
            var methodNodeImpl = methodNode as ModuleMethodNode;
            System.Diagnostics.Debug.Assert(methodNodeImpl != null);
            var method = methodNodeImpl.GetMethodForModule(_module);
            method.VariableTable = _currentLocalsTable;
            _currentLocalsTable = null;
            _module.Methods.Add(method);
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
                AddOperation(OperationCode.PushLocal, _currentLocalsTable.GetIndex(varBinding));
            }
            else
            {
                AddOperation(OperationCode.PushVar, _module.VariableUsageMap.GetIndex(varBinding));
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
            return (SymbolScope)Context.PopScope();
        }

    }
}
