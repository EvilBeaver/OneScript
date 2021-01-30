/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;

namespace OneScript.Language.SyntaxAnalysis
{
    public class DefaultBslParser
    {
        private readonly IAstBuilder _builder;
        private readonly ILexer _lexer;
        private readonly PreprocessorHandlers _preprocessorHandlers;

        private Lexem _lastExtractedLexem;
        
        private bool _inMethodScope;
        private bool _isMethodsDefined;
        private bool _isStatementsDefined;
        private bool _isInFunctionScope;
        private bool _lastDereferenceIsWritable;

        private readonly Stack<Token[]> _tokenStack = new Stack<Token[]>();
        private bool _isInLoopScope;
        private bool _enableException;
        
        private readonly List<BslSyntaxNode> _annotations = new List<BslSyntaxNode>();

        public DefaultBslParser(
            ILexer lexer,
            IAstBuilder astBuilder,
            IErrorSink errorSink,
            PreprocessorHandlers preprocessorHandlers)
        {
            _lexer = lexer;
            _builder = astBuilder;
            _preprocessorHandlers = preprocessorHandlers;
            ErrorSink = errorSink;
        }

        private IErrorSink ErrorSink { get; }
        
        public IEnumerable<ParseError> Errors => ErrorSink.Errors ?? new ParseError[0]; 
        
        public BslSyntaxNode ParseStatefulModule()
        {
            BslSyntaxNode node;
            
            _preprocessorHandlers.OnModuleEnter();
            NextLexem();
            
            try
            {
                node = _builder.CreateNode(NodeKind.Module, _lastExtractedLexem);
                PushContext(node);
                ParseModuleSections();
            }
            finally
            {
                PopContext();
            }

            _preprocessorHandlers.OnModuleLeave();
            
            return node;
        }

        public BslSyntaxNode ParseCodeBatch()
        {
            NextLexem();
            var node = _builder.CreateNode(NodeKind.Module, _lastExtractedLexem);
            PushContext(node);
            try
            {
                BuildModuleBody();
            }
            finally
            {
                PopContext();
            }

            return node;
        }

        public BslSyntaxNode ParseExpression()
        {
            NextLexem();
            var module = _builder.CreateNode(NodeKind.Module, _lastExtractedLexem);
            var parent = CreateChild(module, NodeKind.TopLevelExpression, _lastExtractedLexem);
            BuildExpression(parent, Token.EndOfText);
            return module;
        }

        private void PushContext(BslSyntaxNode node) => _builder.PushContext(node);
        
        private BslSyntaxNode PopContext() => _builder.PopContext();

        private BslSyntaxNode CurrentParent => _builder.CurrentNode;

        private void ParseModuleAnnotation()
        {
            if (_lastExtractedLexem.Type != LexemType.PreprocessorDirective)
                return;
            
            var annotationParser = _preprocessorHandlers
                .Slice(x => x is ModuleAnnotationDirectiveHandler)
                .Cast<ModuleAnnotationDirectiveHandler>()
                .ToList();
            
            if (!annotationParser.Any())
                return;

            while (_lastExtractedLexem.Type == LexemType.PreprocessorDirective)
            {
                bool handled = false;
                var directive = _lastExtractedLexem.Content;
                foreach (var handler in annotationParser)
                {
                    handled = handler.ParseAnnotation(ref _lastExtractedLexem, _lexer);
                    if(handled)
                        break;
                }

                if (!handled)
                {
                    AddError(LocalizedErrors.DirectiveNotSupported(directive));
                }
            }
            
            foreach (var handler in annotationParser)
            {
                handler.OnModuleLeave();
            }
        }

        private void ParseModuleSections()
        {
            ParseModuleAnnotation();
            BuildVariableSection();
            BuildMethodsSection();
            BuildModuleBody();

            if (_annotations.Count != 0)
            {
                AddError(LocalizedErrors.UnexpectedEof());
            }
        }

