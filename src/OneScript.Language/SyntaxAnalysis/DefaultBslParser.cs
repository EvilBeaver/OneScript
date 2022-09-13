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
        private readonly ParserContext _nodeContext;
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
            IErrorSink errorSink,
            PreprocessorHandlers preprocessorHandlers)
        {
            _lexer = lexer;
            _preprocessorHandlers = preprocessorHandlers;
            ErrorSink = errorSink;
            _nodeContext = new ParserContext();
        }

        private IErrorSink ErrorSink { get; }
        
        public IEnumerable<CodeError> Errors => ErrorSink.Errors ?? new CodeError[0]; 
        
        public BslSyntaxNode ParseStatefulModule()
        {
            ModuleNode node;
            
            _preprocessorHandlers.OnModuleEnter();
            NextLexem();
            
            try
            {
                node = new ModuleNode(_lastExtractedLexem);
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

        public BslSyntaxNode ParseCodeBatch(bool allowReturns = false)
        {
            NextLexem();
            var node = new ModuleNode(_lastExtractedLexem);
            PushContext(node);
            try
            {
                if (allowReturns)
                {
                    _inMethodScope = true;
                    _isInFunctionScope = true;
                }
                BuildModuleBody();
            }
            finally
            {
                PopContext();
                _inMethodScope = false;
                _isInFunctionScope = false;
            }

            return node;
        }

        public BslSyntaxNode ParseExpression()
        {
            NextLexem();
            var module = new ModuleNode(_lastExtractedLexem);
            var parent = module.AddNode(new NonTerminalNode(NodeKind.TopLevelExpression, _lastExtractedLexem));
            BuildExpression(parent, Token.EndOfText);
            return module;
        }

        private void PushContext(NonTerminalNode node) => _nodeContext.PushContext(node);
        
        private NonTerminalNode PopContext() => _nodeContext.PopContext();

        private NonTerminalNode CurrentParent => _nodeContext.CurrentParent;

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
                    handled = handler.ParseAnnotation(ref _lastExtractedLexem, _lexer, _nodeContext);
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
            var allVarsSection = new NonTerminalNode(NodeKind.VariablesSection, _lastExtractedLexem);
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
                            parent.AddChild(allVarsSection);
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
                var variable = _nodeContext.AddChild(new VariableDefinitionNode(_lastExtractedLexem));
                
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

        private void ApplyAnnotations(AnnotatableNode annotatable)
        {
            foreach (var astNode in _annotations)
            {
                annotatable.AddChild(astNode);
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
            var allMethodsSection = new NonTerminalNode(NodeKind.MethodsSection, _lastExtractedLexem);
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
                            parent.AddChild(allMethodsSection);
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

            var method = _nodeContext.AddChild(new MethodNode());
            
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
            PushContext((NonTerminalNode)body);
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

            var signature = _nodeContext.AddChild(new MethodSignatureNode(_lastExtractedLexem));
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

        private void BuildMethodParameters(MethodSignatureNode signature)
        {
            if (!NextExpected(Token.OpenPar))
            {
                AddError(LocalizedErrors.TokenExpected(Token.OpenPar));
                return;
            }

            var paramList = new NonTerminalNode(NodeKind.MethodParameters, _lastExtractedLexem);
            signature.AddChild(paramList);
                
            NextLexem(); // (

            var expectParameter = false;
            while (_lastExtractedLexem.Token != Token.ClosePar)
            {
                BuildAnnotations();
                var param = new MethodParameterNode();
                paramList.AddChild(param);
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

        private bool BuildDefaultParameterValue(NonTerminalNode param)
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
            
            var moduleBody = new NonTerminalNode(NodeKind.ModuleBody, _lastExtractedLexem);
            var node = moduleBody.AddNode(new CodeBatchNode(_lastExtractedLexem));
            PushContext(node);
            try
            {
                BuildCodeBatch(Token.EndOfText);
            }
            finally
            {
                PopContext();
            }
            CurrentParent.AddChild(moduleBody);
        }

        #region Annotations
        private void BuildAnnotations()
        {
            while (_lastExtractedLexem.Type == LexemType.Annotation)
            {
                var node = new AnnotationNode(NodeKind.Annotation, _lastExtractedLexem);
                _annotations.Add(node);
                NextLexem();
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    NextLexem();
                    BuildAnnotationParameters(node);
                }
            }
        }
        
        private void BuildAnnotationParameters(AnnotationNode annotation)
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
        
        private bool BuildAnnotationParameter(AnnotationNode annotation)
        {
            bool success = true;
            var node = annotation.AddNode(new AnnotationParameterNode());
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

        private bool BuildAnnotationParamValue(AnnotationParameterNode annotationParam)
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
                    if (!endTokens.Contains(_lastExtractedLexem.Token))
                    {
                        AddError(LocalizedErrors.SemicolonExpected());
                    }
                    break;
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
            var condition = _nodeContext.AddChild(new ConditionNode(_lastExtractedLexem)); 

            NextLexem();
            BuildExpressionUpTo(condition, Token.Then);
            BuildBatchWithContext(condition, Token.Else, Token.ElseIf, Token.EndIf);
            
            while (_lastExtractedLexem.Token == Token.ElseIf)
            {
                var elif = new ConditionNode(_lastExtractedLexem);
                condition.AddChild(elif);
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

        private void BuildBatchWithContext(NonTerminalNode context, params Token[] stopTokens)
        {
            var batch = new CodeBatchNode(_lastExtractedLexem);
            context.AddChild(batch);
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
            var loopNode = _nodeContext.AddChild(new WhileLoopNode(_lastExtractedLexem));
            NextLexem();
            BuildExpressionUpTo(loopNode, Token.Loop);
            var body = CreateChild(loopNode, NodeKind.CodeBatch, _lastExtractedLexem);
            PushContext((NonTerminalNode)body);
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

            NodeKind loopKind;
            NonTerminalNode loopNode;
            if (_lastExtractedLexem.Token == Token.Each)
            {
                loopKind = NodeKind.ForEachLoop;
                loopNode = _nodeContext.AddChild(new ForEachLoopNode(_lastExtractedLexem));
            }
            else
            {
                loopKind = NodeKind.ForLoop;
                loopNode = _nodeContext.AddChild(new ForLoopNode(_lastExtractedLexem));
            }
            
            PushContext(loopNode);
            var loopState = _isInLoopScope;
            try
            {
                _isInLoopScope = true;
                if (loopKind == NodeKind.ForEachLoop)
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

        private void BuildCountableForStatement(NonTerminalNode loopNode)
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
            
            var assignment = new NonTerminalNode(NodeKind.ForInitializer, _lastExtractedLexem);
            
            NextLexem();

            CreateChild(assignment, NodeKind.Identifier, counter);
            BuildExpressionUpTo(assignment, Token.To);
            loopNode.AddChild(assignment);
            
            var limit = new NonTerminalNode(NodeKind.ForLimit, _lastExtractedLexem);
            BuildExpressionUpTo(limit, Token.Loop);
            loopNode.AddChild(limit);
            
            BuildBatchWithContext(loopNode, Token.EndLoop);

            CreateChild(loopNode, NodeKind.BlockEnd, _lastExtractedLexem);

            NextLexem();
        }

        private void BuildForEachStatement(NonTerminalNode loopNode)
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
                var collection = new NonTerminalNode(NodeKind.ForEachCollection, _lastExtractedLexem);
                BuildExpressionUpTo(collection, Token.Loop);
                loopNode.AddChild(collection);
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
            var returnNode = new NonTerminalNode(NodeKind.ReturnStatement, _lastExtractedLexem);
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

            CurrentParent.AddChild(returnNode);
        }

        private void BuildTryExceptStatement()
        {
            var node = new TryExceptNode(_lastExtractedLexem);
            NextLexem();
            BuildBatchWithContext(node, Token.Exception);
            
            Debug.Assert(_lastExtractedLexem.Token == Token.Exception);
            
            NextLexem();
            BuildBatchWithContext(node, Token.EndTry);
            CreateChild(node, NodeKind.BlockEnd, _lastExtractedLexem);
            NextLexem();
            CurrentParent.AddChild(node);
        }
        
        private void BuildRaiseExceptionStatement()
        {
            var node = new NonTerminalNode(NodeKind.RaiseException, _lastExtractedLexem);
            NextLexem();
            if (_lastExtractedLexem.Token == Token.Semicolon || LanguageDef.IsEndOfBlockToken(_lastExtractedLexem.Token))
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
            
            CurrentParent.AddChild(node);
        }

        private void BuildExecuteStatement()
        {
            var node = new NonTerminalNode(NodeKind.ExecuteStatement, _lastExtractedLexem);
            NextLexem();
            BuildExpression(node, Token.Semicolon);
            CurrentParent.AddChild(node);
        }

        private void BuildEventHandlerOperation(Token token)
        {
            var node = new NonTerminalNode(
                token == Token.AddHandler ? NodeKind.AddHandler : NodeKind.RemoveHandler,
                _lastExtractedLexem);

            NextLexem();
            
            var source = BuildExpressionUpTo(node, Token.Comma);
            if (source == null)
                return;

            if (source.Kind != NodeKind.DereferenceOperation || !_lastDereferenceIsWritable)
            {
                AddError(LocalizedErrors.WrongEventName());
                return;
            }

            var expr = BuildExpression(node, Token.Semicolon);

            if (expr.Kind != NodeKind.Identifier &&
                (expr.Kind != NodeKind.DereferenceOperation || !_lastDereferenceIsWritable))
            {
                AddError(LocalizedErrors.WrongHandlerName());
                return;
            }
            
            CurrentParent.AddChild(node);
        }
        
        private void BuildSimpleStatement()
        {
            _isStatementsDefined = true;
            TryParseNode(() => BuildAssignment(CurrentParent));
        }

        private void BuildAssignment(NonTerminalNode batch)
        {
            var call = BuildGlobalCall(_lastExtractedLexem);
            
            if (_lastExtractedLexem.Token == Token.Equal)
            {
                if (_lastDereferenceIsWritable)
                {
                    var node = batch.AddNode(new NonTerminalNode(NodeKind.Assignment, _lastExtractedLexem));
                    node.AddChild(call);
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
                batch.AddChild(call);
            }
        }

        private BslSyntaxNode BuildGlobalCall(Lexem identifier)
        {
            _lastDereferenceIsWritable = true;
            var target = NodeBuilder.CreateNode(NodeKind.Identifier, identifier);
            NextLexem();
            var callNode = BuildCall(target, NodeKind.GlobalCall);
            return BuildDereference(callNode);
        }

        private BslSyntaxNode BuildCall(BslSyntaxNode target, NodeKind callKind)
        {
            if (_lastExtractedLexem.Token != Token.OpenPar) 
                return target;
            
            var callNode = new CallNode(callKind, _lastExtractedLexem);
            callNode.AddChild(target);
            BuildCallParameters(callNode);
            return callNode;
        }

        private void BuildCallParameters(NonTerminalNode callNode)
        {
            var node = callNode.AddNode(new NonTerminalNode(NodeKind.CallArgumentList, _lastExtractedLexem));
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

        private int WalkCallArguments(NonTerminalNode node)
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

        private void BuildCallArgument(NonTerminalNode argsList)
        {
            if (_lastExtractedLexem.Token == Token.Comma)
            {
                CreateChild(argsList, NodeKind.CallArgument, _lastExtractedLexem);
                
                BuildLastDefaultArg(argsList);
            }
            else if (_lastExtractedLexem.Token != Token.ClosePar)
            {
                var node = argsList.AddNode(new NonTerminalNode(NodeKind.CallArgument, _lastExtractedLexem));
                BuildOptionalExpression(node, Token.Comma);
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    BuildLastDefaultArg(argsList);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildLastDefaultArg(NonTerminalNode argsList)
        {
            NextLexem();
            if (_lastExtractedLexem.Token == Token.ClosePar)
            {
                CreateChild(argsList, NodeKind.CallArgument, _lastExtractedLexem);
            }
        }

        #endregion
        
        #region Expression

        private BslSyntaxNode BuildExpression(NonTerminalNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                AddError(LocalizedErrors.ExpressionExpected());
                return default;
            }

            var op = BuildBinaryOperation(LanguageDef.GetPriority(Token.Or));
            parent.AddChild(op);
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
            var op = new UnaryOperationNode(operation);
            op.AddChild(argument);
            return op;
        }
        
        private BslSyntaxNode BuildExpressionUpTo(NonTerminalNode parent, Token stopToken)
        {
            var node = BuildExpression(parent, stopToken);
            if (_lastExtractedLexem.Token == stopToken)
            {
                NextLexem();
            }
            else
            {
                if (_lastExtractedLexem.Token == Token.EndOfText)
                    AddError(LocalizedErrors.UnexpectedEof());
                else
                    AddError(LocalizedErrors.ExpressionSyntax());

                node = default;
            }

            return node;
        }
        
        private void BuildOptionalExpression(NonTerminalNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                return;
            }

            var op = BuildBinaryOperation(LanguageDef.GetPriority(Token.Or));
            parent.AddChild(op);
        }

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static BslSyntaxNode MakeBinaryOperationNode(BslSyntaxNode firstArg, BslSyntaxNode secondArg, in Lexem lexem)
        {
            var node = new BinaryOperationNode(lexem);
            node.AddChild(firstArg);
            node.AddChild(secondArg);
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
                node = NodeBuilder.CreateNode(NodeKind.Constant, _lastExtractedLexem);
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
            var node = new NonTerminalNode(NodeKind.TernaryOperator, _lastExtractedLexem);
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
                var dotNode = new NonTerminalNode(NodeKind.DereferenceOperation, _lastExtractedLexem);
                dotNode.AddChild(activeTarget);
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
                    var ident = NodeBuilder.CreateNode(NodeKind.Identifier, identifier);
                    var call = BuildCall(ident, NodeKind.MethodCall);
                    dotNode.AddChild(call);
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
                var node = new NonTerminalNode(NodeKind.IndexAccess, _lastExtractedLexem);
                node.AddChild(target);
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
            var node = new NewObjectNode(_lastExtractedLexem);
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
        
        private void NewObjectDynamicConstructor(NonTerminalNode node)
        {
            NextLexem();
            if (_lastExtractedLexem.Token == Token.ClosePar)
            {
                AddError(LocalizedErrors.ExpressionExpected());
                return;
            }

            var nameArg = new NonTerminalNode(NodeKind.CallArgument, _lastExtractedLexem);
            PushStructureToken(Token.ClosePar);
            try
            {
                BuildExpression(nameArg, Token.Comma);
                node.AddChild(nameArg);
                var callArgs = new NonTerminalNode(NodeKind.CallArgumentList, _lastExtractedLexem);
                WalkCallArguments(callArgs);
                node.AddChild(callArgs);
                NextLexem();
            }
            finally
            {
                PopStructureToken();
            }
        }

        private void NewObjectStaticConstructor(NonTerminalNode node)
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

        private void AddError(CodeError err, bool doFastForward = true)
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

        private BslSyntaxNode CreateChild(NonTerminalNode parent, NodeKind kind, in Lexem lex)
        {
            var child = NodeBuilder.CreateNode(kind, lex);
            parent.AddChild(child);
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