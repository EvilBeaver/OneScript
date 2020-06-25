/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using OneScript.Language.LexicalAnalysis;

namespace OneScript.Language.SyntaxAnalysis
{
    public class DefaultBslParser
    {
        private readonly IAstBuilder _builder;
        private ILexemGenerator _lexer;
        private Lexem _lastExtractedLexem;
        
        private bool _inMethodScope = false;
        private bool _isMethodsDefined = false;
        private bool _isStatementsDefined = false;
        private bool _isInFunctionScope = false;
        private bool _lastDereferenceIsWritable = false;

        private Stack<IAstNode> _parsingContext = new Stack<IAstNode>();
        
        private List<ParseError> _errors = new List<ParseError>();
        private readonly Stack<Token[]> _tokenStack = new Stack<Token[]>();
        private bool _isInLoopScope;
        private bool _enableException = false;

        public DefaultBslParser(IAstBuilder builder, ILexemGenerator lexer)
        {
            _builder = builder;
            _lexer = lexer;
        }

        public IEnumerable<ParseError> Errors => _errors; 
        
        public void ParseStatefulModule()
        {
            NextLexem();
            var node = _builder.CreateNode(NodeKind.Module, _lastExtractedLexem);
            PushContext(node);
            
            try
            {
                ParseDirectives();
                ParseModuleSections();
            }
            finally
            {
                PopContext();
            }
        }

        private void ParseDirectives()
        {
            while (_lastExtractedLexem.Type == LexemType.PreprocessorDirective)
            {
                _builder.PreprocessorDirective(_lexer, ref _lastExtractedLexem);
                if(_lastExtractedLexem.Token == Token.EndOfText)
                    break;
            }
        }

        public void ParseCodeBatch()
        {
            NextLexem();
            var node = _builder.CreateNode(NodeKind.Module, _lastExtractedLexem);
            PushContext(node);
            try
            {
                ParseDirectives();
                BuildModuleBody();
            }
            finally
            {
                PopContext();
            }
        }

        private void PushContext(IAstNode node) => _parsingContext.Push(node);
        
        private IAstNode PopContext() => _parsingContext.Pop();

        private IAstNode CurrentParent => _parsingContext.Peek();

        private void ParseModuleSections()
        {
            BuildVariableSection();
            BuildMethodsSection();
            BuildModuleBody();
        }

        #region Variables
        
