using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Diagnostics;
using OneScript.Scripting.Compiler;

namespace OneScript.Scripting.Runtime
{
    public class BCodeModuleBuilder : IModuleBuilder
    {
        private const int INVALID_INDEX = -1;

        private int CurrentMethodIndex 
        {
            get { return Module.Methods.Count - 1; }
        }

        public CompiledModule Module { get; private set; }

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
            Module = new CompiledModule();
            NewScope();
        }

        public void CompleteModule()
        {
            ExitScope();
        }

        public void DefineExportVariable(string symbolicName)
        {
            CheckEmptyScope();

            SymbolsContext.DefineVariable(symbolicName);
            Module.Variables.Add(VariableDef.CreateExported(symbolicName));
        }

        public void DefineVariable(string symbolicName)
        {
            CheckEmptyScope();

            SymbolsContext.DefineVariable(symbolicName);
            Module.Variables.Add(VariableDef.CreateGlobal(symbolicName));
        }

        public IASTNode SelectOrUseVariable(string identifier)
        {
            SymbolBinding symbol;
            if (!SymbolsContext.TryGetVariable(identifier, out symbol))
            {
                symbol = SymbolsContext.DefineVariable(identifier);
            }

            PushVariable(ref symbol);
            
            return null;

        }

        private int PushVariable(ref SymbolBinding symbol)
        {
            int cmdAddress;

            if (SymbolsContext.GetScope(symbol.Context) == SymbolsContext.TopScope)
            {
                var method = Module.Methods[CurrentMethodIndex];
                var refIdx = method.LocalRefs.Count;
                method.LocalRefs.Add(symbol);
                cmdAddress = AddCommand(OperationCode.PushLocal, refIdx);
            }
            else
            {
                var refIdx = Module.VariableRefs.Count;
                Module.VariableRefs.Add(symbol);
                cmdAddress = AddCommand(OperationCode.PushVar, refIdx);
            }

            return cmdAddress;
        }

        private int AddCommand(OperationCode operationCode)
        {
            return AddCommand(operationCode, 0);
        }

        private int AddCommand(OperationCode operationCode, int arg)
        {
            var command = new Command()
            {
                Code = operationCode,
                Argument = arg
            };

            var idx = Module.Code.Count;
            Module.Code.Add(command);

            return idx;
        }

        public IASTNode BuildAssignment(IASTNode acceptor, IASTNode source)
        {
            AddCommand(OperationCode.Assign);
            return null;
        }

        public IASTNode ReadLiteral(Lexem lexem)
        {
            var constDef = ConstDefinition.CreateFromLiteral(ref lexem);
            AddCommand(OperationCode.PushConst, GetConstNumber(ref constDef));

            return null;
        }

        private int GetConstNumber(ref ConstDefinition cDef)
        {
            var idx = Module.Constants.IndexOf(cDef);
            if (idx >= 0) 
                return idx;

            idx = Module.Constants.Count;
            Module.Constants.Add(cDef);
            return idx;
        }

        public IASTNode ReadVariable(string identifier)
        {
            var symbol = SymbolsContext.GetVariable(identifier);
            PushVariable(ref symbol);

            return null;
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

        public void BeginModuleBody()
        {
            NewScope();

            var entryMethod = new MethodDef();
            entryMethod.Name = "$entry";
            entryMethod.Signature = MethodSignatureData.CreateProcedure(0);
            entryMethod.EntryPoint = Module.Code.Count;
            int index = Module.Methods.Count;
            Module.EntryMethodIndex = index;
            Module.Methods.Add(entryMethod);
        }

        public void EndModuleBody()
        {
            ExitScope();
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