        #region Variables
        
        private void BuildVariableSection()
        {
            if (_lastExtractedLexem.Token != Token.VarDef && _lastExtractedLexem.Type != LexemType.Annotation)
            {
                return;
            }

            var parent = CurrentParent;
            var allVarsSection = _builder.CreateNode(NodeKind.VariablesSection, _lastExtractedLexem);
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

                ApplyAnnotations(variable);

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

        private void ApplyAnnotations(BslSyntaxNode annotatable)
        {
            foreach (var astNode in _annotations)
            {
                _builder.AddChild(annotatable, astNode);
            }
            _annotations.Clear();
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
                            _isMethodsDefined = true;
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
            
            ApplyAnnotations(method);
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
            }
            finally
            {
                PopContext();
            }
            
            CreateChild(CurrentParent, NodeKind.BlockEnd, _lastExtractedLexem);
            NextLexem();
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

        private void BuildMethodParameters(BslSyntaxNode node)
        {
            if (!NextExpected(Token.OpenPar))
            {
                AddError(LocalizedErrors.TokenExpected(Token.OpenPar));
                return;
            }

            var paramList = CreateChild(
                node, NodeKind.MethodParameters, _lastExtractedLexem);
            NextLexem(); // (

            var expectParameter = false;
            while (_lastExtractedLexem.Token != Token.ClosePar)
            {
                BuildAnnotations();
                var param = CreateChild(paramList, NodeKind.MethodParameter, _lastExtractedLexem);
                ApplyAnnotations(param);
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

                expectParameter = false;
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextLexem();
                    expectParameter = true;
                }
            }

            if (expectParameter)
            {
                AddError(LocalizedErrors.IdentifierExpected(), false);
            }
            
            NextLexem(); // )

        }

        private bool BuildDefaultParameterValue(BslSyntaxNode param)
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
            if (!_lexer.Iterator.MoveToContent())
                return;
            
            var parent = _builder.CreateNode(NodeKind.ModuleBody, _lastExtractedLexem);
            var node = CreateChild(parent, NodeKind.CodeBatch, _lastExtractedLexem);
            PushContext(node);
            try
            {
                BuildCodeBatch(Token.EndOfText);
            }
            finally
            {
                PopContext();
            }
            _builder.AddChild(CurrentParent, parent);
        }