        private void BuildVariableSection(NodeKind sectionKind = NodeKind.VariablesSection)
        {
            if (_lastExtractedLexem.Token != Token.VarDef && _lastExtractedLexem.Type != LexemType.Annotation)
            {
                return;
            }

            var parent = CurrentParent;
            var allVarsSection = _builder.CreateNode(sectionKind, _lastExtractedLexem);
            PushContext(allVarsSection);
            bool hasVars = false;
            try
            {
                while (true)
                {
                    BuildAnnotations();
                    if (_lastExtractedLexem.Token == Token.VarDef)
                    {
                        if (!hasVars)
                        {
                            hasVars = true;
                            _builder.AddChild(parent, allVarsSection);
                        }

                        BuildVariableDefinition();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                PopContext();
            }
        }

        private void BuildVariableDefinition()
        {
            while (true)
            {
                var variable = CreateChild(
                    CurrentParent,
                    NodeKind.VariableDefinition,
                    _lastExtractedLexem);

                NextLexem();

                if (IsUserSymbol(_lastExtractedLexem))
                {
                    if (_inMethodScope)
                    {
                        if (_isStatementsDefined)
                        {
                            AddError(LocalizedErrors.LateVarDefinition());
                            return;
                        }
                    }
                    else
                    {
                        if (_isMethodsDefined)
                        {
                            AddError(LocalizedErrors.LateVarDefinition());
                            return;
                        }
                    }
                    
                    var symbolicName = _lastExtractedLexem.Content;
                    CreateChild(variable, NodeKind.Identifier, _lastExtractedLexem);
                    
                    NextLexem();
                    if (_lastExtractedLexem.Token == Token.Export)
                    {
                        if (_inMethodScope)
                        {
                            AddError(LocalizedErrors.ExportedLocalVar(symbolicName));
                        }
                        CreateChild(variable, NodeKind.ExportFlag, _lastExtractedLexem);
                        NextLexem();
                    }
                    
                    if (_lastExtractedLexem.Token == Token.Comma)
                    {
                        continue;
                    }

                    if (_lastExtractedLexem.Token == Token.Semicolon)
                    {
                        NextLexem();
                    }
                    else
                    {
                        AddError(LocalizedErrors.SemicolonExpected());
                    }
                        
                }
                else
                {
                    AddError(LocalizedErrors.IdentifierExpected());
                }

                break;
            }
        }

        #endregion

        #region Methods

        private void BuildMethodsSection()
        {
            if (_lastExtractedLexem.Type != LexemType.Annotation 
                && _lastExtractedLexem.Token != Token.Procedure 
                && _lastExtractedLexem.Token != Token.Function)
            {
                return;
            }

            var parent = CurrentParent;
            var allMethodsSection = _builder.CreateNode(NodeKind.MethodsSection, _lastExtractedLexem);
            var sectionExist = false;
            PushContext(allMethodsSection);

            try
            {
                while (true)
                {
                    BuildAnnotations();
                    if (_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function)
                    {
                        if (!sectionExist)
                        {
                            sectionExist = true;
                            _builder.AddChild(parent, allMethodsSection);
                        }

                        BuildMethod();
                    }
                    else
                    {
                        break;
                    }
                }
            }
            finally
            {
                PopContext();
            }
        }

        private void BuildMethod()
        {
            Debug.Assert(_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function);

            var method = CreateChild(
                CurrentParent,
                NodeKind.Method,
                _lastExtractedLexem);

            PushContext(method);
            try
            {
                BuildMethodSignature();
                _inMethodScope = true;
                BuildVariableSection();
                _isStatementsDefined = true;
                BuildMethodBody();
            }
            finally
            {
                _isInFunctionScope = false;
                _inMethodScope = false;
                _isStatementsDefined = false;
                PopContext();
            }
        }

        private void BuildMethodBody()
        {
            var body = CreateChild(CurrentParent, NodeKind.CodeBatch, _lastExtractedLexem);
            PushContext(body);
            try
            {
                BuildCodeBatch(_isInFunctionScope ? Token.EndFunction : Token.EndProcedure);
                NextLexem();
            }
            finally
            {
                PopContext();
            }
        }

        private void BuildMethodSignature()
        {
            var isFunction = _lastExtractedLexem.Token == Token.Function;

            var signature = CreateChild(CurrentParent, NodeKind.MethodSignature, _lastExtractedLexem);
            CreateChild(signature, isFunction? NodeKind.Function : NodeKind.Procedure, _lastExtractedLexem);

            _isInFunctionScope = isFunction;
            NextLexem();
            if (!IsUserSymbol(_lastExtractedLexem))
            {
                AddError(LocalizedErrors.IdentifierExpected());
                return;
            }

            CreateChild(signature, NodeKind.Identifier, _lastExtractedLexem);
            BuildMethodParameters(signature);
            if (_lastExtractedLexem.Token == Token.Export)
            {
                CreateChild(signature, NodeKind.ExportFlag, _lastExtractedLexem);
                NextLexem();
            }
        }

        private void BuildMethodParameters(IAstNode node)
        {
            if (!NextExpected(Token.OpenPar))
            {
                AddError(LocalizedErrors.TokenExpected(Token.OpenPar));
                return;
            }

            var paramList = CreateChild(
                node, NodeKind.MethodParameters, _lastExtractedLexem);
            NextLexem(); // (

            while (_lastExtractedLexem.Token != Token.ClosePar)
            {
                BuildAnnotations();
                var param = CreateChild(paramList, NodeKind.MethodParameter, _lastExtractedLexem);
                // [Знач] Identifier [= Literal],...
                if (_lastExtractedLexem.Token == Token.ByValParam)
                {
                    CreateChild(param, NodeKind.ByValModifier, _lastExtractedLexem);
                    NextLexem();
                }

                if (!IsUserSymbol(_lastExtractedLexem))
                {
                    AddError(LocalizedErrors.IdentifierExpected());
                    return;
                }

                CreateChild(param, NodeKind.Identifier, _lastExtractedLexem);
                NextLexem();
                if (_lastExtractedLexem.Token == Token.Equal)
                {
                    NextLexem();
                    if(!BuildDefaultParameterValue(param))
                        return;
                }
                
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextLexem();
                }
            }

            NextLexem(); // )

        }

        private bool BuildDefaultParameterValue(IAstNode param)
        {
            bool hasSign = false;
            bool signIsMinus = _lastExtractedLexem.Token == Token.Minus;
            if (signIsMinus || _lastExtractedLexem.Token == Token.Plus)
            {
                hasSign = true;
                NextLexem();
            }

            if (LanguageDef.IsLiteral(ref _lastExtractedLexem))
            {
                string literalText = _lastExtractedLexem.Content;
                if (hasSign)
                {
                    if (_lastExtractedLexem.Type == LexemType.NumberLiteral && signIsMinus)
                    {
                        literalText = '-' + literalText;
                    }
                    else if (_lastExtractedLexem.Type == LexemType.StringLiteral
                             || _lastExtractedLexem.Type == LexemType.DateLiteral)
                    {
                        AddError(LocalizedErrors.NumberExpected());
                        return false;
                    }
                }

                _lastExtractedLexem.Content = literalText;
                CreateChild(param, NodeKind.ParameterDefaultValue, _lastExtractedLexem);
                NextLexem();
            }
            else
            {
                AddError(LocalizedErrors.LiteralExpected());
                return false;
            }

            return true;
        }
        
        #endregion
        
        private void BuildModuleBody()
        {
            var node = CreateChild(CurrentParent, NodeKind.CodeBatch, _lastExtractedLexem);
            PushContext(node);
            try
            {
                BuildCodeBatch(Token.EndOfText);
            }
            finally
            {
                PopContext();
            }
        }

        #region Annotations
        private void BuildAnnotations()
        {
            while (_lastExtractedLexem.Type == LexemType.Annotation)
            {
                var node = _builder.CreateNode(NodeKind.Annotation, _lastExtractedLexem);
                NextLexem();
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    NextLexem();
                    BuildAnnotationParameters(node);
                }
            }
        }
        
