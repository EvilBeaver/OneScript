/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
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

        private List<ParseError> _errors = new List<ParseError>();
        
        public DefaultBslParser(IAstBuilder builder)
        {
            _builder = builder;
        }

        public IEnumerable<ParseError> Errors => _errors; 
        
        public void ParseStatefulModule(Lexer lexer)
        {
            _lexer = lexer;
            //ParseDirectives();
            NextLexem();
            ParseModuleSections();
        }

        private void ParseModuleSections()
        {
            BuildVariableSection();
            //BuildMethodsSection();
            //BuildModuleBody();
        }

        private void BuildVariableSection()
        {
            bool started = false;
            while (true)
            {
                BuildAnnotations();
                if (_lastExtractedLexem.Token == Token.VarDef)
                {
                    if (!started)
                    {
                        _builder.StartVariablesSection();
                        started = true;
                    }
                    BuildVariableDefinition();
                }
                else
                {
                    break;
                }
            }
        }

        private void BuildMethodsSection()
        {
            throw new NotImplementedException();
        }

        private void BuildModuleBody()
        {
            throw new NotImplementedException();
        }

        private void BuildVariableDefinition()
        {
            while (true)
            {
                NextLexem();

                if (IsUserSymbol(_lastExtractedLexem))
                {
                    var symbolicName = _lastExtractedLexem.Content;
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

                    NextLexem();
                    if (_lastExtractedLexem.Token == Token.Export)
                    {
                        if (_inMethodScope)
                        {
                            AddError(LocalizedErrors.ExportedLocalVar(symbolicName));
                        }
                        _builder.CreateVarDefinition(symbolicName, true);
                        NextLexem();
                    }
                    else
                    {
                        _builder.CreateVarDefinition(symbolicName, false);
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

        #region Annotations
        private void BuildAnnotations()
        {
            while (_lastExtractedLexem.Type == LexemType.Annotation)
            {
                var node = _builder.CreateAnnotation(_lastExtractedLexem.Content);
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
                BuildAnnotationParameter(annotation);
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
        
        private void BuildAnnotationParameter(IAstNode annotation)
        {
            // id | id = value | value
            //var result = new AnnotationParameter();
            if (_lastExtractedLexem.Type == LexemType.Identifier)
            {
                var id = _lastExtractedLexem.Content;
                NextLexem();
                if (_lastExtractedLexem.Token != Token.Equal)
                {
                    _builder.AddAnnotationParameter(annotation, id);
                    return;
                }
                NextLexem();
                _builder.AddAnnotationParameter(annotation, id, _lastExtractedLexem);
            }
            else
            {
                _builder.AddAnnotationParameter(annotation, _lastExtractedLexem);
            }
            
            NextLexem();
        }

        #endregion
        
        private void NextLexem()
        {
            _lastExtractedLexem = _lexer.NextLexem();
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
            _builder.HandleParseError(err);
            SkipToNextStatement();
        }

        private static bool IsUserSymbol(in Lexem lex)
        {
            return LanguageDef.IsUserSymbol(in lex);
        }


    }
}