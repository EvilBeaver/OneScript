/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    partial class Compiler
    {
        private static readonly Dictionary<Token, OperationCode> _tokenToOpCode;

        private Parser _parser;
        private ICompilerContext _ctx;
        private ModuleImage _module;
        private Lexem _lastExtractedLexem;
        private bool _inMethodScope = false;
        private bool _isMethodsDefined = false;
        private bool _isStatementsDefined = false;
        private bool _isFunctionProcessed = false;
        private bool _isInTryBlock = false;

        private readonly Stack<Token[]> _tokenStack = new Stack<Token[]>();
        private readonly Stack<NestedLoopInfo> _nestedLoops = new Stack<NestedLoopInfo>();
        private readonly List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();

        private struct ForwardedMethodDecl
        {
            public string identifier;
            public bool[] factArguments;
            public bool asFunction;
            public int codeLine;
            public int commandIndex;
        }

        private struct NestedLoopInfo
        {
            public static NestedLoopInfo New()
            {
                return new NestedLoopInfo()
                {
                    startPoint = -1,
                    breakStatements = new List<int>()
                };
            }

            public int startPoint;
            public List<int> breakStatements;
        }

        public CompilerDirectiveHandler DirectiveHandler { get; set; }

        public Compiler()
        {
            
        }

        public ModuleImage Compile(Parser parser, ICompilerContext context)
        {
            _module = new ModuleImage();
            _ctx = context;
            _parser = parser;
            _parser.Start();

            BuildModule();
            CheckForwardedDeclarations();

            return _module;
        }

        private void BuildModule()
        {
            NextToken();

            if (_lastExtractedLexem.Type == LexemType.EndOfText)
            {
                return;
            }

            try
            {
                DispatchModuleBuild();
            }
            catch (CompilerException exc)
            {
                if(exc.LineNumber == 0)
                    AppendCodeInfo(_parser.CurrentLine, exc);
                throw;
            }


        }

        private void CheckForwardedDeclarations()
        {
            if (_forwardedMethods.Count > 0)
            {
                foreach (var item in _forwardedMethods)
                {
                    SymbolBinding methN;
                    try
                    {
                        methN = _ctx.GetMethod(item.identifier);
                    }
                    catch (CompilerException exc)
                    {
                        AppendCodeInfo(item.codeLine, exc);
                        throw;
                    }

                    var scope = _ctx.GetScope(methN.ContextIndex);

                    var methInfo = scope.GetMethod(methN.CodeIndex);
                    System.Diagnostics.Debug.Assert(StringComparer.OrdinalIgnoreCase.Compare(methInfo.Name, item.identifier) == 0);
                    if (item.asFunction && !methInfo.IsFunction)
                    {
                        var exc = CompilerException.UseProcAsFunction();
                        AppendCodeInfo(item.codeLine, exc);
                        throw exc;
                    }

                    try
                    {
                        CheckFactArguments(methInfo, item.factArguments);
                    }
                    catch (CompilerException exc)
                    {
                        AppendCodeInfo(item.codeLine, exc);
                        throw;
                    }

                    var cmd = _module.Code[item.commandIndex];
                    cmd.Argument = GetMethodRefNumber(ref methN);
                    _module.Code[item.commandIndex] = cmd;
                }
            }
        }

        private void AppendCodeInfo(int line, CompilerException exc)
        {
            var cp = new CodePositionInfo();
            cp.LineNumber = line;
            cp.Code = _parser.GetCodeLine(line);
            CompilerException.AppendCodeInfo(exc, cp);
        }

        private void DispatchModuleBuild()
        {
            bool isCodeEntered = false;

            while (_lastExtractedLexem.Type != LexemType.EndOfText)
            {
                if (_lastExtractedLexem.Type == LexemType.Identifier)
                {
                    if (_lastExtractedLexem.Token == Token.VarDef)
                    {
                        isCodeEntered = true;
                        BuildVariableDefinitions();
                    }
                    else if (_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function)
                    {
                        isCodeEntered = true;
                        _isMethodsDefined = true;
                        BuildSingleMethod();
                    }
                    else
                    {
                        isCodeEntered = true;
                        BuildModuleBody();
                    }
                }
                else if(_lastExtractedLexem.Type == LexemType.Directive)
                {
                    HandleDirective(isCodeEntered);
                }
                else
                {
                    throw CompilerException.UnexpectedOperation();
                }
            }
        }

        private void BuildVariableDefinitions()
        {
            while (_lastExtractedLexem.Token == Token.VarDef)
            {
                NextToken();
                while (true)
                {

                    if (IsUserSymbol(ref _lastExtractedLexem))
                    {
                        var symbolicName = _lastExtractedLexem.Content;
                        var definition = _ctx.DefineVariable(symbolicName);
                        if (_inMethodScope)
                        {
                            if (_isStatementsDefined)
                            {
                                throw CompilerException.LateVarDefinition();
                            }
                        }
                        else
                        {
                            if (_isMethodsDefined)
                            {
                                throw CompilerException.LateVarDefinition();
                            }

                            _module.VariableRefs.Add(definition);
                            _module.VariableFrameSize++;
                        }
                        NextToken();
                        if (_lastExtractedLexem.Token == Token.Export)
                        {
                            _module.ExportedProperties.Add(new ExportedSymbol()
                            {
                                SymbolicName = symbolicName,
                                Index = definition.CodeIndex
                            });
                            NextToken();
                        }
                        if (_lastExtractedLexem.Token == Token.Comma)
                        {
                            NextToken();
                            continue;
                        }
                        if (_lastExtractedLexem.Token != Token.Semicolon)
                        {
                            throw CompilerException.SemicolonExpected();
                        }
                        NextToken();
                        break;
                    }
                    else
                    {
                        throw CompilerException.IdentifierExpected();
                    }

                }
            }

        }

        private void BuildModuleBody()
        {
            _isFunctionProcessed = false;
            PushStructureToken(Token.EndOfText);
            var entry = _module.Code.Count;

            _ctx.PushScope(new SymbolScope());
            try
            {
                BuildCodeBatch();
            }
            catch
            {
                _ctx.PopScope();
                throw;
            }
            PopStructureToken();

            var localCtx = _ctx.PopScope();
            
            var topIdx = _ctx.TopIndex();

            if (entry != _module.Code.Count)
            {
                var bodyMethod = new MethodInfo();
                bodyMethod.Name = "$entry";
                var descriptor = new MethodDescriptor();
                descriptor.EntryPoint = entry;
                descriptor.Signature = bodyMethod;
                descriptor.VariableFrameSize = localCtx.VariableCount;
                
                var entryRefNumber = _module.MethodRefs.Count;
                var bodyBinding = new SymbolBinding()
                {
                    ContextIndex = topIdx,
                    CodeIndex = _module.Methods.Count
                };
                _module.Methods.Add(descriptor);
                _module.MethodRefs.Add(bodyBinding);
                _module.EntryMethodIndex = entryRefNumber;
            }
        }

        private void HandleDirective(bool codeEntered)
        {
            var directive = _lastExtractedLexem.Content;
            var value = _parser.ReadLineToEnd().Trim();
            NextToken();

            if (DirectiveHandler == null || !DirectiveHandler(directive, value, codeEntered))
                throw new CompilerException(String.Format("Неизвестная директива: {0}({1})", directive, value));

        }

        private void BuildSingleMethod()
        {
            if (_lastExtractedLexem.Token == Token.Procedure)
            {
                PushStructureToken(Token.EndProcedure);
                _isFunctionProcessed = false;
                NextToken();
            }
            else if (_lastExtractedLexem.Token == Token.Function)
            {
                PushStructureToken(Token.EndFunction);
                _isFunctionProcessed = true;
                NextToken();
            }
            else
            {
                throw CompilerException.UnexpectedOperation();
            }

            #region Method signature
            // сигнатура
            if (!IsUserSymbol(ref _lastExtractedLexem))
            {
                throw CompilerException.IdentifierExpected();
            }

            int definitionLine = _parser.CurrentLine;
            MethodInfo method = new MethodInfo();
            method.Name = _lastExtractedLexem.Content;
            method.IsFunction = _isFunctionProcessed;

            NextToken();
            if (_lastExtractedLexem.Token != Token.OpenPar)
            {
                throw CompilerException.TokenExpected(Token.OpenPar);
            } 
            #endregion
            
            NextToken();

            #region Parameters list
            var paramsList = new List<ParameterDefinition>();
            var methodCtx = new SymbolScope();

            while (_lastExtractedLexem.Token != Token.ClosePar)
            {
                var param = new ParameterDefinition();
                string name;

                if (_lastExtractedLexem.Token == Token.ByValParam)
                {
                    param.IsByValue = true;
                    NextToken();
                    if (IsUserSymbol(ref _lastExtractedLexem))
                    {
                        name = _lastExtractedLexem.Content;
                    }
                    else
                    {
                        throw CompilerException.IdentifierExpected();
                    }
                }
                else if (IsUserSymbol(ref _lastExtractedLexem))
                {
                    param.IsByValue = false;
                    name = _lastExtractedLexem.Content;
                }
                else
                {
                    throw CompilerException.UnexpectedOperation();
                }

                NextToken();
                if (_lastExtractedLexem.Token == Token.Equal)
                {
                    param.HasDefaultValue = true;
                    NextToken();
                    if (IsLiteral(ref _lastExtractedLexem))
                    {
                        var cd = CreateConstDefinition(ref _lastExtractedLexem);
                        var num = GetConstNumber(ref cd);
                        param.DefaultValueIndex = num;
                        NextToken();
                    }
                    else
                    {
                        throw CompilerException.UnexpectedOperation();
                    }
                }

                if (_lastExtractedLexem.Token == Token.Comma || _lastExtractedLexem.Token == Token.ClosePar)
                {
                    paramsList.Add(param);
                    methodCtx.DefineVariable(name);
                    
                    if(_lastExtractedLexem.Token != Token.ClosePar)
                        NextToken();
                }
                else
                {
                    throw CompilerException.TokenExpected(Token.Comma);
                }
            }

            method.Params = paramsList.ToArray();
 
            #endregion

            NextToken();
            bool isExportedMethod = false;
            if (_lastExtractedLexem.Token == Token.Export)
            {
                isExportedMethod = true;
                NextToken();
            }

            #region Body
            // тело
            var entryPoint = _module.Code.Count;

            try
            {
                _ctx.PushScope(methodCtx);
                DispatchMethodBody();
            }
            finally
            {
                _ctx.PopScope();
            }
            
            PopStructureToken();

            var descriptor = new MethodDescriptor();
            descriptor.EntryPoint = entryPoint;
            descriptor.Signature = method;
            descriptor.VariableFrameSize = methodCtx.VariableCount;
            SymbolBinding binding;
            try
            {
                binding = _ctx.DefineMethod(method);
            }
            catch (CompilerException)
            {
                var exc = new CompilerException("Метод с таким именем уже определен: " + method.Name);
                exc.LineNumber = definitionLine;
                exc.Code = _parser.GetCodeLine(exc.LineNumber);
                throw exc;
            }
            _module.MethodRefs.Add(binding);
            _module.Methods.Add(descriptor);

            if (isExportedMethod)
            {
                _module.ExportedMethods.Add(new ExportedSymbol()
                {
                    SymbolicName = method.Name,
                    Index = binding.CodeIndex
                });
            }

            #endregion

            NextToken(); 
            
        }

        private void DispatchMethodBody()
        {
            _inMethodScope = true;
            BuildVariableDefinitions();
            _isStatementsDefined = true;
            BuildCodeBatch();
            if (_isFunctionProcessed)
            {
                var undefConst = new ConstDefinition()
                {
                    Type = DataType.Undefined,
                    Presentation = "Неопределено"
                };

                AddCommand(OperationCode.PushConst, GetConstNumber(ref undefConst));
                
            }
            AddCommand(OperationCode.Return, 0);
            _isStatementsDefined = false;
            _inMethodScope = false;
        }

        private void BuildCodeBatch()
        {
            var endTokens = _tokenStack.Peek();

            while (true)
            {
                if (endTokens.Contains(_lastExtractedLexem.Token))
                {
                    return;
                }
                if (_lastExtractedLexem.Token == Token.Semicolon)
                {
                    NextToken();
                    continue;
                }

                if (_lastExtractedLexem.Type != LexemType.Identifier && _lastExtractedLexem.Token != Token.EndOfText)
                {
                    throw CompilerException.UnexpectedOperation();
                }

                if (_lastExtractedLexem.Token == Token.NotAToken)
                {
                    BuildSimpleStatement();
                }
                else
                {
                    BuildComplexStructureStatement();
                }

                //NextToken();
                if (_lastExtractedLexem.Token != Token.Semicolon)
                {
                    if (endTokens.Contains(_lastExtractedLexem.Token))
                    {
                        break;
                    }
                    else
                    {
                        throw CompilerException.SemicolonExpected();
                    }
                }
                else
                {
                    NextToken();
                }
            }

        }

        private void BuildComplexStructureStatement()
        {
            AddCommand(OperationCode.LineNum, _parser.CurrentLine);
            switch (_lastExtractedLexem.Token)
            {
                case Token.VarDef:
                    BuildVariableDefinitions();
                    break;
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
                default:
                    var expected = PopStructureToken();
                    throw CompilerException.TokenExpected(expected);
            }
        }

        private void BuildIfStatement()
        {
            var exitIndices = new List<int>();
            NextToken();
            BuildExpression(Token.Then);
            PushStructureToken(Token.Else, Token.ElseIf, Token.EndIf);

            var jumpFalseIndex = AddCommand(OperationCode.JmpFalse, 0);

            NextToken();
            BuildCodeBatch();
            PopStructureToken();
            exitIndices.Add(AddCommand(OperationCode.Jmp, 0));

            bool hasAlternativeBranches = false;

            while (_lastExtractedLexem.Token == Token.ElseIf)
            {
                hasAlternativeBranches = true;
                _module.Code[jumpFalseIndex] = new Command()
                {
                    Code = OperationCode.JmpFalse,
                    Argument = _module.Code.Count
                };
                NextToken();
                BuildExpression(Token.Then);
                PushStructureToken(Token.Else, Token.ElseIf, Token.EndIf);

                jumpFalseIndex = AddCommand(OperationCode.JmpFalse, -1);
                NextToken();
                BuildCodeBatch();
                PopStructureToken();
                exitIndices.Add(AddCommand(OperationCode.Jmp, -1));
                hasAlternativeBranches = false;
            }

            if (_lastExtractedLexem.Token == Token.Else)
            {
                hasAlternativeBranches = true;
                _module.Code[jumpFalseIndex] = new Command()
                {
                    Code = OperationCode.JmpFalse,
                    Argument = _module.Code.Count
                };
                NextToken();
                PushStructureToken(Token.EndIf);
                BuildCodeBatch();
                PopStructureToken();
            }

            if (!hasAlternativeBranches)
            {
                _module.Code[jumpFalseIndex] = new Command()
                {
                    Code = OperationCode.JmpFalse,
                    Argument = _module.Code.Count
                };
            }

            var exitIndex = AddCommand(OperationCode.Nop, 0);
            foreach (var indexToWrite in exitIndices)
            {
                _module.Code[indexToWrite] = new Command()
                {
                    Code = OperationCode.Jmp,
                    Argument = exitIndex
                };
            }
            NextToken();

        }

        private void BuildForStatement()
        {
            NextToken();
            if (_lastExtractedLexem.Token == Token.Each)
            {
                BuildForEachStatement();
            }
            else if (_lastExtractedLexem.Type == LexemType.Identifier)
            {
                BuildCountableForStatement();
            }
            else
            {
                throw CompilerException.IdentifierExpected();
            }


        }

        private void BuildForEachStatement()
        {
            Assert(_lastExtractedLexem.Token == Token.Each);
            NextToken();

            if (!IsUserSymbol(ref _lastExtractedLexem))
                throw CompilerException.IdentifierExpected();

            var identifier = _lastExtractedLexem.Content;
            NextToken();
            if(_lastExtractedLexem.Token != Token.In)
                throw CompilerException.TokenExpected(Token.In);

            NextToken();
            BuildExpression(Token.Loop);
            AddCommand(OperationCode.PushIterator, 0);
            var loopBegin = AddCommand(OperationCode.IteratorNext, 0);
            var condition = AddCommand(OperationCode.JmpFalse, -1);
            BuildLoadVariable(identifier);
            PushStructureToken(Token.EndLoop);

            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = loopBegin;
            _nestedLoops.Push(loopRecord);

            NextToken();
            bool savedTryFlag = SetTryBlockFlag(false);
            BuildCodeBatch();
            SetTryBlockFlag(savedTryFlag);
            PopStructureToken();

            AddCommand(OperationCode.Jmp, loopBegin);
            var cmd = _module.Code[condition];
            cmd.Argument = AddCommand(OperationCode.StopIterator, 0);
            _module.Code[condition] = cmd;

            CorrectBreakStatements(_nestedLoops.Pop(), cmd.Argument);
            NextToken();
        }

        private void BuildCountableForStatement()
        {
            string counter = _lastExtractedLexem.Content;
            NextToken();
            if (_lastExtractedLexem.Token != Token.Equal)
            {
                throw CompilerException.TokenExpected(Token.Equal);
            }
            NextToken();
            BuildExpression(Token.To);
            BuildLoadVariable(counter);
            NextToken();
            BuildExpression(Token.Loop);
            AddCommand(OperationCode.MakeRawValue, 0);
            AddCommand(OperationCode.PushTmp, 0);
            var lastIdx = _module.Code.Count;
            AddCommand(OperationCode.Jmp, lastIdx + 4);
            // increment
            var indexLoopBegin = BuildPushVariable(counter);
            AddCommand(OperationCode.Inc, 0);
            BuildLoadVariable(counter);

            BuildPushVariable(counter);
            var conditionIndex = AddCommand(OperationCode.JmpCounter, -1);
            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = indexLoopBegin;
            _nestedLoops.Push(loopRecord);
            NextToken();
            PushStructureToken(Token.EndLoop);
            bool savedTryFlag = SetTryBlockFlag(false);
            BuildCodeBatch();
            SetTryBlockFlag(savedTryFlag);
            PopStructureToken();

            // jmp to start
            AddCommand(OperationCode.Jmp, indexLoopBegin);
            var indexLoopEnd = AddCommand(OperationCode.PopTmp, 1);
            
            var cmd = _module.Code[conditionIndex];
            cmd.Argument = indexLoopEnd;
            _module.Code[conditionIndex] = cmd;

            CorrectBreakStatements(_nestedLoops.Pop(), indexLoopEnd);
            NextToken();
            
        }

        private void BuildWhileStatement()
        {
            NextToken();
            var conditionIndex = _module.Code.Count;
            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = conditionIndex;
            _nestedLoops.Push(loopRecord);
            BuildExpression(Token.Loop);
            PushStructureToken(Token.EndLoop);

            var jumpFalseIndex = AddCommand(OperationCode.JmpFalse, 0);
            NextToken();
            bool savedTryFlag = SetTryBlockFlag(false);
            BuildCodeBatch();
            SetTryBlockFlag(savedTryFlag);
            PopStructureToken();
            AddCommand(OperationCode.Jmp, conditionIndex);
            
            var endLoop = AddCommand(OperationCode.Nop, 0);
            _module.Code[jumpFalseIndex] = new Command()
            {
                Code = OperationCode.JmpFalse,
                Argument = endLoop
            };

            CorrectBreakStatements(_nestedLoops.Pop(), endLoop);

            NextToken();

        }

        private void BuildBreakStatement()
        {
            if (_nestedLoops.Count == 0)
            {
                throw CompilerException.BreakOutsideOfLoop();
            }

            var loopInfo = _nestedLoops.Peek();
            if(_isInTryBlock)
                AddCommand(OperationCode.EndTry, 0);
            var idx = AddCommand(OperationCode.Jmp, -1);
            loopInfo.breakStatements.Add(idx);
            NextToken();
        }

        private void BuildContinueStatement()
        {
            if (_nestedLoops.Count == 0)
            {
                throw CompilerException.ContinueOutsideOfLoop();
            }

            var loopInfo = _nestedLoops.Peek();
            if(_isInTryBlock)
                AddCommand(OperationCode.EndTry, 0);
            AddCommand(OperationCode.Jmp, loopInfo.startPoint);
            NextToken();
        }

        private void BuildReturnStatement()
        {
            if (_isFunctionProcessed)
            {
                NextToken();
                if (_lastExtractedLexem.Token == Token.Semicolon)
                {
                    throw CompilerException.FuncEmptyReturnValue();
                }
                BuildExpression(Token.Semicolon);
                AddCommand(OperationCode.MakeRawValue, 0);
            }
            else if (_inMethodScope)
            {
                NextToken();
                if (_lastExtractedLexem.Token != Token.Semicolon)
                {
                    throw CompilerException.ProcReturnsAValue();
                }
            }
            else
            {
                throw CompilerException.ReturnOutsideOfMethod();
            }

            AddCommand(OperationCode.Return, 0);
        }

        private void BuildTryExceptStatement()
        {
            var beginTryIndex = AddCommand(OperationCode.BeginTry, -1);
            bool savedTryFlag = SetTryBlockFlag(true);
            PushStructureToken(Token.Exception);
            NextToken();
            BuildCodeBatch();
            PopStructureToken();
            SetTryBlockFlag(savedTryFlag);
            var jmpIndex = AddCommand(OperationCode.Jmp, -1);

            CorrectCommandArgument(beginTryIndex, _module.Code.Count);

            Assert(_lastExtractedLexem.Token == Token.Exception);
            if(StringComparer.OrdinalIgnoreCase.Compare(_lastExtractedLexem.Content, "Exception") == 0)
                SystemLogger.Write("WARNING! BREAKING CHANGE: Keyword 'Exception' is not supported anymore. Consider using 'Except'");

            PushStructureToken(Token.EndTry);
            NextToken();
            BuildCodeBatch();
            PopStructureToken();

            var endIndex = AddCommand(OperationCode.EndTry, 0);
            CorrectCommandArgument(jmpIndex, endIndex);
            
            NextToken();
        }

        private void BuildRaiseExceptionStatement()
        {
            NextToken();
            if (_lastExtractedLexem.Token == Token.Semicolon)
            {
                if (_tokenStack.Any(x => x.Contains(Token.EndTry)))
                {
                    AddCommand(OperationCode.RaiseException, -1);
                }
                else
                {
                    throw CompilerException.MismatchedRaiseException();
                }
            }
            else
            {
                BuildExpression(Token.Semicolon);
                AddCommand(OperationCode.RaiseException, 0);
            }

        }

        private void CorrectCommandArgument(int index, int newArgument)
        {
            var cmd = _module.Code[index];
            cmd.Argument = newArgument;
            _module.Code[index] = cmd;
        }

        private void CorrectBreakStatements(NestedLoopInfo nestedLoopInfo, int endLoopIndex)
        {
            foreach (var breakCmdIndex in nestedLoopInfo.breakStatements)
            {
                CorrectCommandArgument(breakCmdIndex, endLoopIndex);
            }
        }

        private bool SetTryBlockFlag(bool isInTry)
        {
            bool current = _isInTryBlock;
            _isInTryBlock = isInTry;
            return current;
        }

        private void BuildSimpleStatement()
        {
            var identifier = _lastExtractedLexem.Content;

            NextToken();
            AddCommand(OperationCode.LineNum, _parser.CurrentLine);
            switch (_lastExtractedLexem.Token)
            {
                case Token.Equal:
                    NextToken();
                    BuildExpression(Token.Semicolon);
                    BuildLoadVariable(identifier);
                    break;
                case Token.OpenPar:
                    ProcessCallOnLeftHand(identifier);
                    break;
                case Token.Dot:
                case Token.OpenBracket:
                    // access chain
                    BuildPushVariable(identifier);
                    BuildAccessChainLeftHand();
                    break;
                default:
                    throw CompilerException.UnexpectedOperation();
            }
        }

        private void ProcessCallOnLeftHand(string identifier)
        {
            var args = PushMethodArgumentsBeforeCall();
            if(IsContinuationToken(ref _lastExtractedLexem))
            {
                BuildMethodCall(identifier, args, true);
                BuildAccessChainLeftHand();
            }
            else
            {
                BuildMethodCall(identifier, args, false);
            }

        }

        private void BuildAccessChainLeftHand()
        {
            string ident;
            BuildContinuationLeftHand(out ident);

            if (ident == null)
            {
                // это присваивание
                if (_lastExtractedLexem.Token != Token.Equal)
                    throw CompilerException.UnexpectedOperation();

                NextToken(); // перешли к выражению
                BuildExpression(Token.Semicolon);
                AddCommand(OperationCode.AssignRef, 0);

            }
            else
            {
                // это вызов
                System.Diagnostics.Debug.Assert(_lastExtractedLexem.Token == Token.OpenPar);
                var args = PushMethodArgumentsBeforeCall();
                var cDef = new ConstDefinition();
                cDef.Type = DataType.String;
                cDef.Presentation = ident;
                int lastIdentifierConst = GetConstNumber(ref cDef);

                if (IsContinuationToken(ref _lastExtractedLexem))
                {
                    AddCommand(OperationCode.ResolveMethodFunc, lastIdentifierConst);
                    BuildAccessChainLeftHand();
                }
                else
                {
                    AddCommand(OperationCode.ResolveMethodProc, lastIdentifierConst);
                }

            }
        }

        private bool IsContinuationToken(ref Lexem lex)
        {
            return lex.Token == Token.Dot || lex.Token == Token.OpenBracket;
        }

        private void BuildExpression(Token stopToken)
        {
            BuildPrimaryNode();
            BuildOperation(0);
            if (_lastExtractedLexem.Token == stopToken)
                return;

            var endTokens = _tokenStack.Peek();

            if (endTokens.Contains(_lastExtractedLexem.Token))
                return;
            else if (_lastExtractedLexem.Token == Token.EndOfText)
                throw CompilerException.UnexpectedEndOfText();
            else
                throw CompilerException.ExpressionSyntax();
        }

        private void BuildOperation(int acceptablePriority)
        {
            var currentOp = _lastExtractedLexem.Token;
            var opPriority = GetBinaryPriority(currentOp);
            while (LanguageDef.IsBinaryOperator(currentOp) && opPriority >= acceptablePriority)
            {
                bool isLogical = LanguageDef.IsLogicalOperator(currentOp);
                int logicalCmdIndex = -1;

                if (isLogical)
                {
                    logicalCmdIndex = AddCommand(TokenToOperationCode(currentOp), 0);
                }

                NextToken();
                BuildPrimaryNode();

                var newOp = _lastExtractedLexem.Token;
                int newPriority = GetBinaryPriority(newOp);

                if (newPriority > opPriority)
                {
                    BuildOperation(newPriority);
                }

                if (isLogical)
                {
                    currentOp = _lastExtractedLexem.Token;
                    newPriority = GetBinaryPriority(currentOp);
                    if(opPriority < newPriority)
                    {
                        BuildOperation(newPriority);
                    }

                    AddCommand(OperationCode.MakeBool, 0);
                    CorrectCommandArgument(logicalCmdIndex, _module.Code.Count);
                }
                else
                {
                    var opCode = TokenToOperationCode(currentOp);
                    AddCommand(opCode, 0);
                }

                currentOp = _lastExtractedLexem.Token;
                opPriority = GetBinaryPriority(currentOp);

            }
        }

        private static OperationCode TokenToOperationCode(Token stackOp)
        {
            OperationCode opCode;
            switch (stackOp)
            {
                case Token.Equal:
                    opCode = OperationCode.Equals;
                    break;
                case Token.NotEqual:
                    opCode = OperationCode.NotEqual;
                    break;
                case Token.Plus:
                    opCode = OperationCode.Add;
                    break;
                case Token.Minus:
                    opCode = OperationCode.Sub;
                    break;
                case Token.Multiply:
                    opCode = OperationCode.Mul;
                    break;
                case Token.Division:
                    opCode = OperationCode.Div;
                    break;
                case Token.Modulo:
                    opCode = OperationCode.Mod;
                    break;
                case Token.UnaryMinus:
                    opCode = OperationCode.Neg;
                    break;
                case Token.And:
                    opCode = OperationCode.And;
                    break;
                case Token.Or:
                    opCode = OperationCode.Or;
                    break;
                case Token.Not:
                    opCode = OperationCode.Not;
                    break;
                case Token.LessThan:
                    opCode = OperationCode.Less;
                    break;
                case Token.LessOrEqual:
                    opCode = OperationCode.LessOrEqual;
                    break;
                case Token.MoreThan:
                    opCode = OperationCode.Greater;
                    break;
                case Token.MoreOrEqual:
                    opCode = OperationCode.GreaterOrEqual;
                    break;
                default:
                    throw new NotSupportedException();
            }
            return opCode;
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

        private void BuildPrimaryNode()
        {
            if (LanguageDef.IsLiteral(ref _lastExtractedLexem))
            {
                BuildPushConstant();
                NextToken();
            }
            else if (LanguageDef.IsUserSymbol(ref _lastExtractedLexem))
            {
                ProcessPrimaryIdentifier();
            }
            else if (_lastExtractedLexem.Token == Token.Minus)
            {
                ProcessPrimaryUnaryMinus();
            }
            else if (_lastExtractedLexem.Token == Token.Not)
            {
                ProcessUnaryBoolean();
            }
            else if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                ProcessSubexpression();
                BuildContinuationRightHand();
            }
            else if(_lastExtractedLexem.Token == Token.NewObject)
            {
                BuildNewObjectCreation();
                BuildContinuationRightHand();
            }
            else if (LanguageDef.IsBuiltInFunction(_lastExtractedLexem.Token))
            {
                BuildBuiltinFunction();
                BuildContinuationRightHand();
            }
            else if (_lastExtractedLexem.Token == Token.Question)
            {
                BuildQuestionOperator();
            }
            else
            {
                throw CompilerException.ExpressionSyntax();
            }
        }
        private void ProcessPrimaryIdentifier()
        {
            var identifier = _lastExtractedLexem.Content;
            NextToken();
            if (IsContinuationToken(ref _lastExtractedLexem))
            {
                BuildPushVariable(identifier);
                BuildContinuationRightHand();
            }
            else if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                BuildFunctionCall(identifier);
                BuildContinuationRightHand();
            }
            else
            {
                BuildPushVariable(identifier);
            }
        }

        private void ProcessPrimaryUnaryMinus()
        {
            NextToken();
            if (!(LanguageDef.IsLiteral(ref _lastExtractedLexem)
                || LanguageDef.IsIdentifier(ref _lastExtractedLexem)
                || _lastExtractedLexem.Token == Token.OpenPar))
            {
                throw CompilerException.ExpressionExpected();
            }

            BuildPrimaryNode();
            AddCommand(OperationCode.Neg, 0);
        }

        private void ProcessSubexpression()
        {
            NextToken(); // съели открывающую скобку
            BuildPrimaryNode();
            BuildOperation(0);

            if (_lastExtractedLexem.Token != Token.ClosePar)
                throw CompilerException.TokenExpected(")");

            NextToken(); // съели закрывающую скобку
        }

        private void ProcessUnaryBoolean()
        {
            NextToken();
            BuildPrimaryNode();
            BuildOperation(GetBinaryPriority(Token.Not));
            AddCommand(OperationCode.Not, 0);
        }

        private void BuildContinuationRightHand()
        {
            string dummy;
            BuildContinuationInternal(false, out dummy);
        }

        private void BuildContinuationLeftHand(out string lastIdentifier)
        {
            BuildContinuationInternal(true, out lastIdentifier);
        }

        private void BuildContinuationInternal(bool interruptOnCall, out string lastIdentifier)
        {
            lastIdentifier = null;
            while (true)
            {
                if (_lastExtractedLexem.Token == Token.Dot)
                {
                    NextToken();
                    if (!LanguageDef.IsIdentifier(ref _lastExtractedLexem))
                        throw CompilerException.IdentifierExpected();

                    string identifier = _lastExtractedLexem.Content;
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.OpenPar)
                    {
                        if (interruptOnCall)
                        {
                            lastIdentifier = identifier;
                            return;
                        }
                        else
                        {
                            var args = BuildArgumentList();
                            var cDef = new ConstDefinition();
                            cDef.Type = DataType.String;
                            cDef.Presentation = identifier;
                            int lastIdentifierConst = GetConstNumber(ref cDef);
                            AddCommand(OperationCode.ArgNum, args.Length);
                            AddCommand(OperationCode.ResolveMethodFunc, lastIdentifierConst);
                        }
                    }
                    else
                    {
                        ResolveProperty(identifier);
                    }
                }
                else if (_lastExtractedLexem.Token == Token.OpenBracket)
                {
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.CloseBracket)
                        throw CompilerException.ExpressionExpected();

                    BuildExpression(Token.CloseBracket);
                    System.Diagnostics.Debug.Assert(_lastExtractedLexem.Token == Token.CloseBracket);
                    NextToken();

                    AddCommand(OperationCode.PushIndexed, 0);
                }
                else
                {
                    break;
                }
            }

        }

        private void ResolveProperty(string identifier)
        {
            var cDef = new ConstDefinition();
            cDef.Type = DataType.String;
            cDef.Presentation = identifier;
            var identifierConstIndex = GetConstNumber(ref cDef);
            AddCommand(OperationCode.ResolveProp, identifierConstIndex);
        }

        private void BuildQuestionOperator()
        {
            Assert(_lastExtractedLexem.Token == Token.Question);
            NextToken();
            if (_lastExtractedLexem.Token != Token.OpenPar)
                throw CompilerException.UnexpectedOperation();

            NextToken();
            BuildExpression(Token.Comma);
            if (_lastExtractedLexem.Token != Token.Comma)
                throw CompilerException.UnexpectedOperation();
            
            AddCommand(OperationCode.MakeBool, 0);
            var addrOfCondition = AddCommand(OperationCode.JmpFalse, -1);

            NextToken();
            BuildExpression(Token.Comma); // построили true-part
            if (_lastExtractedLexem.Token != Token.Comma)
                throw CompilerException.UnexpectedOperation();

            var endOfTruePart = AddCommand(OperationCode.Jmp, -1); // уход в конец оператора
            
            CorrectCommandArgument(addrOfCondition, AddCommand(OperationCode.Nop, 0)); // отметили, куда переходить по false
            NextToken();
            BuildExpression(Token.ClosePar); // построили false-part
            
            var endOfFalsePart = AddCommand(OperationCode.Nop, 0);
            CorrectCommandArgument(endOfTruePart, endOfFalsePart);
            
            NextToken();

        }

        private bool[] BuildArgumentList()
        {
            System.Diagnostics.Debug.Assert(_lastExtractedLexem.Token == Token.OpenPar);

            PushStructureToken(Token.ClosePar);
            List<bool> arguments = new List<bool>();
            
            NextToken();
            while (_lastExtractedLexem.Token != Token.ClosePar)
            {
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    AddCommand(OperationCode.PushDefaultArg, 0);
                    arguments.Add(false);
                    NextToken();
                    continue;
                }

                BuildExpression(Token.Comma);
                arguments.Add(true);
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.ClosePar)
                    {
                        // список аргументов кончился
                        AddCommand(OperationCode.PushDefaultArg, 0);
                        arguments.Add(false);
                    }
                }
            }

            if (_lastExtractedLexem.Token != Token.ClosePar)
                throw CompilerException.TokenExpected(")");

            NextToken(); // съели закрывающую скобку
            PopStructureToken();

            return arguments.ToArray();
        }

        private void BuildLoadVariable(string identifier)
        {
            try
            {
                var varBinding = _ctx.GetVariable(identifier);
                if (varBinding.binding.ContextIndex == _ctx.TopIndex())
                {
                    AddCommand(OperationCode.LoadLoc, varBinding.binding.CodeIndex);
                }
                else
                {
                    var num = GetVariableRefNumber(ref varBinding.binding);
                    AddCommand(OperationCode.LoadVar, num);
                }
            }
            catch (SymbolNotFoundException)
            {
                // can create variable
                var binding = _ctx.DefineVariable(identifier);
                AddCommand(OperationCode.LoadLoc, binding.CodeIndex);
            }
        }

        private void BuildIndexedAccess(string identifier)
        {
            BuildPushVariable(identifier);
            NextToken();
            BuildExpression(Token.CloseBracket);
            AddCommand(OperationCode.PushIndexed, 0);
        }

        private void BuildProcedureCall(string identifier)
        {
            var args = PushMethodArgumentsBeforeCall();
            BuildMethodCall(identifier, args, false);
        }

        private void BuildFunctionCall(string identifier)
        {
            bool[] args = PushMethodArgumentsBeforeCall();
            BuildMethodCall(identifier, args, true);
        }

        private bool[] PushMethodArgumentsBeforeCall()
        {
            var argsPassed = BuildArgumentList();
            AddCommand(OperationCode.ArgNum, argsPassed.Length);
            return argsPassed;
        }

        private int BuildMethodCall(string identifier, bool[] argsPassed, bool asFunction)
        {
            try
            {
                var methBinding = _ctx.GetMethod(identifier);
                var scope = _ctx.GetScope(methBinding.ContextIndex);
                
                // dynamic scope checks signatures only at runtime
                if (!scope.IsDynamicScope)
                {
                    var methInfo = scope.GetMethod(methBinding.CodeIndex);
                    if (asFunction && !methInfo.IsFunction)
                    {
                        throw CompilerException.UseProcAsFunction();
                    }
                    CheckFactArguments(methInfo, argsPassed);
                }
                
                int callAddr;
                
                if(asFunction)
                    callAddr = AddCommand(OperationCode.CallFunc, GetMethodRefNumber(ref methBinding));
                else
                    callAddr = AddCommand(OperationCode.CallProc, GetMethodRefNumber(ref methBinding));
                
                return callAddr;
            }
            catch (SymbolNotFoundException)
            {
                // can be defined later
                var forwarded = new ForwardedMethodDecl();
                forwarded.identifier = identifier;
                forwarded.asFunction = asFunction;
                forwarded.codeLine = _parser.CurrentLine;
                forwarded.factArguments = argsPassed;

                var opCode = asFunction ? OperationCode.CallFunc : OperationCode.CallProc;
                int callAddr = forwarded.commandIndex = AddCommand(opCode, -1);
                _forwardedMethods.Add(forwarded);

                return callAddr;
            }
        }

        private void CheckFactArguments(MethodInfo methInfo, bool[] argsPassed)
        {
            CheckFactArguments(methInfo.Params, argsPassed);
        }

        private void CheckFactArguments(ParameterDefinition[] parameters, bool[] argsPassed)
        {
            if (argsPassed.Length > parameters.Length)
            {
                throw CompilerException.TooManyArgumentsPassed();
            }

            for (int i = 0; i < parameters.Length; i++)
            {
                var paramDef = parameters[i];
                if (i < argsPassed.Length)
                {
                    if (argsPassed[i] == false && !paramDef.HasDefaultValue)
                    {
                        throw CompilerException.ArgHasNoDefaultValue(i + 1);
                    }
                }
                else if (!paramDef.HasDefaultValue)
                {
                    throw CompilerException.TooLittleArgumentsPassed();
                }
            }
        }

        private void BuildNewObjectCreation()
        {
            NextToken();
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                // создание по строковому имени класса
                NewObjectDynamicConstructor();

            }
            else if (IsUserSymbol(ref _lastExtractedLexem) || _lastExtractedLexem.Token == Token.ExceptionInfo)
            {
                NewObjectStaticConstructor();
            }
            else
            {
                throw CompilerException.IdentifierExpected();
            }

        }

        private void NewObjectDynamicConstructor()
        {
            var argsPassed = BuildArgumentList();
            if (argsPassed.Length == 0)
                throw CompilerException.ExpressionExpected();

            AddCommand(OperationCode.NewInstance, argsPassed.Length-1);
        }

        private void NewObjectStaticConstructor()
        {
            var name = _lastExtractedLexem.Content;
            var cDef = new ConstDefinition()
            {
                Type = DataType.String,
                Presentation = name
            };

            AddCommand(OperationCode.PushConst, GetConstNumber(ref cDef));

            NextToken();
            bool[] argsPassed;
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                // Отрабатываем только в тех случаях, если явно указана скобка.
                // В остальных случаях дальнейшую обработку отдаём наружу
                argsPassed = BuildArgumentList();
            }
            else
            {
                argsPassed = new bool[0];
                // TODO: разобраться с костылем про BackOneToken()
                //if (_lastExtractedLexem.Token == Token.ClosePar)
                    //BackOneToken();
            }

            AddCommand(OperationCode.NewInstance, argsPassed.Length);
        }

        private void BuildPushConstant()
        {
            var cDef = CreateConstDefinition(ref _lastExtractedLexem);
            var num = GetConstNumber(ref cDef);
            AddCommand(OperationCode.PushConst, num);
        }

        private int BuildPushVariable(string identifier)
        {
            var varNum = _ctx.GetVariable(identifier);
            if (varNum.type == SymbolType.ContextProperty)
            {
                return PushReference(varNum.binding);
            }
            else
            {
                return PushSimpleVariable(varNum.binding);
            }
        }

        private void BuildBuiltinFunction()
        {
            OperationCode funcId = BuiltInFunctionCode(_lastExtractedLexem.Token);
            NextToken();
            if(_lastExtractedLexem.Token != Token.OpenPar)
            {
                throw CompilerException.TokenExpected(Token.OpenPar);
            }

            var passedArgs = BuildArgumentList();
            if (funcId == OperationCode.Min || funcId == OperationCode.Max)
            {
                if (passedArgs.Length == 0)
                    throw CompilerException.TooLittleArgumentsPassed();
            }
            else
            {
                var parameters = BuiltinFunctions.ParametersInfo(funcId);
                CheckFactArguments(parameters, passedArgs);
            }

            AddCommand(funcId, passedArgs.Length);

        }

        #region Helper methods

        private static bool IsUserSymbol(ref Lexem lex)
        {
            return LanguageDef.IsUserSymbol(ref lex);
        }

        private static bool IsLiteral(ref Lexem lex)
        {
            return LanguageDef.IsLiteral(ref lex);
        }

        private static OperationCode BuiltInFunctionCode(Token token)
        {
            return _tokenToOpCode[token];
        }

        private static ConstDefinition CreateConstDefinition(ref Lexem lex)
        {
            DataType constType = DataType.Undefined;
            switch (lex.Type)
            {
                case LexemType.BooleanLiteral:
                    constType = DataType.Boolean;
                    break;
                case LexemType.DateLiteral:
                    constType = DataType.Date;
                    break;
                case LexemType.NumberLiteral:
                    constType = DataType.Number;
                    break;
                case LexemType.StringLiteral:
                    constType = DataType.String;
                    break;
                case LexemType.NullLiteral:
                    constType = DataType.GenericValue;
                    break;
            }

            ConstDefinition cDef = new ConstDefinition()
            {
                Type = constType,
                Presentation = lex.Content
            };
            return cDef;
        }

        private int GetConstNumber(ref ConstDefinition cDef)
        {
            var idx = _module.Constants.IndexOf(cDef);
            if (idx < 0)
            {
                idx = _module.Constants.Count;
                _module.Constants.Add(cDef);
            }
            return idx;
        }

        private int GetMethodRefNumber(ref SymbolBinding methodBinding)
        {
            var idx = _module.MethodRefs.IndexOf(methodBinding);
            if (idx < 0)
            {
                idx = _module.MethodRefs.Count;
                _module.MethodRefs.Add(methodBinding);
            }
            return idx;
        }

        private int GetVariableRefNumber(ref SymbolBinding binding)
        {
            var idx = _module.VariableRefs.IndexOf(binding);
            if (idx < 0)
            {
                idx = _module.VariableRefs.Count;
                _module.VariableRefs.Add(binding);
            }

            return idx;
        }

        private void NextToken()
        {
            if (_lastExtractedLexem.Token != Token.EndOfText)
            {
                _lastExtractedLexem = _parser.NextLexem();
            }
            else
            {
                throw CompilerException.UnexpectedEndOfText();
            }
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

        private int AddCommand(OperationCode code, int arg)
        {
            var addr = _module.Code.Count;
            _module.Code.Add(new Command() { Code = code, Argument = arg });
            return addr;
        }

        private int PushSimpleVariable(SymbolBinding binding)
        {
            if (binding.ContextIndex == _ctx.TopIndex())
            {
                return AddCommand(OperationCode.PushLoc, binding.CodeIndex);
            }
            else
            {
                var idx = GetVariableRefNumber(ref binding);
                return AddCommand(OperationCode.PushVar, idx);
            }
        }

        private int PushReference(SymbolBinding binding)
        {
            var idx = GetVariableRefNumber(ref binding);

            return AddCommand(OperationCode.PushRef, idx);
        }

        #endregion

        [System.Diagnostics.Conditional("DEBUG")]
        private static void Assert(bool condition)
        {
            System.Diagnostics.Debug.Assert(condition);
        }

        static Compiler()
        {
            _tokenToOpCode = new Dictionary<Token, OperationCode>();

            var tokens  = LanguageDef.BuiltInFunctions();
            var opCodes = BuiltinFunctions.GetOperationCodes();

            Assert(tokens.Length == opCodes.Length);
            for (int i = 0; i < tokens.Length; i++)
            {
                _tokenToOpCode.Add(tokens[i], opCodes[i]);
            }
        }

    }

    public delegate bool CompilerDirectiveHandler(string directive, string value, bool codeEntered);

}
