using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class Parser : ILexemExtractor
    {
        private Lexer _lexer;
        private Lexem _lastExtractedLexem;
        private IModuleBuilder _builder;
        private bool _wasErrorsInBuild;
        private Stack<Token[]> _blockEndings;

        public Parser(IModuleBuilder builder)
        {
            _builder = builder;
        }

        public bool Build(Lexer lexer)
        {
            _lexer = lexer;
            _lastExtractedLexem = default(Lexem);
            _lexer.UnexpectedCharacterFound += _lexer_UnexpectedCharacterFound;
            _wasErrorsInBuild = false;
            _blockEndings = new Stack<Token[]>();

            return BuildModule();
        }

        void _lexer_UnexpectedCharacterFound(object sender, LexerErrorEventArgs e)
        {
            // синтаксические ошибки пока не обрабатываются.
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
                ReportError(e);
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

            return !_wasErrorsInBuild;
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
                        ReportError(CompilerException.LateVarDefinition());
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
                }
                else
                {
                    ReportError(CompilerException.UnexpectedOperation());
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
                    else if (_lastExtractedLexem.Token != Token.Semicolon)
                        throw CompilerException.SemicolonExpected();

                    NextLexem();
                }
                catch (ScriptException e)
                {
                    ReportError(e);
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

            IASTNode methodNode = _builder.BeginMethod(identifier, isFunction);
            try
            {
                List<ASTMethodParameter> parameters = new List<ASTMethodParameter>();
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
                    bool isOptional = _lastExtractedLexem.Token == Token.Equal;
                    Lexem optionalLiteral = Lexem.Empty();

                    if (isOptional)
                    {
                        NextLexem();

                        if (!LanguageDef.IsLiteral(ref _lastExtractedLexem))
                            throw CompilerException.LiteralExpected();

                        optionalLiteral = _lastExtractedLexem;

                    }

                    var paramData = new ASTMethodParameter();
                    paramData.Name = paramName;
                    paramData.ByValue = byValParam;
                    paramData.IsOptional = isOptional;
                    if (isOptional)
                        paramData.DefaultValueLiteral = _builder.ReadLiteral(optionalLiteral);

                    parameters.Add(paramData);

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
                ReportError(exc);
                SkipToNextStatement();
            }

            if (isFunction)
                PushEndTokens(Token.EndFunction);
            else
                PushEndTokens(Token.EndProcedure);
            
            BuildCodeBatch();
            _builder.EndMethod(methodNode);

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

        private void BuildCodeBatch()
        {
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
                    ReportError(e);
                    SkipToNextStatement();
                }
            }
        }

        private bool BuildStatement()
        {
            Debug.Assert(_lastExtractedLexem.Type == LexemType.Identifier);

            if(LanguageDef.IsBeginOfStatement(_lastExtractedLexem.Token))
            {
                throw new NotImplementedException();
            }
            else if(LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
            {
                return BuildSimpleStatement();
            }
            else
            {
                ReportError(CompilerException.UnexpectedOperation());
                return false;
            }
        }

        private bool BuildSimpleStatement()
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
                    ReportError(CompilerException.UnexpectedOperation());
                    return false;
            }

            return true;
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

            var endTokens = PopEndTokens();

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

        private void ReportError(ScriptException compilerException)
        {
            _wasErrorsInBuild = true;
            ScriptException.AppendCodeInfo(compilerException, _lexer.GetIterator().GetPositionInfo());

            if (CompilerError != null)
            {
                var eventArgs = new CompilerErrorEventArgs();
                eventArgs.Exception = compilerException;
                eventArgs.LexerState = _lexer;
                CompilerError(this, eventArgs);

                if (!eventArgs.IsHandled)
                    throw compilerException;

                _builder.OnError(eventArgs);

            }
            else
            {
                throw compilerException;
            }
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

        Lexem ILexemExtractor.LastExtractedLexem
        {
            get { return _lastExtractedLexem; }
        }

        void ILexemExtractor.NextLexem()
        {
            this.NextLexem();
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