        private void BuildAnnotationParameters(IAstNode annotation)
        {
            while (_lastExtractedLexem.Token != Token.EndOfText)
            {
                if(!BuildAnnotationParameter(annotation))
                    return;
                
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextLexem();
                    continue;
                }
                if (_lastExtractedLexem.Token == Token.ClosePar)
                {
                    NextLexem();
                    break;
                }

                AddError(LocalizedErrors.UnexpectedOperation());
            }
        }
        
        private bool BuildAnnotationParameter(IAstNode annotation)
        {
            bool success = true;
            var node = CreateChild(annotation, NodeKind.AnnotationParameter, _lastExtractedLexem);
            // id | id = value | value
            if (_lastExtractedLexem.Type == LexemType.Identifier)
            {
                CreateChild(node, NodeKind.AnnotationParameterName, _lastExtractedLexem);
                NextLexem();
                if (_lastExtractedLexem.Token == Token.Equal)
                {
                    NextLexem();
                    success = BuildAnnotationParamValue(node);
                }
            }
            else
            {
                success = BuildAnnotationParamValue(node);
            }

            return success;
        }

        public bool BuildAnnotationParamValue(IAstNode annotationParam)
        {
            if (LanguageDef.IsLiteral(ref _lastExtractedLexem))
            {
                CreateChild(annotationParam, NodeKind.AnnotationParameterValue, _lastExtractedLexem);
                NextLexem();
            }
            else
            {
                AddError(LocalizedErrors.LiteralExpected());
                return false;
            }

            return true;
        }
        
        #endregion
        