        #region Annotations
        private void BuildAnnotations()
        {
            while (_lastExtractedLexem.Type == LexemType.Annotation)
            {
                var node = _builder.CreateNode(NodeKind.Annotation, _lastExtractedLexem);
                _annotations.Add(node);
                NextLexem();
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    NextLexem();
                    BuildAnnotationParameters(node);
                }
            }
        }
        
        private void BuildAnnotationParameters(BslSyntaxNode annotation)
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
        
        private bool BuildAnnotationParameter(BslSyntaxNode annotation)
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

        public bool BuildAnnotationParamValue(BslSyntaxNode annotationParam)
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
                    continue;
                }

                BuildStatement();

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

        #region Statements

        private void BuildStatement()
        {
            if (_lastExtractedLexem.Token == Token.NotAToken)
            {
                BuildSimpleStatement();
            }
            else
            {
                BuildComplexStructureStatement();
            }
        }

        private void BuildComplexStructureStatement()
        {
            switch (_lastExtractedLexem.Token)
            {
                case Token.If:
                    BuildIfStatement();
                    break;
                case Token.For:
                    BuildForStatement();
                    break;
                case Token.While:
                    BuildWhileStatement();
                    break;
                case Token.Break:
                    BuildBreakStatement();
                    break;
                case Token.Continue:
                    BuildContinueStatement();
                    break;
                case Token.Return:
                    BuildReturnStatement();
                    break;
                case Token.Try:
                    BuildTryExceptStatement();
                    break;
                case Token.RaiseException:
                    BuildRaiseExceptionStatement();
                    break;
                case Token.Execute:
                    BuildExecuteStatement();
                    break;
                case Token.AddHandler:
                case Token.RemoveHandler:
                    BuildEventHandlerOperation(_lastExtractedLexem.Token);
                    break;        
                default:
                    var expected = _tokenStack.Peek();
                    AddError(LocalizedErrors.TokenExpected(expected));
                    break;
            }
        }

        private void BuildIfStatement()
        {
            var condition = CreateChild(CurrentParent, NodeKind.Condition, _lastExtractedLexem);
            NextLexem();
            BuildExpressionUpTo(condition, Token.Then);
            BuildBatchWithContext(condition, Token.Else, Token.ElseIf, Token.EndIf);
            
            while (_lastExtractedLexem.Token == Token.ElseIf)
            {
                var elif = CreateChild(condition, NodeKind.Condition, _lastExtractedLexem);
                NextLexem();
                BuildExpressionUpTo(elif, Token.Then);
                BuildBatchWithContext(elif, Token.Else, Token.ElseIf, Token.EndIf);
            }

            if (_lastExtractedLexem.Token == Token.Else)
            {
                NextLexem();
                BuildBatchWithContext(condition, Token.EndIf);
            }

            CreateChild(condition, NodeKind.BlockEnd, _lastExtractedLexem);

            NextLexem();
        }

        private void BuildBatchWithContext(BslSyntaxNode context, params Token[] stopTokens)
        {
            var batch = CreateChild(context, NodeKind.CodeBatch, _lastExtractedLexem);
            PushContext(batch);
            try
            {
                BuildCodeBatch(stopTokens);
            }
            finally
            {
                PopContext();
            }
        }
        
        private void BuildWhileStatement()
        {
            var loopNode = CreateChild(CurrentParent, NodeKind.WhileLoop, _lastExtractedLexem);
            NextLexem();
            BuildExpressionUpTo(loopNode, Token.Loop);
            var body = CreateChild(loopNode, NodeKind.CodeBatch, _lastExtractedLexem);
            PushContext(body);
            var loopState = _isInLoopScope;
            try
            {
                _isInLoopScope = true;
                BuildCodeBatch(Token.EndLoop);
                NextLexem();
                CreateChild(loopNode, NodeKind.BlockEnd, _lastExtractedLexem);
            }
            finally
            {
                _isInLoopScope = loopState;
                PopContext();
            }
        }

        private void BuildForStatement()
        {
            var lexem = _lastExtractedLexem;
            NextLexem();

            var nodeType = _lastExtractedLexem.Token == Token.Each ? NodeKind.ForEachLoop : NodeKind.ForLoop;
            var loopNode = CreateChild(CurrentParent, nodeType, lexem);
            
            PushContext(loopNode);
            var loopState = _isInLoopScope;
            try
            {
                _isInLoopScope = true;
                if (nodeType == NodeKind.ForEachLoop)
                    BuildForEachStatement(loopNode);
                else
                    BuildCountableForStatement(loopNode);
            }
            finally
            {
                _isInLoopScope = loopState;
                PopContext();
            }
        }

        private void BuildCountableForStatement(BslSyntaxNode loopNode)
        {
            if (!IsUserSymbol(_lastExtractedLexem))
            {
                AddError(LocalizedErrors.IdentifierExpected());
                BuildBatchWithContext(loopNode, Token.EndLoop);
                return;
            }
            
            var counter = _lastExtractedLexem;
            if (!NextExpected(Token.Equal))
            {
                AddError(LocalizedErrors.TokenExpected(Token.Equal));
                BuildBatchWithContext(loopNode, Token.EndLoop);
                return;
            }
            
            var assignment = _builder.CreateNode(NodeKind.ForInitializer, _lastExtractedLexem);
            
            NextLexem();

            CreateChild(assignment, NodeKind.Identifier, counter);
            BuildExpressionUpTo(assignment, Token.To);
            _builder.AddChild(loopNode, assignment);
            
            var limit = _builder.CreateNode(NodeKind.ForLimit, _lastExtractedLexem);
            BuildExpressionUpTo(limit, Token.Loop);
            _builder.AddChild(loopNode, limit);
            
            BuildBatchWithContext(loopNode, Token.EndLoop);

            CreateChild(loopNode, NodeKind.BlockEnd, _lastExtractedLexem);

            NextLexem();
        }

        private void BuildForEachStatement(BslSyntaxNode loopNode)
        {
            NextLexem();
            if (!IsUserSymbol(_lastExtractedLexem))
            {
                AddError(LocalizedErrors.IdentifierExpected());
                BuildBatchWithContext(loopNode, Token.EndLoop);
                return;
            }

            CreateChild(loopNode, NodeKind.ForEachVariable, _lastExtractedLexem);
            if (!NextExpected(Token.In))
            {
                AddError(LocalizedErrors.TokenExpected(Token.In));
                BuildBatchWithContext(loopNode, Token.EndLoop);
                return;
            }

            NextLexem();
            TryParseNode(() =>
            {
                var collection = _builder.CreateNode(NodeKind.ForEachCollection, _lastExtractedLexem);
                BuildExpressionUpTo(collection, Token.Loop);
                _builder.AddChild(loopNode, collection);
            });

            BuildBatchWithContext(loopNode, Token.EndLoop);
            CreateChild(loopNode, NodeKind.BlockEnd, _lastExtractedLexem);
            
            NextLexem();
        }

        private void BuildBreakStatement()
        {
            if (!_isInLoopScope)
            {
                AddError(LocalizedErrors.BreakOutsideOfLoop());
            }

            CreateChild(CurrentParent, NodeKind.BreakStatement, _lastExtractedLexem);
            NextLexem();
        }
        
        private void BuildContinueStatement()
        {
            if (!_isInLoopScope)
            {
                AddError(LocalizedErrors.ContinueOutsideLoop());
            }

            CreateChild(CurrentParent, NodeKind.ContinueStatement, _lastExtractedLexem);
            NextLexem();
        }
        
        private void BuildReturnStatement()
        {
            var returnNode = _builder.CreateNode(NodeKind.ReturnStatement, _lastExtractedLexem);
            if (_isInFunctionScope)
            {
                NextLexem();
                if (_lastExtractedLexem.Token == Token.Semicolon ||
                    LanguageDef.IsEndOfBlockToken(_lastExtractedLexem.Token))
                {
                    AddError(LocalizedErrors.FuncEmptyReturnValue());
                }
                else
                {
                    BuildExpression(returnNode, Token.Semicolon);
                }
            }
            else if (_inMethodScope)
            {
                NextLexem();
                if (_lastExtractedLexem.Token != Token.Semicolon
                    && !LanguageDef.IsEndOfBlockToken(_lastExtractedLexem.Token))
                {
                    AddError(LocalizedErrors.ProcReturnsAValue());
                }
            }
            else
            {
                AddError(LocalizedErrors.ReturnOutsideOfMethod());
            }

            _builder.AddChild(CurrentParent, returnNode);
        }

        private void BuildTryExceptStatement()
        {
            var node = _builder.CreateNode(NodeKind.TryExcept, _lastExtractedLexem);
            NextLexem();
            BuildBatchWithContext(node, Token.Exception);
            
            Debug.Assert(_lastExtractedLexem.Token == Token.Exception);
            
            NextLexem();
            BuildBatchWithContext(node, Token.EndTry);
            CreateChild(node, NodeKind.BlockEnd, _lastExtractedLexem);
            NextLexem();
            _builder.AddChild(CurrentParent, node);
        }
        
        private void BuildRaiseExceptionStatement()
        {
            var node = _builder.CreateNode(NodeKind.RaiseException, _lastExtractedLexem);
            NextLexem();
            if (_lastExtractedLexem.Token == Token.Semicolon)
            {
                if (!_tokenStack.Any(x => x.Contains(Token.EndTry)))
                {
                    AddError(LocalizedErrors.MismatchedRaiseException());
                    return;
                }
            }
            else
            {
                BuildExpression(node, Token.Semicolon);
            }
            
            _builder.AddChild(CurrentParent, node);
        }

        private void BuildExecuteStatement()
        {
            var node = _builder.CreateNode(NodeKind.ExecuteStatement, _lastExtractedLexem);
            NextLexem();
            BuildExpression(node, Token.Semicolon);
            _builder.AddChild(CurrentParent, node);
        }

        private void BuildEventHandlerOperation(Token token)
        {
            var node = _builder.CreateNode(
                token == Token.AddHandler ? NodeKind.AddHandler : NodeKind.RemoveHandler,
                _lastExtractedLexem);
            
            NextLexem();
            
            var source = BuildExpression(node, Token.Comma);
            if (source.Kind != NodeKind.DereferenceOperation || !_lastDereferenceIsWritable)
            {
                AddError(LocalizedErrors.WrongEventName());
                return;
            }

            NextLexem();
            var expr = BuildExpression(node, Token.Semicolon);

            if (expr.Kind != NodeKind.Identifier &&
                (expr.Kind != NodeKind.DereferenceOperation || !_lastDereferenceIsWritable))
            {
                AddError(LocalizedErrors.WrongHandlerName());
                return;
            }
            
            _builder.AddChild(CurrentParent, node);
        }
        
        private void BuildSimpleStatement()
        {
            _isStatementsDefined = true;
            TryParseNode(() => BuildAssignment(CurrentParent));
        }

        private void BuildAssignment(BslSyntaxNode batch)
        {
            var call = BuildGlobalCall(_lastExtractedLexem);
            
            if (_lastExtractedLexem.Token == Token.Equal)
            {
                if (_lastDereferenceIsWritable)
                {
                    var node = CreateChild(batch, NodeKind.Assignment, _lastExtractedLexem);
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
                _builder.AddChild(batch, call);
            }
        }

        private BslSyntaxNode BuildGlobalCall(Lexem identifier)
        {
            _lastDereferenceIsWritable = true;
            var target = _builder.CreateNode(NodeKind.Identifier, identifier);
            NextLexem();
            var callNode = BuildCall(target, NodeKind.GlobalCall);
            return BuildDereference(callNode);
        }

        private BslSyntaxNode BuildCall(BslSyntaxNode target, NodeKind callKind)
        {
            BslSyntaxNode callNode = default;
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                callNode = _builder.CreateNode(callKind, _lastExtractedLexem);
                _builder.AddChild(callNode, target);
                BuildCallParameters(callNode);
            }
            return callNode ?? target;
        }

        private void BuildCallParameters(BslSyntaxNode callNode)
        {
            var node = CreateChild(callNode, NodeKind.CallArgumentList, _lastExtractedLexem);
            PushStructureToken(Token.ClosePar);
            try
            {
                NextLexem(); // съели открывающую скобку
                WalkCallArguments(node);

                NextLexem(); // съели закрывающую скобку
            }
            finally
            {
                PopStructureToken();
            }
        }

        private int WalkCallArguments(BslSyntaxNode node)
        {
            int argCount = 0;
            while (_lastExtractedLexem.Token != Token.ClosePar)
            {
                BuildCallArgument(node);
                argCount++;
            }

            if (_lastExtractedLexem.Token != Token.ClosePar)
            {
                AddError(LocalizedErrors.TokenExpected(Token.ClosePar));
                argCount = -1;
            }

            return argCount;
        }

        private void BuildCallArgument(BslSyntaxNode argsList)
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
        private void BuildLastDefaultArg(BslSyntaxNode argsList)
        {
            NextLexem();
            if (_lastExtractedLexem.Token == Token.ClosePar)
            {
                CreateChild(argsList, NodeKind.CallArgument, _lastExtractedLexem);
            }
        }

        #endregion
        
        #region Expression

        private BslSyntaxNode BuildExpression(BslSyntaxNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                AddError(LocalizedErrors.ExpressionExpected());
                return default;
            }

            var op = BuildBinaryOperation(LanguageDef.GetPriority(Token.Or));
            _builder.AddChild(parent, op);
            return op;
        }

        private BslSyntaxNode BuildBinaryOperation(int acceptablePriority)
        {
            if (acceptablePriority == LanguageDef.MAX_OPERATION_PRIORITY)
                return BuildParenthesis();

            var isUnary = LanguageDef.IsUnaryOperator(_lastExtractedLexem.Token);
            BslSyntaxNode firstArg;
            if (isUnary)
            {
                firstArg = BuildUnaryOperation();
            }
            else
            {
                firstArg = BuildBinaryOperation(acceptablePriority + 1);
            }
            
            var priority = GetBinaryPriority(_lastExtractedLexem.Token);
            while (priority >= acceptablePriority)
            {
                var token = _lastExtractedLexem;
                NextLexem();
                var secondArg = BuildBinaryOperation(acceptablePriority + 1);
                priority = GetBinaryPriority(_lastExtractedLexem.Token);
                firstArg = MakeBinaryOperationNode(firstArg, secondArg, token);
            }

            return firstArg;
        }

        private static int GetBinaryPriority(Token newOp)
        {
            int newPriority;
            if (LanguageDef.IsBinaryOperator(newOp))
                newPriority = LanguageDef.GetPriority(newOp);
            else
                newPriority = -1;

            return newPriority;
        }
        
        private BslSyntaxNode BuildUnaryOperation()
        {
            if (_lastExtractedLexem.Token == Token.Plus)
                _lastExtractedLexem.Token = Token.UnaryPlus;
            else if (_lastExtractedLexem.Token == Token.Minus)
                _lastExtractedLexem.Token = Token.UnaryMinus;

            var unaryPriority = LanguageDef.GetPriority(_lastExtractedLexem.Token);
            var operation = _lastExtractedLexem;
            NextLexem();
            var argument = BuildBinaryOperation(unaryPriority);
            var op = _builder.CreateNode(NodeKind.UnaryOperation, operation);
            _builder.AddChild(op, argument);
            return op;
        }
        
        private BslSyntaxNode BuildExpressionUpTo(BslSyntaxNode parent, Token stopToken)
        {
            var node = BuildExpression(parent, stopToken);
            if (_lastExtractedLexem.Token == stopToken)
            {
                NextLexem();
            }
            
            return node;
        }
        
        private void BuildOptionalExpression(BslSyntaxNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                return;
            }

            var op = BuildBinaryOperation(LanguageDef.GetPriority(Token.Or));
            _builder.AddChild(parent, op);
        }

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BslSyntaxNode MakeBinaryOperationNode(BslSyntaxNode firstArg, BslSyntaxNode secondArg, in Lexem lexem)
        {
            var node = _builder.CreateNode(NodeKind.BinaryOperation, lexem);
            _builder.AddChild(node, firstArg);
            _builder.AddChild(node, secondArg);
            return node;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private BslSyntaxNode BuildParenthesis()
        {
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                NextLexem();
                var expr = BuildBinaryOperation(LanguageDef.GetPriority(Token.Or));
                if (_lastExtractedLexem.Token != Token.ClosePar)
                {
                    AddError(LocalizedErrors.TokenExpected(Token.ClosePar));
                }
                NextLexem();
                
                return BuildDereference(expr);
            }

            return TerminalNode();
        }

        #endregion

        private BslSyntaxNode TerminalNode()
        {
            BslSyntaxNode node = default;
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
            else if (LanguageDef.IsBuiltInFunction(_lastExtractedLexem.Token))
            {
                node = BuildGlobalCall(_lastExtractedLexem);
            }
            else
            {
                AddError(LocalizedErrors.ExpressionSyntax());
            }

            return node;
        }

        private BslSyntaxNode BuildQuestionOperator()
        {
            var node = _builder.CreateNode(NodeKind.TernaryOperator, _lastExtractedLexem);
            if(!NextExpected(Token.OpenPar))
                AddError(LocalizedErrors.TokenExpected(Token.OpenPar));

            if (!TryParseNode(() =>
            {
                NextLexem();
                BuildExpression(node, Token.Comma);
                NextLexem();
                BuildExpression(node, Token.Comma);
                NextLexem();
                BuildExpression(node, Token.ClosePar);
            }))
            {
                return default;
            }

            if (_lastExtractedLexem.Token != Token.ClosePar)
            {
                AddError(LocalizedErrors.TokenExpected(Token.ClosePar));
                return default;
            }
            NextLexem();

            return BuildDereference(node);
        }

        private BslSyntaxNode BuildDereference(BslSyntaxNode target)
        {
            var activeTarget = BuildIndexerAccess(target);
            if (_lastExtractedLexem.Token == Token.Dot)
            {
                var dotNode = _builder.CreateNode(NodeKind.DereferenceOperation, _lastExtractedLexem);
                _builder.AddChild(dotNode, activeTarget);
                NextLexem();
                if (!LanguageDef.IsValidPropertyName(_lastExtractedLexem))
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
                    var call = BuildCall(ident, NodeKind.MethodCall);
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

        private BslSyntaxNode BuildIndexerAccess(BslSyntaxNode target)
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
        
        private BslSyntaxNode BuildNewObjectCreation()
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

            return BuildDereference(node);
        }
        
        private void NewObjectDynamicConstructor(BslSyntaxNode node)
        {
            NextLexem();
            if (_lastExtractedLexem.Token == Token.ClosePar)
            {
                AddError(LocalizedErrors.ExpressionExpected());
                return;
            }

            var nameArg = _builder.CreateNode(NodeKind.CallArgument, _lastExtractedLexem);
            PushStructureToken(Token.ClosePar);
            try
            {
                BuildExpressionUpTo(nameArg, Token.Comma);
                _builder.AddChild(node, nameArg);
                var callArgs = _builder.CreateNode(NodeKind.CallArgumentList, _lastExtractedLexem);
                WalkCallArguments(callArgs);
                _builder.AddChild(node, callArgs);
                NextLexem();
            }
            finally
            {
                PopStructureToken();
            }
        }

        private void NewObjectStaticConstructor(BslSyntaxNode node)
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
        
        private void SkipToNextStatement(Token[] additionalStops = null)
        {
            var recovery = new NextStatementRecoveryStrategy
            {
                AdditionalStops = additionalStops
            };

            _lastExtractedLexem = recovery.Recover(_lexer);
        }

        private void AddError(ParseError err, bool doFastForward = true)
        {
            err.Position = _lexer.GetErrorPosition();
            ErrorSink.AddError(err);

            if (doFastForward)
            {
                if (_tokenStack.Count > 0)
                    SkipToNextStatement(_tokenStack.Peek());
                else
                    SkipToNextStatement();
            }

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

        private BslSyntaxNode CreateChild(BslSyntaxNode parent, NodeKind kind, in Lexem lex)
        {
            var child = _builder.CreateNode(kind, lex);
            _builder.AddChild(parent, child);
            return child;
        }

        private bool TryParseNode(Action action)
        {
            var exc = _enableException;
            try
            {
                _enableException = true;
                action();
                return true;
            }
            catch (InternalParseException)
            {
                return false;
            }
            finally
            {
                _enableException = exc;
            }
        }
    }
}