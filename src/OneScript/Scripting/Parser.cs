using System;
using System.Collections.Generic;
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

        public Parser(IModuleBuilder builder)
        {
            _builder = builder;
        }

        public void Build(CompilerContext context, Lexer lexer)
        {
            _lexer = lexer;
            _ctx = context;
            _lastExtractedLexem = default(Lexem);

            BuildModule();
        }

        public event EventHandler<CompilerErrorEventArgs> CompilerError;

        private void BuildModule()
        {
            _builder.BeginModule(_ctx);
            
            DispatchModuleBuild();
            ProcessForwardedDeclarations();

            _builder.CompleteModule();
        }

        private void DispatchModuleBuild()
        {
            do
            {
                NextLexem();
                if (_lastExtractedLexem.Token == Token.VarDef)
                {
                    BuildVariableDefinitions();
                }
            }
            while (_lastExtractedLexem.Token != Token.EndOfText);
        }

        private void BuildVariableDefinitions()
        {
            while (_lastExtractedLexem.Token == Token.VarDef)
            {
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
                        else if (_lastExtractedLexem.Token == Token.Semicolon)
                        {
                            NextLexem();
                            break;
                        }
                        else if (_lastExtractedLexem.Token != Token.Semicolon)
                        {
                            ReportError(CompilerException.SemicolonExpected());
                            break; // goto next statement (if any)
                        }
                    }
                    else
                    {
                        ReportError(CompilerException.IdentifierExpected());
                        SkipTill(Token.Semicolon);
                        break;
                    }
                }
    
            }

        }

        private void SkipTill(Token token)
        {
            while(_lastExtractedLexem.Token != token && _lastExtractedLexem.Token != Token.EndOfText)
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

        private void ReportError(CompilerException compilerException)
        {
            CompilerException.AppendCodeInfo(compilerException, _lexer.GetIterator().GetPositionInfo());

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