        private void BuildCodeBatch(params Token[] endTokens)
        {
            PushStructureToken(endTokens);

            while (true)
            {
                if (endTokens.Contains(_lastExtractedLexem.Token))
                {
                    break;
                }

                if (_lastExtractedLexem.Token == Token.Semicolon)
                {
                    NextLexem();
                    continue;
                }

                if (_lastExtractedLexem.Type != LexemType.Identifier && _lastExtractedLexem.Token != Token.EndOfText)
                {
                    AddError(LocalizedErrors.UnexpectedOperation());
                }

                if (_lastExtractedLexem.Token == Token.NotAToken)
                {
                    BuildSimpleStatement();
                }
                else
                {
                    BuildComplexStructureStatement();
                }

                if (_lastExtractedLexem.Token != Token.Semicolon)
                {
                    if (endTokens.Contains(_lastExtractedLexem.Token) || LanguageDef.IsEndOfBlockToken(_lastExtractedLexem.Token))
                    {
                        break;
                    }
                    AddError(LocalizedErrors.SemicolonExpected());
                }
                NextLexem();
            }
            PopStructureToken();
        }

        private void BuildComplexStructureStatement()
        {
            switch (_lastExtractedLexem.Token)
            {
                case Token.If:
                    //BuildIfStatement();
                    break;
                case Token.For:
                    //BuildForStatement();
                    break;
                case Token.While:
                    BuildWhileStatement();
                    break;
                case Token.Break:
                    //BuildBreakStatement();
                    break;
                case Token.Continue:
                    //BuildContinueStatement();
                    break;
                case Token.Return:
                    //BuildReturnStatement();
                    break;
                case Token.Try:
                    //BuildTryExceptStatement();
                    break;
                case Token.RaiseException:
                    //BuildRaiseExceptionStatement();
                    break;
                case Token.Execute:
                    //BuildExecuteStatement();
                    break;
                case Token.AddHandler:
                case Token.RemoveHandler:
                    //BuildEventHandlerOperation(_lastExtractedLexem.Token);
                    break;        
                default:
                    var expected = PopStructureToken();
                    AddError(LocalizedErrors.TokenExpected(expected));
                    break;
            }
        }

        private void BuildWhileStatement()
        {
            var loopNode = CreateChild(CurrentParent, NodeKind.Loop, _lastExtractedLexem);
            NextLexem();
            BuildExpressionUpTo(loopNode, Token.Loop);
            NextLexem();
            var body = CreateChild(loopNode, NodeKind.CodeBatch, _lastExtractedLexem);
            PushContext(body);
            try
            {
                _isInLoopScope = true;
                BuildCodeBatch(Token.EndLoop);
                CreateChild(loopNode, NodeKind.BlockEnd, _lastExtractedLexem);
            }
            finally
            {
                _isInLoopScope = false ;
                PopContext();
            }
        }

        private void BuildSimpleStatement()
        {
            var statement = CreateChild(CurrentParent, NodeKind.Statement, _lastExtractedLexem);
            try
            {
                _enableException = true;
                PushContext(statement);
                BuildAssignment(statement);
            }
            catch (InternalParseException)
            {
                // просто выход из стека построения выражения
            }
            finally
            {
                _enableException = true;
                PopContext();
            }
        }

        private void BuildAssignment(IAstNode statement)
        {
            var call = BuildGlobalCall(_lastExtractedLexem);
            if (call == default)
                return;
            
            if (_lastExtractedLexem.Token == Token.Equal)
            {
                if (_lastDereferenceIsWritable)
                {
                    var node = CreateChild(statement, NodeKind.Assignment, _lastExtractedLexem);
                    _builder.AddChild(node, call);
                    NextLexem();
                    BuildExpression(node, Token.Semicolon);
                }
                else
                {
                    AddError(LocalizedErrors.ExpressionSyntax());
                }
            }
            else
            {
                _builder.AddChild(statement, call);
            }
        }

        private IAstNode BuildGlobalCall(Lexem identifier)
        {
            _lastDereferenceIsWritable = true;
            var target = _builder.CreateNode(NodeKind.Identifier, identifier);
            NextLexem();
            var callNode = BuildCall(target);
            return BuildDereference(callNode);
        }

