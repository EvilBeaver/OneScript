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
        private PositionInModule _parserPosition;

        private enum PositionInModule
        {
            Begin,
            VarSection,
            MethodSection,
            MethodHeader,
            MethodVarSection,
            MethodBody,
            ModuleBody
        }

        public Parser(IModuleBuilder builder)
        {
            _builder = builder;
        }

        public bool Build(Lexer lexer)
        {
            _lexer = lexer;
            _lastExtractedLexem = default(Lexem);
            _lexer.UnexpectedCharacterFound += _lexer_UnexpectedCharacterFound;
            _parserPosition = PositionInModule.Begin;
            _wasErrorsInBuild = false;

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
            catch (ScriptException e)
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
            NextLexem();

            do
            {
                bool success = false;
                try
                {
                    success = SelectAndBuildOperation();
                }
                catch(CompilerException e)
                {
                    ReportError(e);
                    success = false;
                }

                if (success && CheckCorrectStatementEnd())
                {
                    // это точка с запятой или конец блока
                    if (_lastExtractedLexem.Token != Token.EndOfText)
                        NextLexem();
                }
                else
                {
                    SkipToNextStatement();
                }

            }
            while (_lastExtractedLexem.Token != Token.EndOfText);
        }

        private bool SelectAndBuildOperation()
        {
            bool success = false;

            if (_lastExtractedLexem.Token == Token.VarDef)
            {
                if (_parserPosition == PositionInModule.Begin)
                    SetPosition(PositionInModule.VarSection);
                else
                    SetPosition(PositionInModule.MethodVarSection);
                
                success = BuildVariableDefinition();
            }
            else if (_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function)
            {

            }
            else if (_lastExtractedLexem.Type == LexemType.Identifier)
            {
                if (_parserPosition == PositionInModule.Begin
                    || _parserPosition == PositionInModule.VarSection
                    || _parserPosition == PositionInModule.MethodSection)
                {
                    SetPosition(PositionInModule.ModuleBody);
                }
                success = BuildStatement();
            }
            else
            {
                success = false;
                ReportError(CompilerException.UnexpectedOperation());
            }

            return success;
        }

        private bool BuildVariableDefinition()
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
                        if (_parserPosition == PositionInModule.VarSection)
                        {
                            _builder.DefineExportVariable(symbolicName);
                            NextLexem();
                        }
                        else
                        {
                            ReportError(CompilerException.ExportOnLocalVariable());
                            return false;
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
                    ReportError(CompilerException.IdentifierExpected());
                    return false;
                }
            }

            return true;
    
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
                    // access chain
                    throw new NotImplementedException();
                    break;
                case Token.OpenPar:
                    // call
                    throw new NotImplementedException();
                    break;
                case Token.OpenBracket:
                    // access by index
                    throw new NotImplementedException();
                    break;
                default:
                    ReportError(CompilerException.UnexpectedOperation());
                    return false;
            }

            return true;
        }

        private IASTNode BuildExpression(Token stopToken)
        {
            IASTNode subNode = BuildOperation(0, BuildPrimaryNode());
            if(_lastExtractedLexem.Token == stopToken)
                return subNode;

            // выход из подвыражения осуществляется как только встретился не-бинарный оператор


            // здесь нужно добавить проверку на допустимый конецблока. Пока что жестко конец текста
            if (_lastExtractedLexem.Token == Token.EndOfText)
                return subNode;

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
                primary = _builder.ReadVariable(_lastExtractedLexem.Content);
                NextLexem();
                if(_lastExtractedLexem.Token == Token.Dot)
                {

                }
                else if(_lastExtractedLexem.Token == Token.OpenPar)
                {

                }
                else if(_lastExtractedLexem.Token == Token.OpenBracket)
                {

                }
                else if(!LanguageDef.IsBinaryOperator(_lastExtractedLexem.Token))
                {
                    throw CompilerException.ExpressionSyntax();
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
                var subNode = BuildPrimaryNode();
                primary = _builder.UnaryOperation(Token.Minus, subNode);
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

        

        public static ConstDefinition CreateConstDefinition(ref Lexem lex)
        {
            ConstType constType = ConstType.Undefined;
            switch (lex.Type)
            {
                case LexemType.BooleanLiteral:
                    constType = ConstType.Boolean;
                    break;
                case LexemType.DateLiteral:
                    constType = ConstType.Date;
                    break;
                case LexemType.NumberLiteral:
                    constType = ConstType.Number;
                    break;
                case LexemType.StringLiteral:
                    constType = ConstType.String;
                    break;
            }

            ConstDefinition cDef = new ConstDefinition()
            {
                Type = constType,
                Presentation = lex.Content
            };
            return cDef;
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

        private bool CheckCorrectStatementEnd()
        {
            if (!(_lastExtractedLexem.Token == Token.Semicolon ||
                 _lastExtractedLexem.Token == Token.EndOfText))
            {
                ReportError(CompilerException.SemicolonExpected());
                return false;
            }

            return true;
        }

        private void SkipToNextStatement()
        {
            while (!(_lastExtractedLexem.Token == Token.EndOfText
                    || LanguageDef.IsBeginOfStatement(_lastExtractedLexem.Token)))
            {
                NextLexem();
            }
        }

        private void SetPosition(PositionInModule newPosition)
        {
            switch(newPosition)
            {
                case PositionInModule.VarSection:
                {
                    if(!(_parserPosition == PositionInModule.Begin ||
                        _parserPosition == PositionInModule.VarSection ||
                        _parserPosition == PositionInModule.MethodVarSection))

                        throw CompilerException.LateVarDefinition();

                    break;
                }
                case PositionInModule.MethodVarSection:
                {
                    if(_parserPosition != PositionInModule.MethodHeader)
                        throw CompilerException.LateVarDefinition();

                    break;
                }
            }
            
            _parserPosition = newPosition;
        }

        Lexem ILexemExtractor.LastExtractedLexem
        {
            get { return _lastExtractedLexem; }
        }

        void ILexemExtractor.NextLexem()
        {
            this.NextLexem();
        }

        #endregion
   
    }
}
