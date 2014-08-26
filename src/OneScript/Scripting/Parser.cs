using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OneScript.Scripting
{
    public class Parser
    {
        private Lexer _lexer;
        private Lexem _lastExtractedLexem;
        private IModuleBuilder _builder;
        private CompilerContext _ctx;
        private bool _wasErrorsInBuild;

        public Parser(IModuleBuilder builder)
        {
            _builder = builder;
        }

        public bool Build(CompilerContext context, Lexer lexer)
        {
            _lexer = lexer;
            _ctx = context;
            _lastExtractedLexem = default(Lexem);

            return BuildModule();
        }

        public event EventHandler<CompilerErrorEventArgs> CompilerError;

        private bool BuildModule()
        {
            try
            {
                _builder.BeginModule(_ctx);

                DispatchModuleBuild();
                ProcessForwardedDeclarations();

                _builder.CompleteModule();
            }
            catch (ScriptException e)
            {
                ReportError(e);
            }
            catch(Exception e)
            {
                var newExc = new CompilerException(new CodePositionInfo(), "Внутренняя ошибка компилятора", e);
                ReportError(newExc);
            }

            return !_wasErrorsInBuild;
        }

        private void DispatchModuleBuild()
        {
            NextLexem();

            do
            {
                bool success = false;

                if (_lastExtractedLexem.Token == Token.VarDef)
                {
                    success = BuildVariableDefinition();
                }

                if (success && CheckCorrectStatementEnd())
                {
                    // это точка с запятой или конец блока
                    NextLexem();
                }
                else
                {
                    SkipToNextStatement();
                }

            }
            while (_lastExtractedLexem.Token != Token.EndOfText);
        }

        private bool BuildVariableDefinition()
        {
            Debug.Assert(_lastExtractedLexem.Token == Token.VarDef);
            
            NextLexem();
            while (true)
            {
                if (IsUserSymbol(ref _lastExtractedLexem))
                {
                    var symbolicName = _lastExtractedLexem.Content;
                    NextLexem();

                    if (_lastExtractedLexem.Token == Token.Export)
                    {
                        _builder.DefineExportVariable(symbolicName);
                        NextLexem();
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

        private bool CheckCorrectStatementEnd()
        {
            if(!(_lastExtractedLexem.Token == Token.Semicolon ||
                 _lastExtractedLexem.Token == Token.EndOfText))
            {
                ReportError(CompilerException.SemicolonExpected());
                return false;
            }

            return true;
        }

        public void SkipToNextStatement()
        {
            while(!(_lastExtractedLexem.Token == Token.EndOfText
                    || LanguageDef.IsBeginOfStatement(_lastExtractedLexem.Token)))
            {
                NextLexem();
            }
        }

        private void ProcessForwardedDeclarations()
        {
            //throw new NotImplementedException();
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

        private bool IsUserSymbol(ref Lexem lex)
        {
            return lex.Type == LexemType.Identifier && lex.Token == Token.NotAToken;
        }

        private bool IsValidIdentifier(ref Lexem lex)
        {
            return lex.Type == LexemType.Identifier;
        }

        private bool IsLiteral(ref Lexem lex)
        {
            return lex.Type == LexemType.StringLiteral
                || lex.Type == LexemType.NumberLiteral
                || lex.Type == LexemType.BooleanLiteral
                || lex.Type == LexemType.DateLiteral
                || lex.Type == LexemType.UndefinedLiteral;
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

        #endregion
    }
}