        private IAstNode BuildCall(IAstNode target)
        {
            IAstNode callNode = default;
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                callNode = _builder.CreateNode(NodeKind.Call, _lastExtractedLexem);
                _builder.AddChild(callNode, target);
                BuildCallParameters(callNode);
            }
            return callNode ?? target;
        }

        private int BuildCallParameters(IAstNode callNode)
        {
            var node = CreateChild(callNode, NodeKind.CallArgumentList, _lastExtractedLexem);
            int argCount = 0;
            PushStructureToken(Token.ClosePar);
            try
            {
                NextLexem(); // съели открывающую скобку
                while (_lastExtractedLexem.Token != Token.ClosePar)
                {
                    BuildCallArgument(node);
                    argCount++;
                }

                if (_lastExtractedLexem.Token != Token.ClosePar)
                {
                    AddError(LocalizedErrors.TokenExpected(Token.OpenPar));
                    return -1;
                }

                NextLexem(); // съели закрывающую скобку
            }
            finally
            {
                PopStructureToken();
            }

            return argCount;
        }

        private void BuildCallArgument(IAstNode argsList)
        {
            if (_lastExtractedLexem.Token == Token.Comma)
            {
                CreateChild(argsList, NodeKind.CallArgument, _lastExtractedLexem);
                
                BuildLastDefaultArg(argsList);
            }
            else if (_lastExtractedLexem.Token != Token.ClosePar)
            {
                var node = CreateChild(argsList, NodeKind.CallArgument, _lastExtractedLexem);
                BuildOptionalExpression(node, Token.Comma);
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    BuildLastDefaultArg(argsList);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildLastDefaultArg(IAstNode argsList)
        {
            NextLexem();
            if (_lastExtractedLexem.Token == Token.ClosePar)
            {
                CreateChild(argsList, NodeKind.CallArgument, _lastExtractedLexem);
            }
        }

        private IAstNode BuildDereference(IAstNode target)
        {
            var activeTarget = BuildIndexerAccess(target);
            if (_lastExtractedLexem.Token == Token.Dot)
            {
                var dotNode = _builder.CreateNode(NodeKind.DereferenceOperation, _lastExtractedLexem);
                _builder.AddChild(dotNode, activeTarget);
                NextLexem();
                if (!LanguageDef.IsIdentifier(ref _lastExtractedLexem))
                {
                    AddError(LocalizedErrors.IdentifierExpected());
                    return default;
                }

                var identifier = _lastExtractedLexem;
                NextLexem();
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    _lastDereferenceIsWritable = false;
                    var ident = _builder.CreateNode(NodeKind.Identifier, identifier);
                    var call = BuildCall(ident);
                    _builder.AddChild(dotNode, call);
                }
                else
                {
                    _lastDereferenceIsWritable = true;
                    CreateChild(dotNode, NodeKind.Identifier, identifier);
                }
                
                return BuildDereference(dotNode);
            }

            return activeTarget;
        }

        private IAstNode BuildIndexerAccess(IAstNode target)
        {
            if (_lastExtractedLexem.Token == Token.OpenBracket)
            {
                var node = _builder.CreateNode(NodeKind.IndexAccess, _lastExtractedLexem);
                _builder.AddChild(node, target);
                NextLexem();
                var expression = BuildExpression(node, Token.CloseBracket);
                if (expression == default)
                {
                    AddError(LocalizedErrors.ExpressionSyntax());
                    return default;
                }
                NextLexem();
                _lastDereferenceIsWritable = true;
                return BuildDereference(node);
            }

            return target;
        }

        #region Expression

        private IAstNode BuildExpression(IAstNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                AddError(LocalizedErrors.ExpressionExpected());
                return default;
            }

            var op = BuildLogicalOr();
            _builder.AddChild(parent, op);
            return op;
        }
        
        private IAstNode BuildExpressionUpTo(IAstNode parent, Token stopToken)
        {
            var node = BuildExpression(parent, stopToken);
            if (_lastExtractedLexem.Token != stopToken)
            {
                AddError(LocalizedErrors.TokenExpected(stopToken));
                return default;
            }
            
            return node;
        }
        
        private IAstNode BuildOptionalExpression(IAstNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                return default;
            }

            var op = BuildLogicalOr();
            _builder.AddChild(parent, op);
            return op;
        }

        #region Operators

         [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildLogicalOr()
        {
            var firstArg = BuildLogicalAnd();
            if (_lastExtractedLexem.Token == Token.Or)
            {
                var orToken = _lastExtractedLexem;
                NextLexem();
                var secondArg = BuildLogicalAnd();
                var node = _builder.CreateNode(NodeKind.BinaryOperation, orToken);
                _builder.AddChild(node, firstArg);
                _builder.AddChild(node, secondArg);

                return node;
            }
            
            return firstArg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildLogicalAnd()
        {
            var firstArg = BuildLogicalComparison();
            if (_lastExtractedLexem.Token == Token.And)
            {
                var token = _lastExtractedLexem;
                NextLexem();
                var secondArg = BuildLogicalComparison();
                var node = _builder.CreateNode(NodeKind.BinaryOperation, token);
                _builder.AddChild(node, firstArg);
                _builder.AddChild(node, secondArg);
            }

            return firstArg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildLogicalNot()
        {
            bool hasNegative = _lastExtractedLexem.Token == Token.Not;
            if (hasNegative)
            {
                var operation = _builder.CreateNode(NodeKind.UnaryOperation, _lastExtractedLexem);
                _builder.AddChild(operation, BuildAddition());
                return operation;
            }

            return BuildAddition();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildLogicalComparison()
        {
            var firstArg = BuildLogicalNot();
            var operatorSign = _lastExtractedLexem.Token;
            if (operatorSign == Token.LessThan
                || operatorSign == Token.LessOrEqual
                || operatorSign == Token.MoreThan
                || operatorSign == Token.MoreOrEqual
                || operatorSign == Token.Equal
                || operatorSign == Token.NotEqual)
            {
                var token = _lastExtractedLexem;
                NextLexem();
                var secondArg = BuildLogicalNot();
                var node = _builder.CreateNode(NodeKind.BinaryOperation, token);
                _builder.AddChild(node, firstArg);
                _builder.AddChild(node, secondArg);
                return node;
            }

            return firstArg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildAddition()
        {
            var firstArg = BuildMultiplication();
            if (_lastExtractedLexem.Token == Token.Plus || _lastExtractedLexem.Token == Token.Minus)
            {
                var token = _lastExtractedLexem;
                NextLexem();
                var secondArg = BuildMultiplication();
                var node = _builder.CreateNode(NodeKind.BinaryOperation, token);
                _builder.AddChild(node, firstArg);
                _builder.AddChild(node, secondArg);
            }

            return firstArg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildMultiplication()
        {
            var firstArg = BuildUnaryArifmetics();
            if (_lastExtractedLexem.Token == Token.Multiply 
                || _lastExtractedLexem.Token == Token.Multiply
                ||_lastExtractedLexem.Token == Token.Modulo)
            {
                var token = _lastExtractedLexem;
                NextLexem();
                var secondArg = BuildUnaryArifmetics();
                var node = _builder.CreateNode(NodeKind.BinaryOperation, token);
                _builder.AddChild(node, firstArg);
                _builder.AddChild(node, secondArg);
            }

            return firstArg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildUnaryArifmetics()
        {
            var hasUnarySign = _lastExtractedLexem.Token == Token.Minus || _lastExtractedLexem.Token == Token.Plus;
            var operation = _lastExtractedLexem;
            var arg = BuildParenthesis();
            if (hasUnarySign)
            {
                var op = _builder.CreateNode(NodeKind.UnaryOperation, _lastExtractedLexem);
                _builder.AddChild(op, arg);
                return op;
            }

            return arg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private IAstNode BuildParenthesis()
        {
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                NextLexem();
                var expr = BuildLogicalOr();
                if (_lastExtractedLexem.Token != Token.ClosePar)
                {
                    AddError(LocalizedErrors.TokenExpected(Token.ClosePar));
                }
                NextLexem();
                return expr;
            }
            else
            {
                return TerminalNode();
            }
        }

        #endregion

        private IAstNode TerminalNode()
        {
            IAstNode node = default;
            if (LanguageDef.IsLiteral(ref _lastExtractedLexem))
            {
                node = _builder.CreateNode(NodeKind.Constant, _lastExtractedLexem);
                NextLexem();
                
            }
            else if (LanguageDef.IsUserSymbol(in _lastExtractedLexem))
            {
                node = BuildGlobalCall(_lastExtractedLexem);
            }
            else if(_lastExtractedLexem.Token == Token.NewObject)
            {
                node = BuildNewObjectCreation();
            }
            else if (_lastExtractedLexem.Token == Token.Question)
            {
                node = BuildQuestionOperator();
            }
            else
            {
                AddError(LocalizedErrors.ExpressionSyntax());
            }

            return node;
        }

        private IAstNode BuildQuestionOperator()
        {
            var node = _builder.CreateNode(NodeKind.TernaryOperator, _lastExtractedLexem);
            if(!NextExpected(Token.OpenPar))
                AddError(LocalizedErrors.TokenExpected(Token.OpenPar));

            NextLexem();
            BuildExpression(node, Token.Comma);
            NextLexem();
            BuildExpression(node, Token.Comma);
            NextLexem();
            BuildExpression(node, Token.ClosePar);
            if (_lastExtractedLexem.Token != Token.ClosePar)
            {
                AddError(LocalizedErrors.TokenExpected(Token.ClosePar));
                return default;
            }
            NextLexem();

            return BuildDereference(node);
        }

        private IAstNode BuildNewObjectCreation()
        {
            var node = _builder.CreateNode(NodeKind.NewObject, _lastExtractedLexem);
            NextLexem();
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                // создание по строковому имени класса
                NewObjectDynamicConstructor(node);
            }
            else if (IsUserSymbol(_lastExtractedLexem) || _lastExtractedLexem.Token == Token.ExceptionInfo)
            {
                NewObjectStaticConstructor(node);
            }
            else
            {
                AddError(LocalizedErrors.IdentifierExpected());
                node = default;
            }

            return node;
        }
        
        private void NewObjectDynamicConstructor(IAstNode node)
        {
            var argsPassed = BuildCallParameters(node);
            if (argsPassed < 1)
            {
                AddError(LocalizedErrors.ExpressionExpected());
            }
        }

        private void NewObjectStaticConstructor(IAstNode node)
        {
            CreateChild(node, NodeKind.Identifier, _lastExtractedLexem);
            
            NextLexem();
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                BuildCallParameters(node);
            }
        }

        #endregion  
        
        private void NextLexem()
        {
            _lastExtractedLexem = _lexer.NextLexem();
        }

        private bool NextExpected(Token expected)
        {
            NextLexem();
            
            return expected == _lastExtractedLexem.Token;
        }
        
        private bool NextExpected(LexemType expected)
        {
            NextLexem();
            
            return expected == _lastExtractedLexem.Type;
        }

        private void SkipToNextStatement(Token[] additionalStops = null)
        {
            while (!(_lastExtractedLexem.Token == Token.EndOfText
                     || LanguageDef.IsBeginOfStatement(_lastExtractedLexem.Token)))
            {
                NextLexem();
                if(additionalStops != null && additionalStops.Contains(_lastExtractedLexem.Token) )
                {
                    break;
                }
            }
        }
        
        private void AddError(ParseError err)
        {
            err.Position = _lexer.GetCodePosition();
            _errors.Add(err);
            _builder.HandleParseError(err, _lastExtractedLexem, _lexer);
            if(_tokenStack.Count > 0)
                SkipToNextStatement(_tokenStack.Peek());
            else
                SkipToNextStatement();
            
            if(_enableException)
                throw new InternalParseException(err);
        }

        private static bool IsUserSymbol(in Lexem lex)
        {
            return LanguageDef.IsUserSymbol(in lex);
        }

        private void PushStructureToken(params Token[] tok)
        {
            _tokenStack.Push(tok);
        }

        private Token[] PopStructureToken()
        {
            var tok = _tokenStack.Pop();
            return tok;
        }

        private IAstNode CreateChild(IAstNode parent, NodeKind kind, in Lexem lex)
        {
            var child = _builder.CreateNode(kind, lex);
            _builder.AddChild(parent, child);
            return child;
        }

    }
}