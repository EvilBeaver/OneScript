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
        private bool _isInAsyncMethod;
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

        public IEnumerable<CodeError> Errors => ErrorSink.Errors ?? Array.Empty<CodeError>();

        public BslSyntaxNode ParseStatefulModule()
        {
            ModuleNode node;

            _preprocessorHandlers.OnModuleEnter();
            NextLexem();

            node = new ModuleNode(_lexer.Iterator.Source, _lastExtractedLexem);
            PushContext(node);
            try
            {
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
            var node = new ModuleNode(_lexer.Iterator.Source, _lastExtractedLexem);
            PushContext(node);
            try
            {
                _inMethodScope = allowReturns;
                _isInFunctionScope = allowReturns;

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
            var module = new ModuleNode(_lexer.Iterator.Source, _lastExtractedLexem);
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

            if (annotationParser.Count == 0)
                return;

            while (_lastExtractedLexem.Type == LexemType.PreprocessorDirective)
            {
                bool handled = false;
                var directive = _lastExtractedLexem.Content;
                foreach (var handler in annotationParser)
                {
                    handled = handler.ParseAnnotation(ref _lastExtractedLexem, _lexer, _nodeContext);
                    if (handled)
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
            BuildVariablesSection();
            BuildMethodsSection();
            if (_annotations.Count != 0)
            {
                AddError(LocalizedErrors.AnnotationNotAllowed());
            }
            BuildModuleBody();
        }

        #region Variables

        private void BuildVariablesSection()
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

                    if (_lastExtractedLexem.Token != Token.VarDef)
                        break;

                    if (!hasVars)
                    {
                        hasVars = true;
                        parent.AddChild(allVarsSection);
                    }

                    BuildVariablesDefinition();
                }
            }
            finally
            {
                PopContext();
            }

        }

        private void BuildVariablesDefinition()
        {
            if (_inMethodScope)
            {
                if (_isStatementsDefined)
                {
                    AddError(LocalizedErrors.LateVarDefinition());
                    return;
                }
            }
            else if (_isMethodsDefined)
            {
                AddError(LocalizedErrors.LateVarDefinition());
                return;
            }

            while (true)
            {
                NextLexem(); // skip opening VarDef or Comma

                if (!IsUserSymbol(_lastExtractedLexem))
                {
                    if(_lastExtractedLexem.Type == LexemType.Annotation)
                        AddError(LocalizedErrors.AnnotationNotAllowed());
                    else
                        AddError(LocalizedErrors.IdentifierExpected());
                    return;
                }

                BuildVariable();

                if (_lastExtractedLexem.Token == Token.Semicolon)
                {
                    break;
                }

                if (_lastExtractedLexem.Token != Token.Comma)
                {
                    AddError(LocalizedErrors.SemicolonExpected());
                    return;
                }
            }

            NextLexem(); // skip Semicolon
            _annotations.Clear();
        }

        private void BuildVariable()
        {
            var variable = _nodeContext.AddChild(new VariableDefinitionNode(_lastExtractedLexem));
            if (!_inMethodScope)
                foreach (var astNode in _annotations)
                {
                    variable.AddChild(astNode);
                }

            var symbolicName = _lastExtractedLexem.Content;
            CreateChild(variable, NodeKind.Identifier, _lastExtractedLexem);

            NextLexem();
            if (_lastExtractedLexem.Token == Token.Export)
            {
                if (_inMethodScope)
                {
                    AddError(LocalizedErrors.ExportedLocalVar(symbolicName));
                    return;
                }
                CreateChild(variable, NodeKind.ExportFlag, _lastExtractedLexem);
                NextLexem();
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
                && !IsStartOfMethod(_lastExtractedLexem))
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
                    if (!IsStartOfMethod(_lastExtractedLexem))
                        break;

                    if (!sectionExist)
                    {
                        sectionExist = true;
                        _isMethodsDefined = true;
                        parent.AddChild(allMethodsSection);
                    }

                    BuildMethod();
                }
            }
            finally
            {
                PopContext();
            }
        }

        private static bool IsStartOfMethod(in Lexem lex)
        {
            return lex.Token == Token.Async || lex.Token == Token.Procedure || lex.Token == Token.Function;
        }

        private void BuildMethod()
        {
            Debug.Assert(IsStartOfMethod(_lastExtractedLexem));

            var method = _nodeContext.AddChild(new MethodNode());

            ApplyAnnotations(method);
            PushContext(method);
            if (_lastExtractedLexem.Token == Token.Async)
            {
                method.IsAsync = true;
                _isInAsyncMethod = true;
                NextLexem();
            }

            try
            {
                BuildMethodSignature();
                _inMethodScope = true;
                BuildMethodVariablesSection();
                _isStatementsDefined = true;
                BuildMethodBody();
            }
            finally
            {
                _isInFunctionScope = false;
                _inMethodScope = false;
                _isStatementsDefined = false;
                _isInAsyncMethod = false;
                PopContext();
            }
        }

        private void BuildMethodVariablesSection()
        {
            try
            {
                // для корректной перемотки вперед в случае ошибок в секции переменных
                PushStructureToken(_isInFunctionScope ? Token.EndFunction : Token.EndProcedure);
                BuildVariablesSection();
            }
            finally
            {
                PopStructureToken();
            }
        }

        private void BuildMethodBody()
        {
            var body = _nodeContext.AddChild(new CodeBatchNode(_lastExtractedLexem));
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
            var signature = _nodeContext.AddChild(new MethodSignatureNode(_lastExtractedLexem));
            var isFunction = _lastExtractedLexem.Token == Token.Function;
            CreateChild(signature, isFunction ? NodeKind.Function : NodeKind.Procedure, _lastExtractedLexem);
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

            if (_lastExtractedLexem.Token != Token.ClosePar)
                while (true)
                {
                    BuildMethodParameter(paramList);

                    if (_lastExtractedLexem.Token == Token.ClosePar)
                    {
                        break;
                    }

                    if (_lastExtractedLexem.Token == Token.Comma)
                    {
                        NextLexem();
                    }
                    else
                    {
                        AddError(LocalizedErrors.TokenExpected(Token.ClosePar));
                        return;
                    }
                }

            NextLexem(); // )
        }

        private void BuildMethodParameter(NonTerminalNode paramList)
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
                BuildDefaultParameterValue(param, NodeKind.ParameterDefaultValue);
            }
        }

        private bool BuildDefaultParameterValue(NonTerminalNode param, NodeKind nodeKind)
        {
            bool hasSign = false;
            bool signIsMinus = _lastExtractedLexem.Token == Token.Minus;
            if (signIsMinus || _lastExtractedLexem.Token == Token.Plus)
            {
                hasSign = true;
                NextLexem();
            }

            if (LanguageDef.IsLiteral(_lastExtractedLexem))
            {
                if (hasSign)
                {
                    if (_lastExtractedLexem.Type == LexemType.NumberLiteral && signIsMinus)
                    {
                        _lastExtractedLexem.Content = '-' + _lastExtractedLexem.Content;
                    }
                    else if (_lastExtractedLexem.Type == LexemType.StringLiteral
                             || _lastExtractedLexem.Type == LexemType.DateLiteral)
                    {
                        AddError(LocalizedErrors.NumberExpected());
                        return false;
                    }
                }

                CreateChild(param, nodeKind, _lastExtractedLexem);
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
                if (_inMethodScope)
                {
                    AddError(LocalizedErrors.AnnotationNotAllowed());
                    return;
                }

                var node = BuildAnnotationDefinition();
                _annotations.Add(node);
            }
        }

        private AnnotationNode BuildAnnotationDefinition() {
            var node = new AnnotationNode(NodeKind.Annotation, _lastExtractedLexem);
            NextLexem();
            BuildAnnotationParameters(node);
            return node;
        }

        private void BuildAnnotationParameters(AnnotationNode annotation)
        {
            if (_lastExtractedLexem.Token != Token.OpenPar)
                return;

            NextLexem();

            if (_lastExtractedLexem.Token != Token.ClosePar)
            while (true)
            {
                BuildAnnotationParameter(annotation);

                if (_lastExtractedLexem.Token == Token.ClosePar)
                {
                    break;
                }

                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextLexem();
                }
                else
                {
                    AddError(LocalizedErrors.TokenExpected(Token.ClosePar), false);
                    return;
                }
            }

            NextLexem(); // )
        }

        private void BuildAnnotationParameter(AnnotationNode annotation)
        {
            bool success = true;
            var node = new AnnotationParameterNode();
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

            if (success)
            {
                annotation.AddChild(node);
            }
        }

        private bool BuildAnnotationParamValue(AnnotationParameterNode annotationParam)
        {
            if (_lastExtractedLexem.Type == LexemType.Annotation) {
                var annotation = BuildAnnotationDefinition();
                annotationParam.AddChild(annotation);
                return true;
            }
            return BuildDefaultParameterValue(annotationParam, NodeKind.AnnotationParameterValue);
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

                if (_lastExtractedLexem.Type == LexemType.Label)
                {
                    DefineLabel(_lastExtractedLexem);
                    continue;
                }
                
                if (_lastExtractedLexem.Type != LexemType.Identifier && _lastExtractedLexem.Token != Token.EndOfText)
                {
                    if (_lastExtractedLexem.Type == LexemType.Annotation)
                        AddError(LocalizedErrors.AnnotationNotAllowed());
                    else
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

        private void DefineLabel(Lexem label)
        {
            var node = new LabelNode(label);
            CurrentParent.AddChild(node);
            NextLexem();
        }

        #region Statements

        private void BuildStatement()
        {
            if (!_isInAsyncMethod && (_lastExtractedLexem.Token == Token.Async || _lastExtractedLexem.Token == Token.Await))
            {
                _lastExtractedLexem.Token = Token.NotAToken;
            }
            
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
                case Token.Await:
                    BuildGlobalCallAwaitOperator();
                    break;
                case Token.Goto:
                    BuildGotoOperator();
                    break;
                default:
                    if (LanguageDef.IsBuiltInFunction(_lastExtractedLexem.Token))
                    {
                        AddError(LocalizedErrors.UseBuiltInFunctionAsProcedure());
                    }
                    else
                    {
                        AddError(LocalizedErrors.TokenExpected(_tokenStack.Peek()));
                    }
                    break;
            }
        }

        private void BuildGlobalCallAwaitOperator()
        {
            Debug.Assert(_lastExtractedLexem.Token == Token.Await);

            CurrentParent.AddChild(TerminalNode());
        }


        private BslSyntaxNode BuildExpressionAwaitOperator(Lexem lexem)
        {
            Debug.Assert(_lastExtractedLexem.Token == Token.Await);
            
            NextLexem();

            var argument = SelectTerminalNode(_lastExtractedLexem, false);
            if (argument != default)
            {
                CheckAsyncMethod();
                return new UnaryOperationNode(argument, lexem);
            }
            else if (!_isInAsyncMethod)
            {
                // это просто переменная Ждать или метод Ждать
                return CallOrVariable(lexem);
            }
            else
            {
                return CreateError(LocalizedErrors.ExpressionSyntax());
            }
        }

        private void BuildGotoOperator()
        {
            var gotoNode = new NonTerminalNode(NodeKind.Goto, _lastExtractedLexem);
            NextLexem();

            if (_lastExtractedLexem.Type != LexemType.LabelRef)
            {
                AddError(LocalizedErrors.LabelNameExpected());
                return;
            }
            
            gotoNode.AddChild(new LabelNode(_lastExtractedLexem));
            NextLexem();

            CurrentParent.AddChild(gotoNode);
        }
        
        private void CheckAsyncMethod()
        {
            if (!_isInAsyncMethod)
            {
                AddError(LocalizedErrors.AwaitMustBeInAsyncMethod(), false);
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
            var body = loopNode.AddNode(new CodeBatchNode(_lastExtractedLexem));

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

            if ((source.Kind != NodeKind.DereferenceOperation || !_lastDereferenceIsWritable) 
                && source.Kind != NodeKind.IndexAccess)
            {
                AddError(LocalizedErrors.WrongEventName());
                return;
            }

            var expr = BuildExpression(node, Token.Semicolon);

            if (expr.Kind != NodeKind.Identifier &&
                (expr.Kind != NodeKind.DereferenceOperation || !_lastDereferenceIsWritable) &&
                expr.Kind != NodeKind.IndexAccess)
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
                if (_lastDereferenceIsWritable)
                {
                    AddError(LocalizedErrors.ExpressionSyntax());
                }
                else
                {
                    batch.AddChild(call);
                }
            }
        }

        private BslSyntaxNode BuildGlobalCall(Lexem identifier)
        {
            NextLexem();

            return CallOrVariable(identifier);
        }

        private BslSyntaxNode CallOrVariable(Lexem identifier)
        {
            BslSyntaxNode target = new TerminalNode(NodeKind.Identifier, identifier);
            if (_lastExtractedLexem.Token != Token.OpenPar)
            {
                _lastDereferenceIsWritable = true; // одиночный идентификатор
            }
            else
            {
                target = BuildCall(target, NodeKind.GlobalCall);
            }

            return BuildDereference(target);
        }

        private CallNode BuildCall(BslSyntaxNode target, NodeKind callKind)
        {
            var callNode = new CallNode(callKind, _lastExtractedLexem);
            callNode.AddChild(target);
            BuildCallParameters(callNode);
            _lastDereferenceIsWritable = false;
            return callNode;
        }

        private void BuildCallParameters(NonTerminalNode callNode)
        {
            var node = callNode.AddNode(new NonTerminalNode(NodeKind.CallArgumentList, _lastExtractedLexem));
            PushStructureToken(Token.ClosePar);
            try
            {
                NextLexem(); // съели открывающую скобку
                BuildCallArguments(node);
                NextLexem(); // съели закрывающую скобку
            }
            finally
            {
                PopStructureToken();
            }
        }

        private void BuildCallArguments(NonTerminalNode node)
        {
            if (_lastExtractedLexem.Token != Token.ClosePar)
            while (true)
            {
                BuildOptionalCallArgument(node);

                if (_lastExtractedLexem.Token == Token.ClosePar)
                {
                    break;
                }

                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextLexem();
                }
                else
                {
                    AddError(LocalizedErrors.TokenExpected(Token.ClosePar));
                    return;
                }
            }
        }

        private void BuildOptionalCallArgument(NonTerminalNode argsList)
        {
            var arg = argsList.AddNode(new NonTerminalNode(NodeKind.CallArgument, _lastExtractedLexem));
            if (_lastExtractedLexem.Token == Token.Comma
                || _lastExtractedLexem.Token == Token.ClosePar)
            {
                return;
            }

            arg.AddNode( BuildExpression(0) );
        }

        #endregion
        
        #region Expression

        private BslSyntaxNode BuildExpression(NonTerminalNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                return CreateError(LocalizedErrors.ExpressionExpected());
            }

            var op = BuildExpression(0);
            parent.AddChild(op);
            return op;
        }

        private BslSyntaxNode BuildExpression(int prio)
        {
            var firstArg = BuildPrimaryExpression();
            while (LanguageDef.GetBinaryPriority(_lastExtractedLexem.Token) > prio)
            {
                var operationLexem = _lastExtractedLexem;
                NextLexem();
                var secondArg = BuildExpression(LanguageDef.GetBinaryPriority(operationLexem.Token));

                firstArg = new BinaryOperationNode(firstArg, secondArg, operationLexem);
            }

            return firstArg;
        }

        private BslSyntaxNode BuildPrimaryExpression()
        {
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                return BuildParenthesis();
            }

            var operation = _lastExtractedLexem;
            var prio = LanguageDef.GetUnaryPriority(operation.Token);

            if (prio == LanguageDef.MAX_OPERATION_PRIORITY)
            {
                return TerminalNode();
            }

            NextLexem();

            if (operation.Token == Token.Plus)
                operation.Token = Token.UnaryPlus;
            else if (operation.Token == Token.Minus)
            {
                operation.Token = Token.UnaryMinus;
                if (_lastExtractedLexem.Type == LexemType.NumberLiteral) //TODO:move it to lexer
                {
                    _lastExtractedLexem.Content = '-' + _lastExtractedLexem.Content;
                    return TerminalNode();
                }
            }

            if (LanguageDef.GetUnaryPriority(_lastExtractedLexem.Token) <= prio)
            {
                return CreateError(LocalizedErrors.ExpressionSyntax());
            }

            var arg = BuildExpression(prio);
            return new UnaryOperationNode(arg, operation);
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
                {
                    return CreateError(LocalizedErrors.UnexpectedEof());
                }
                else
                {
                    return CreateError(LocalizedErrors.TokenExpected(stopToken), false);
                }
            }

            return node;
        }
        
        private void BuildOptionalExpression(NonTerminalNode parent, Token stopToken)
        {
            if (_lastExtractedLexem.Token == stopToken)
            {
                return;
            }

            var op = BuildExpression(0);
            parent.AddChild(op);
        }

        #region Operators

        private BslSyntaxNode BuildParenthesis()
        {
            NextLexem();
            var expr = BuildExpression(0);
            if (_lastExtractedLexem.Token != Token.ClosePar)
            {
                return CreateError(LocalizedErrors.TokenExpected(Token.ClosePar));
            }
            NextLexem();

            return BuildDereference(expr);
        }

        #endregion

        private BslSyntaxNode TerminalNode()
        {
            BslSyntaxNode node = SelectTerminalNode(_lastExtractedLexem, true);
            if (node == default)
            {
                return CreateError(LocalizedErrors.ExpressionSyntax());
            }
        
            return node;
        }
        
        private BslSyntaxNode SelectTerminalNode(in Lexem currentLexem, bool supportAwait)
        {
            BslSyntaxNode node = default;
            if (LanguageDef.IsLiteral(currentLexem))
            {
                node = new TerminalNode(NodeKind.Constant, currentLexem);
                NextLexem();
            }
            else if (LanguageDef.IsUserSymbol(currentLexem))
            {
                node = BuildGlobalCall(currentLexem);
            }
            else if (currentLexem.Token == Token.NewObject)
            {
                node = BuildNewObjectCreation();
            }
            else if (LanguageDef.IsBuiltInFunction(currentLexem.Token))
            {
                node = BuildGlobalCall(currentLexem);
            }
            else if (currentLexem.Token == Token.Question)
            {
                node = BuildQuestionOperator();
            }
            else if (supportAwait && currentLexem.Token == Token.Await)
            {
                node = BuildExpressionAwaitOperator(currentLexem);
            }
                
            return node;
        }

        private BslSyntaxNode BuildQuestionOperator()
        {
            var node = new NonTerminalNode(NodeKind.TernaryOperator, _lastExtractedLexem);
            if (!NextExpected(Token.OpenPar))
                return CreateError(LocalizedErrors.TokenExpected(Token.OpenPar));

            NextLexem();

            if (!TryParseNode(() =>
            {
                BuildExpressionUpTo(node, Token.Comma);
                BuildExpressionUpTo(node, Token.Comma);
                BuildExpressionUpTo(node, Token.ClosePar);
            }))
            {
                return CreateError(LocalizedErrors.ExpressionSyntax());
            }

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
                    return CreateError(LocalizedErrors.IdentifierExpected());
                }

                var identifier = _lastExtractedLexem;
                NextLexem();
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    var ident = new TerminalNode(NodeKind.Identifier, identifier);
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
                if (expression.Kind == NodeKind.Unknown)
                {
                    return CreateError(LocalizedErrors.ExpressionSyntax());
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
                return CreateError(LocalizedErrors.IdentifierExpected());
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
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    // есть аргументы после имени
                    NextLexem();
                }
                BuildCallArguments(callArgs);
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            err.Position.ColumnNumber -= _lastExtractedLexem.Content?.Length ?? 1;
            ErrorSink.AddError(err);

            if (doFastForward)
            {
                if (_tokenStack.Count != 0)
                    SkipToNextStatement(_tokenStack.Peek());
                else
                    SkipToNextStatement();
            }

            if(_enableException)
                throw new InternalParseException(err);
        }

        private ErrorTerminalNode CreateError(CodeError error, bool doFastForward = true)
        {
            var lexem = _lastExtractedLexem;
            AddError(error, doFastForward);
            return new ErrorTerminalNode(lexem);
        }

        private bool IsUserSymbol(in Lexem lex)
        {
            return LanguageDef.IsUserSymbol(in lex) || (!_isInAsyncMethod && lex.Token == Token.Await);
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

        private static void CreateChild(NonTerminalNode parent, NodeKind kind, in Lexem lex)
        {
            var child = NodeBuilder.CreateNode(kind, lex);
            parent.AddChild(child);
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