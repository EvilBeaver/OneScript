using OneScript.Compiler.Lexics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OneScript.Compiler
{
    public class Parser
    {
        private ILexemGenerator _lexer;
        private Lexem _lastExtractedLexem;
        private IModuleBuilder _builder;
        private bool _wereErrorsInBuild;
        private Stack<Token[]> _blockEndings;

        private bool _isInMethodScope = false;

        public Parser(IModuleBuilder builder)
        {
            _builder = builder;
        }

        public bool Build(ILexemGenerator lexer)
        {
            _lexer = lexer;
            _lastExtractedLexem = default(Lexem);
            // TODO: обработка нераспознанных символов
            //_lexer.UnexpectedCharacterFound += _lexer_UnexpectedCharacterFound;
            _wereErrorsInBuild = false;
            _blockEndings = new Stack<Token[]>();

            return BuildModule();
        }

        void _lexer_UnexpectedCharacterFound(object sender, LexerErrorEventArgs e)
        {
            // TODO: синтаксические ошибки пока не обрабатываются.
        }

        public event EventHandler<CompilerErrorEventArgs> CompilerError;

        private bool BuildModule()
        {
            try
            {
                
                _builder.BeginModule();

                DispatchModuleBuild();

            }
            catch(ScriptException e)
            {
                if (!ReportError(e))
                    throw;
            }
            catch(Exception e)
            {
                var newExc = new CompilerException(new CodePositionInfo(), "Внутренняя ошибка компилятора", e);
                throw newExc;
            }
            finally
            {
                _builder.CompleteModule();
            }

            return !_wereErrorsInBuild;
        }

        private void DispatchModuleBuild()
        {
            NextLexem(); // переход к первой лексеме

            bool methodsDefined = false;
            bool bodyDefined = false;

            while (_lastExtractedLexem.Token != Token.EndOfText)
            {
                if (_lastExtractedLexem.Token == Token.VarDef)
                {
                    if (!methodsDefined && !bodyDefined)
                        DefineModuleVariables();
                    else
                    {
                        ReportErrorOrThrow(CompilerException.LateVarDefinition());
                        SkipToNextStatement();
                    }
                }
                else if (_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function)
                {
                    if (!bodyDefined)
                    {
                        BuildMethod();
                        methodsDefined = true;
                    }
                    else
                    {
                        throw CompilerException.LateMethodDefinition();
                    }
                }
                else if (LanguageDef.IsIdentifier(ref _lastExtractedLexem) || _lastExtractedLexem.Token == Token.Semicolon)
                {
                    bodyDefined = true;
                    PushEndTokens(Token.EndOfText);
                    BuildCodeBatch();
                    PopEndTokens();
                }
                else
                {
                    ReportErrorOrThrow(CompilerException.UnexpectedOperation());
                    SkipToNextStatement();
                }
            }

        }

        private void DefineModuleVariables()
        {
            while (true)
            {
                try
                {
                    if (_lastExtractedLexem.Token != Token.VarDef)
                        break;

                    BuildVariableDefinition(true);

                    if (_lastExtractedLexem.Token == Token.EndOfText)
                        break;
                    
                    if (_lastExtractedLexem.Token != Token.Semicolon)
                        throw CompilerException.SemicolonExpected();

                    NextLexem();
                }
                catch (ScriptException e)
                {
                    if(!ReportError(e))
                        throw;

                    SkipToNextStatement();
                }
            }
        }

        private void BuildMethod()
        {
            Debug.Assert(_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function);
            
            bool isFunction = _lastExtractedLexem.Token == Token.Function;

            NextLexem();

            if (!LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
                throw CompilerException.IdentifierExpected();

            string identifier = _lastExtractedLexem.Content;

            NextLexem();

            if (_lastExtractedLexem.Token != Token.OpenPar)
                throw CompilerException.TokenExpected("(");

            var methodNode = _builder.BeginMethod(identifier, isFunction);
            try
            {
                var parameters = new List<ASTMethodParameter>();
                do
                {

                    NextLexem();

                    bool byValParam = _lastExtractedLexem.Token == Token.ByValParam;
                    if (byValParam)
                        NextLexem();

                    if (!LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
                        throw CompilerException.IdentifierExpected();

                    string paramName = _lastExtractedLexem.Content;
                    if (parameters.FindIndex((x) => x.Name.Equals(paramName, StringComparison.OrdinalIgnoreCase)) >= 0)
                        throw new CompilerException("Параметр с именем " + paramName + " уже определен");

                    NextLexem();
                    bool isOptional;
                    if (_lastExtractedLexem.Token == Token.Equal)
                        isOptional = true;
                    else if (_lastExtractedLexem.Token == Token.Comma || _lastExtractedLexem.Token == Token.ClosePar)
                        isOptional = false;
                    else
                        throw CompilerException.UnexpectedOperation();

                    Lexem optionalLiteral = Lexem.Empty();

                    if (isOptional)
                    {
                        NextLexem();

                        if (!LanguageDef.IsLiteral(ref _lastExtractedLexem))
                            throw CompilerException.LiteralExpected();

                        optionalLiteral = _lastExtractedLexem;
                        NextLexem();

                    }

                    var paramData = new ASTMethodParameter();
                    paramData.Name = paramName;
                    paramData.ByValue = byValParam;
                    paramData.IsOptional = isOptional;
                    if (isOptional)
                        paramData.DefaultValueLiteral = ConstDefinition.CreateFromLiteral(ref optionalLiteral);

                    parameters.Add(paramData);

                    if (_lastExtractedLexem.Token != Token.ClosePar && _lastExtractedLexem.Token != Token.Comma)
                        throw CompilerException.TokenExpected(")");

                } while (_lastExtractedLexem.Token != Token.ClosePar);

                NextLexem(); // убрали закрывающую скобку

                bool isExported = false;
                if (_lastExtractedLexem.Token == Token.Export)
                {
                    isExported = true;
                    NextLexem();
                }

                _builder.SetMethodSignature(methodNode, parameters.ToArray(), isExported);

            }
            catch (CompilerException exc)
            {
                if(!ReportError(exc))
                    throw;

                SkipToNextStatement();
            }

            PushEndTokens(isFunction ? Token.EndFunction : Token.EndProcedure);

            _isInMethodScope = true;
            BuildCodeBatch();            
            _isInMethodScope = false;
            
            PopEndTokens();

            _builder.EndMethod(methodNode);

            NextLexem(); // убрали конецпроцедуры/функции

        }

        private void BuildVariableDefinition(bool allowExports)
        {
            Debug.Assert(_lastExtractedLexem.Token == Token.VarDef);
            
            NextLexem();
            while (true)
            {
                if (LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
                {
                    var symbolicName = _lastExtractedLexem.Content;
                    NextLexem();

                    if (_lastExtractedLexem.Token == Token.Export)
                    {
                        if (allowExports)
                        {
                            _builder.DefineExportVariable(symbolicName);
                            NextLexem();
                        }
                        else
                        {
                            throw CompilerException.ExportOnLocalVariable();
                        }
                    }
                    else
                    {
                        _builder.DefineVariable(symbolicName);
                    }

                    if (_lastExtractedLexem.Token == Token.Comma)
                    {
                        NextLexem();
                        continue;
                    }
                    else
                    {
                        // переменная объявлена.
                        // далее, диспетчер определит - нужна ли точка с запятой
                        // и переведет обработку дальше
                        break;
                    }
                }
                else
                {
                    throw CompilerException.IdentifierExpected();
                }
            }
        }

        private IASTNode BuildCodeBatch()
        {
            var batch = _builder.BeginBatch();

            var endTokens = _blockEndings.Peek();
            while(!endTokens.Contains(_lastExtractedLexem.Token))
            {
                if (_lastExtractedLexem.Token == Token.EndOfText)
                    throw CompilerException.UnexpectedEndOfText();

                if (_lastExtractedLexem.Token == Token.Semicolon)
                {
                    NextLexem();
                    continue;
                }

                try
                {
                    if (_lastExtractedLexem.Token == Token.VarDef)
                        throw CompilerException.LateVarDefinition();
                    else if (_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function)
                        throw CompilerException.LateMethodDefinition();
                
                    BuildStatement();

                    if (_lastExtractedLexem.Token != Token.Semicolon)
                    {
                        if (!endTokens.Contains(_lastExtractedLexem.Token))
                            throw CompilerException.SemicolonExpected();
                    }
                    else
                        NextLexem();

                }
                catch (CompilerException e)
                {
                    if(!ReportError(e))
                        throw;

                    SkipToNextStatement(endTokens);
                }
            }

            _builder.EndBatch(batch);
            return batch;
        }

        private void BuildStatement()
        {
            Debug.Assert(_lastExtractedLexem.Type == LexemType.Identifier);

            if(LanguageDef.IsBeginOfStatement(_lastExtractedLexem.Token))
            {
                BuildComplexStatement();
            }
            else if(LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
            {
                BuildSimpleStatement();
            }
            else
            {
                throw CompilerException.UnexpectedOperation();
            }
        }

        private void BuildComplexStatement()
        {
            switch(_lastExtractedLexem.Token)
            {
                case Token.VarDef:
                    throw new NotImplementedException();
                case Token.If:
                    BuildIfStatement();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void BuildSimpleStatement()
        {
            Debug.Assert(LanguageDef.IsUserSymbol(ref _lastExtractedLexem));

            string identifier = _lastExtractedLexem.Content;

            NextLexem();

            switch(_lastExtractedLexem.Token)
            {
                case Token.Equal:
                    // simple assignment
                    NextLexem();

                    var acceptor = _builder.SelectOrUseVariable(identifier);
                    var source = BuildExpression(Token.Semicolon);
                    
                    _builder.BuildAssignment(acceptor, source);

                    break;
                case Token.Dot:
                case Token.OpenBracket:
                    // access chain
                    BuildAccessChainLeftHand(identifier);
                    break;
                case Token.OpenPar:
                    // call
                    var args = BuildArgumentList();
                    _builder.BuildProcedureCall(null, identifier, args);
                    break;
                
                default:
                    throw CompilerException.UnexpectedOperation();
            }

        }

        private void BuildAccessChainLeftHand(string identifier)
        {
            var target = _builder.ReadVariable(identifier);

            string ident;
            var resolved = BuildContinuationLeftHand(target, out ident);

            if (ident == null)
            {
                // это присваивание
                //NextLexem();
                if (_lastExtractedLexem.Token != Token.Equal)
                    throw CompilerException.UnexpectedOperation();

                NextLexem(); // перешли к выражению
                var source = BuildExpression(Token.Semicolon);
                _builder.BuildAssignment(resolved, source);

            }
            else
            {
                // это вызов
                Debug.Assert(_lastExtractedLexem.Token == Token.OpenPar);
                var args = BuildArgumentList();
                _builder.BuildProcedureCall(resolved, ident, args);
            }
        }

        private IASTNode BuildExpression(Token stopToken)
        {
            IASTNode subNode = BuildOperation(0, BuildPrimaryNode());
            if(_lastExtractedLexem.Token == stopToken)
                return subNode;

            var endTokens = _blockEndings.Peek();

            // здесь нужно добавить проверку на допустимый конецблока.
            if (endTokens.Contains(_lastExtractedLexem.Token))
                return subNode;
            else if(_lastExtractedLexem.Token == Token.EndOfText)
                throw CompilerException.UnexpectedEndOfText();
            else
                throw CompilerException.ExpressionSyntax();
        }

        private IASTNode BuildOperation(int acceptablePriority, IASTNode leftHandedNode)
        {
            var currentOp = _lastExtractedLexem.Token;
            var opPriority = GetBinaryPriority(currentOp);
            while(LanguageDef.IsBinaryOperator(currentOp) && opPriority >= acceptablePriority)
            {
                NextLexem();
                var rightHandedNode = BuildPrimaryNode();

                var newOp = _lastExtractedLexem.Token;
                int newPriority = GetBinaryPriority(newOp);

                if(newPriority > opPriority)
                {
                    rightHandedNode = BuildOperation(newPriority, rightHandedNode);
                }

                leftHandedNode = _builder.BinaryOperation(currentOp, leftHandedNode, rightHandedNode);

                currentOp = _lastExtractedLexem.Token;
                opPriority = GetBinaryPriority(currentOp);
                
            }

            return leftHandedNode;
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

        private IASTNode BuildPrimaryNode()
        {
            IASTNode primary;
            if(LanguageDef.IsLiteral(ref _lastExtractedLexem))
            {
                primary = _builder.ReadLiteral(_lastExtractedLexem);
                NextLexem();
            }
            else if(LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
            {
                var identifier = _lastExtractedLexem.Content;
                NextLexem();
                if(_lastExtractedLexem.Token == Token.Dot)
                {
                    // это цепочка разыменований
                    var target = _builder.ReadVariable(identifier);
                    primary = BuildContinuationRightHand(target);
                }
                else if(_lastExtractedLexem.Token == Token.OpenPar)
                {
                    // это вызов функции
                    IASTNode[] args = BuildArgumentList();
                    primary = _builder.BuildFunctionCall(null, identifier, args);

                }
                else if(_lastExtractedLexem.Token == Token.OpenBracket)
                {
                    var target = _builder.ReadVariable(identifier);
                    primary = BuildContinuationRightHand(target);
                }
                else
                {
                    primary = _builder.ReadVariable(identifier);
                }
            }
            else if(_lastExtractedLexem.Token == Token.Minus)
            {
                NextLexem();
                if(!(LanguageDef.IsLiteral(ref _lastExtractedLexem)
                    ||LanguageDef.IsIdentifier(ref _lastExtractedLexem)
                    ||_lastExtractedLexem.Token == Token.OpenPar))
                {
                    throw CompilerException.ExpressionExpected();
                }

                var subNode = BuildPrimaryNode();
                primary = _builder.UnaryOperation(Token.Minus, subNode);
            }
            else if(_lastExtractedLexem.Token == Token.Not)
            {
                NextLexem();
                var subNode = BuildPrimaryNode();
                primary = _builder.UnaryOperation(Token.Not, subNode);
            }
            else if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                NextLexem(); // съели открывающую скобку
                var firstSubNode = BuildPrimaryNode();
                primary = BuildOperation(0, firstSubNode);
                
                if (_lastExtractedLexem.Token != Token.ClosePar)
                    throw CompilerException.TokenExpected(")");
                NextLexem(); // съели закрывающую скобку
            }
            else
            {
                throw CompilerException.ExpressionSyntax();
            }

            return primary;
        }

        private IASTNode BuildContinuationRightHand(IASTNode target)
        {
            string dummy;
            return BuildContinuationInternal(target, false, out dummy);
        }

        private IASTNode BuildContinuationLeftHand(IASTNode target, out string lastIdentifier)
        {
            return BuildContinuationInternal(target, true, out lastIdentifier);
        }

        private IASTNode BuildContinuationInternal(IASTNode target, bool interruptOnCall, out string lastIdentifier)
        {
            lastIdentifier = null;
            while (true)
            {
                if (_lastExtractedLexem.Token == Token.Dot)
                {
                    NextLexem();
                    if (!LanguageDef.IsIdentifier(ref _lastExtractedLexem))
                        throw CompilerException.IdentifierExpected();

                    string identifier = _lastExtractedLexem.Content;
                    NextLexem();
                    if (_lastExtractedLexem.Token == Token.Dot||_lastExtractedLexem.Token == Token.OpenBracket)
                    {
                        target = _builder.ResolveProperty(target, identifier);
                    }
                    else if (_lastExtractedLexem.Token == Token.OpenPar)
                    {
                        if (interruptOnCall)
                        {
                            lastIdentifier = identifier;
                            return target;
                        }
                        else
                        {
                            var args = BuildArgumentList();
                            target = _builder.BuildFunctionCall(target, identifier, args);
                        }
                    }
                    else
                    {
                        return _builder.ResolveProperty(target, identifier);
                    }
                }
                else if(_lastExtractedLexem.Token == Token.OpenBracket)
                {
                    NextLexem();
                    if (_lastExtractedLexem.Token == Token.CloseBracket)
                        throw CompilerException.ExpressionExpected();

                    var expr = BuildExpression(Token.CloseBracket);
                    Debug.Assert(_lastExtractedLexem.Token == Token.CloseBracket);
                    NextLexem();

                    target = _builder.BuildIndexedAccess(target, expr);
                }
                else
                {
                    return target;
                }
            }

        }

        private IASTNode[] BuildArgumentList()
        {
            Debug.Assert(_lastExtractedLexem.Token == Token.OpenPar);
            NextLexem();
            
            List<IASTNode> arguments = new List<IASTNode>();
            PushEndTokens(Token.ClosePar);
            while(_lastExtractedLexem.Token != Token.ClosePar)
            {
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    arguments.Add(null);
                    NextLexem();
                    continue;
                }

                var argNode = BuildExpression(Token.Comma);
                arguments.Add(argNode);
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextLexem();
                    if(_lastExtractedLexem.Token == Token.ClosePar)
                    {
                        // список аргументов кончился
                        arguments.Add(null);
                    }
                }
            }

            if (_lastExtractedLexem.Token != Token.ClosePar)
                throw CompilerException.TokenExpected(")");

            NextLexem(); // съели закрывающую скобку

            return arguments.ToArray();
        }

        private void BuildIfStatement()
        {
            System.Diagnostics.Debug.Assert(_lastExtractedLexem.Token == Token.If);

            NextLexem();

            var ifBlock = _builder.BeginConditionStatement();
            ifBlock.Condition = BuildExpression(Token.Then);

            NextLexem();
            PushEndTokens(Token.Else, Token.ElseIf, Token.EndIf);
            ifBlock.TruePart = BuildCodeBatch();
            
            var currentIf = ifBlock;
            while (_lastExtractedLexem.Token == Token.ElseIf)
            {
                var elseif = _builder.BeginConditionStatement();
                
                NextLexem();
                elseif.Condition = BuildExpression(Token.Then);
                NextLexem();
                elseif.TruePart = BuildCodeBatch();
                currentIf.FalsePart = elseif;
                currentIf = elseif;
            }

            if(_lastExtractedLexem.Token == Token.Else)
            {
                PopEndTokens();
                NextLexem();
                PushEndTokens(Token.EndIf);
                currentIf.FalsePart = BuildCodeBatch();

            }

            PopEndTokens();

            NextLexem(); // endif

            _builder.EndConditionStatement(ifBlock);

        }

        #region Helper methods

        public void NextLexem()
        {
            if (_lastExtractedLexem.Token != Token.EndOfText)
            {
                _lastExtractedLexem = _lexer.NextLexem();
            }
            else
            {
                throw CompilerException.UnexpectedEndOfText();
            }
        }

        private bool ReportError(ScriptException compilerException)
        {
            _wereErrorsInBuild = true;
            ScriptException.AppendCodeInfo(compilerException, _lexer.GetCodePosition());

            if (CompilerError == null) 
                return false;
            
            var eventArgs = new CompilerErrorEventArgs
            {
                Exception = compilerException, 
                LexemGenerator = _lexer
            };

            CompilerError(this, eventArgs);

            return eventArgs.IsHandled;
        }

        private void ReportErrorOrThrow(ScriptException compilerException)
        {
            if (!ReportError(compilerException))
                throw compilerException;
        }

        private void SkipToNextStatement(Token[] additionalStops = null)
        {
            while (!(_lastExtractedLexem.Token == Token.EndOfText
                    || LanguageDef.IsBeginOfStatement(_lastExtractedLexem.Token)))
            {
                NextLexem();
                if(additionalStops != null && additionalStops.Contains(_lastExtractedLexem.Token))
                {
                    break;
                }
            }
        }

        void PushEndTokens(params Token[] tokens)
        {
            _blockEndings.Push(tokens);
        }

        Token[] PopEndTokens()
        {
            return _blockEndings.Pop();
        }

        #endregion
   
    }
}
