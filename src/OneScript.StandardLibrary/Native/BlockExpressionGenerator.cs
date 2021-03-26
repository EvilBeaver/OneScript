/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using OneScript.Commons;
using OneScript.Language;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.Compiler;
using ScriptEngine.Compiler.Extensions;
using ScriptEngine.Environment;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;
using Refl = System.Reflection;

namespace OneScript.StandardLibrary.Native
{
    internal class BlockExpressionGenerator : BslSyntaxWalker
    {
        private readonly SymbolResolver _ctx;
        private readonly ITypeManager _typeManager;
        private readonly IErrorSink _errors;
        private ModuleInformation _moduleInfo;
        
        private List<Expression> _statements = new List<Expression>();
        private Stack<Expression> _statementBuildParts = new Stack<Expression>();
        
        public BlockExpressionGenerator(
            SymbolResolver context,
            ITypeManager typeManager,
            IErrorSink errors)
        {
            _ctx = context;
            _typeManager = typeManager;
            _errors = errors;
        }

        public BlockExpression CreateExpression(CodeBatchNode blockNode, ModuleInformation moduleInfo)
        {
            _moduleInfo = moduleInfo;

            Visit(blockNode);

            return Expression.Block(_localVariables, _statements);
        }

        private static Type ConvertTypeToClrType(TypeTypeValue typeVal)
        {
            var type = typeVal.TypeValue;

            Type clrType;
            if (type == BasicTypes.String)
                clrType = typeof(string);
            else if (type == BasicTypes.Date)
                clrType = typeof(DateTime);
            else if (type == BasicTypes.Boolean)
                clrType = typeof(bool);
            else if (type == BasicTypes.Number)
                clrType = typeof(decimal);
            else if (type == BasicTypes.Type)
                clrType = typeof(TypeTypeValue);
            else
                clrType = type.ImplementingClass;

            return clrType;
        }

        protected override void VisitStatement(BslSyntaxNode statement)
        {
            _statementBuildParts.Clear();
            base.VisitStatement(statement);
        }

        protected override void VisitGlobalProcedureCall(CallNode node)
        {
            if (LanguageDef.IsBuiltInFunction(node.Identifier.Lexem.Token))
            {   
                _errors.AddError(LocalizedErrors.UseBuiltInFunctionAsProcedure());
                return;
            }

            MethodSymbol symbol;
            try
            {
                symbol = _ctx.GetMethod(node.Identifier.GetIdentifier());
                
                var expression = CreateClrMethodCall(symbol.Target, symbol.MethodInfo);
                _statements.Add(expression);
            }
            catch (CompilerException e)
            {
                _errors.AddError(e);
            }
        }

        protected override void VisitVariableRead(TerminalNode node)
        {
            VariableSymbol symbol;
            var identifier = node.GetIdentifier();
            try
            {
                symbol = _ctx.GetVariable(identifier);
                
            }
            catch (CompilerException e)
            {
                _errors.AddError(e); 
                return;
            }
            
            _errors.AddError(new ParseError
            {
                Description = Locale.NStr($"ru='Переменная {identifier} не доступна для чтения';en='Variable {identifier} is not availiable for reading'"),
                Position = node.Lexem.Location.ToCodePosition(_moduleInfo)
            });
        }

        protected override void VisitVariableWrite(TerminalNode node)
        {
            var identifier = node.GetIdentifier();
            var hasVar = _ctx.TryGetVariable(identifier, out var varBinding);
            if (hasVar)
            {
                if (varBinding.binding.ContextIndex == _ctx.TopIndex())
                {
                    
                    //AddCommand(OperationCode.LoadLoc, varBinding.binding.CodeIndex);
                }
                else
                {
                    _errors.AddError(new ParseError
                    {
                        Description = Locale.NStr($"ru='Переменная {identifier} не доступна для записи';en='Variable {identifier} is not availiable for write'"),
                        Position = node.Lexem.Location.ToCodePosition(_moduleInfo)
                    });
                }
            }
            else
            {
                // can create variable
                var typeOnStack = _statementBuildParts.Peek().Type;
                _ctx.DefineVariable(identifier);
                var variable = Expression.Variable(typeOnStack, identifier);
                _localVariables.Add(variable, identifier);
                _statementBuildParts.Push(variable);
                var assign = Assign();
                _statements.Add(assign);
            }
        }

        private Expression Assign()
        {
            var left = _statementBuildParts.Pop();
            var right = _statementBuildParts.Pop();
            var assign = Expression.Assign(left, right);

            return assign;
        }

        private Expression CreateClrMethodCall(IRuntimeContextInstance context, Refl.MethodInfo method)
        {
            // этот контекст или класс контекстный или скриптовый метод
            // для скриптовых методов мы сделаем обычный вызовем через машину
            // для контекстов - сделаем прямой вызов метода [ContextMethod] с типизацией параметров
            throw new NotImplementedException();
        }
    }
}