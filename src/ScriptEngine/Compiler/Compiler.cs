using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    class Compiler
    {
        private Parser _parser;
        private CompilerContext _ctx;
        private ModuleImage _module;
        private Lexem _lastExtractedLexem;
        private bool _inMethodScope = false;
        private bool _isMethodsDefined = false;
        private bool _isStatementsDefined = false;
        private bool _isFunctionProcessed = false;

        private Stack<Token[]> _tokenStack = new Stack<Token[]>();
        private Stack<NestedLoopInfo> _nestedLoops = new Stack<NestedLoopInfo>();
        private List<ForwardedMethodDecl> _forwardedMethods = new List<ForwardedMethodDecl>();

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

        public Compiler()
        {
            
        }

        public ModuleImage Compile(Parser parser, CompilerContext context)
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
            // резервируем место под переменные, определенные извне компилятора
            _module.VariableFrameSize = _ctx.GetScope(_ctx.TopIndex()).VariableCount;
            
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
                CompilerException.AppendLineNumber(exc, _parser.CurrentLine);
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
                        CompilerException.AppendLineNumber(exc, item.codeLine);
                        throw;
                    }

                    var methInfo = _module.Methods[methN.CodeIndex].Signature;
                    if (item.asFunction && !methInfo.IsFunction)
                    {
                        throw CompilerException.AppendLineNumber(CompilerException.UseProcAsFunction(), item.codeLine);
                    }

                    try
                    {
                        CheckFactArguments(methInfo, item.factArguments);
                    }
                    catch (CompilerException exc)
                    {
                        CompilerException.AppendLineNumber(exc, item.codeLine);
                        throw;
                    }

                    var cmd = _module.Code[item.commandIndex];
                    cmd.Argument = GetMethodRefNumber(ref methN);
                    _module.Code[item.commandIndex] = cmd;
                }
            }
        }

        private void DispatchModuleBuild()
        {
            if (_lastExtractedLexem.Type == LexemType.Identifier)
            {
                if (_lastExtractedLexem.Token == Token.VarDef)
                {
                    BuildVariableDefinitions();
                }
                else if (_lastExtractedLexem.Token == Token.Procedure || _lastExtractedLexem.Token == Token.Function)
                {
                    _isMethodsDefined = true;
                    BuildMethods();
                }
                else
                {
                    BuildModuleBody();
                }
            }
            else if (_lastExtractedLexem.Type != LexemType.EndOfText)
            {
                throw CompilerException.UnexpectedOperation();
            }
        }

        private void BuildVariableDefinitions()
        {
            while (_lastExtractedLexem.Token == Token.VarDef)
            {
                NextToken();
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
                    if (_lastExtractedLexem.Token != Token.Semicolon)
                    {
                        throw CompilerException.SemicolonExpected();
                    }
                    NextToken();
                }
                else
                {
                    throw CompilerException.IdentifierExpected();
                }
            }

            if (!_inMethodScope)
                DispatchModuleBuild();
        }

        private void BuildMethods()
        {
            BuildSingleMethod();
            DispatchModuleBuild();
        }

        private void BuildModuleBody()
        {
            _isFunctionProcessed = false;
            PushStructureToken(Token.EndOfText);
            var entry = _module.Code.Count;

            _ctx.PushScope(new SymbolScope());

            BuildCodeBatch();

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
                    throw CompilerException.UnexpectedOperation();
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

            _ctx.PushScope(methodCtx);
            DispatchMethodBody();
            _ctx.PopScope();
            PopStructureToken();

            var descriptor = new MethodDescriptor();
            descriptor.EntryPoint = entryPoint;
            descriptor.Signature = method;
            descriptor.VariableFrameSize = methodCtx.VariableCount;
            var binding = _ctx.DefineMethod(method);
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
            List<int> exitIndices = new List<int>();
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
            BuildCodeBatch();
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
            BuildCodeBatch();
            PopStructureToken();

            // jmp to start
            AddCommand(OperationCode.Jmp, indexLoopBegin);
            var indexLoopEnd = AddCommand(OperationCode.Nop, 0);
            
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
            BuildCodeBatch();
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
            PushStructureToken(Token.Exception);
            NextToken();
            BuildCodeBatch();
            PopStructureToken();

            var jmpIndex = AddCommand(OperationCode.Jmp, -1);

            CorrectCommandArgument(beginTryIndex, _module.Code.Count);

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

        private void CorrectCommandArgument(int beginTryIndex, int newArgument)
        {
            var cmd = _module.Code[beginTryIndex];
            cmd.Argument = newArgument;
            _module.Code[beginTryIndex] = cmd;
        }

        private void CorrectBreakStatements(NestedLoopInfo nestedLoopInfo, int endLoopIndex)
        {
            foreach (var breakCmdIndex in nestedLoopInfo.breakStatements)
            {
                CorrectCommandArgument(breakCmdIndex, endLoopIndex);
            }
        }

        private void BuildSimpleStatement()
        {
            var identifier = _lastExtractedLexem.Content;

            NextToken();
            switch (_lastExtractedLexem.Token)
            {
                case Token.OpenPar:
                    BuildProcedureCall(identifier);
                    break;
                case Token.OpenBracket:

                    BuildIndexedAccess(identifier);
                    NextToken();
                    BuildRefAssignment();
                    break;
                case Token.Dot:
                    // build resolve chain
                    BuildPushVariable(identifier);
                    BuildRefAssignment();
                    break;
                case Token.Equal:
                    NextToken();
                    BuildExpression(Token.Semicolon);

                    BuildLoadVariable(identifier);

                    break;
                default:
                    throw CompilerException.UnexpectedOperation();
            }
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

        private void BuildResolveChain(out int lastIdentifierConst)
        {
            lastIdentifierConst = -1;
            while (_lastExtractedLexem.Token == Token.Dot)
            {
                NextToken();
                if (IsUserSymbol(ref _lastExtractedLexem))
                {
                    var name = _lastExtractedLexem.Content;
                    var cDef = new ConstDefinition();
                    cDef.Type = DataType.String;
                    cDef.Presentation = name;
                    lastIdentifierConst = GetConstNumber(ref cDef);
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.OpenPar)
                    {
                        break;
                    }
                    else
                    {
                        AddCommand(OperationCode.ResolveProp, lastIdentifierConst);
                        break;
                    }
                }
                else
                {
                    throw CompilerException.IdentifierExpected();
                }
            }
            
        }

        private void ProcessResolvedItem(int identifierId, Token stopToken)
        {
            if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                Assert(identifierId >= 0);
                PrepareMethodArguments();
                NextToken();
                
                if (_lastExtractedLexem.Token == Token.Dot)
                {
                    AddCommand(OperationCode.ResolveMethodFunc, identifierId);
                    int lastId;
                    BuildResolveChain(out lastId);
                    ProcessResolvedItem(lastId, stopToken);
                }
                else if (_lastExtractedLexem.Type == LexemType.Operator || _lastExtractedLexem.Token == stopToken)
                {
                    AddCommand(OperationCode.ResolveMethodFunc, identifierId);
                }
            }
        }

        private void BuildRefAssignment()
        {
            bool hasAction = false;
            if (_lastExtractedLexem.Token == Token.Dot)
            {
                int identifier;
                BuildResolveChain(out identifier);
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    Assert(identifier >= 0);
                    hasAction = true;
                    PrepareMethodArguments();
                    NextToken();

                    if (_lastExtractedLexem.Token == Token.Semicolon)
                    {
                        AddCommand(OperationCode.ResolveMethodProc, identifier);
                        return;// no ref assignment happens.
                    }
                    else
                    {
                        if (_lastExtractedLexem.Token == Token.OpenBracket || _lastExtractedLexem.Token == Token.Dot)
                        {
                            AddCommand(OperationCode.ResolveMethodFunc, identifier);
                        }
                        else
                        {
                            throw CompilerException.UnexpectedOperation();
                        }
                    }
                }

                BuildRefAssignment(); // multiple indexer/resolver [][].[][].[]
                return;

            }
            else if (_lastExtractedLexem.Token == Token.OpenBracket)
            {
                NextToken();
                BuildExpression(Token.CloseBracket);
                AddCommand(OperationCode.PushIndexed, 0);
                NextToken();
                BuildRefAssignment(); // multiple indexer/resolver [][].[][].[]
                return;
            }

            if (_lastExtractedLexem.Token == Token.Equal)
            {
                NextToken();
                BuildExpression(Token.Semicolon);
                AddCommand(OperationCode.AssignRef, 0);
            }
            else if(!hasAction)//_lastExtractedLexem.Token != Token.Semicolon)
            {
                throw CompilerException.UnexpectedOperation();
            }

        }

        private void BuildProcedureCall(string identifier)
        {
            var args = PrepareMethodArguments();
            NextToken();
            if (_lastExtractedLexem.Token == Token.Semicolon)
            {
                BuildMethodCall(identifier, args, false);
            }
            else
            {
                if (_lastExtractedLexem.Token == Token.OpenBracket || _lastExtractedLexem.Token == Token.Dot)
                {
                    BuildMethodCall(identifier, args, true);
                    BuildRefAssignment();
                }
                else
                {
                    throw CompilerException.SemicolonExpected();
                }
            }
        }

        private void BuildFunctionCall(string identifier)
        {
            var args = PrepareMethodArguments();
            BuildMethodCall(identifier, args, true);
        }

        private bool[] PrepareMethodArguments()
        {
            var argsPassed = PushFactArguments();
            AddCommand(OperationCode.ArgNum, argsPassed.Length);
            return argsPassed;
        }

        private int BuildMethodCall(string identifier, bool[] argsPassed, bool asFunction)
        {
            try
            {
                var methBinding = _ctx.GetMethod(identifier);
                var scope = _ctx.GetScope(methBinding.ContextIndex);
                var methInfo = scope.GetMethod(methBinding.CodeIndex);
                if (asFunction && !methInfo.IsFunction)
                {
                    throw CompilerException.UseProcAsFunction();
                }
                
                // dynamic scope checks signatures only at runtime
                if(!scope.IsDynamicScope)
                    CheckFactArguments(methInfo, argsPassed);
                
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
                AddCommand(OperationCode.ArgNum, forwarded.factArguments.Length);

                int callAddr = forwarded.commandIndex = AddCommand(OperationCode.CallProc, -1);
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

        private bool[] PushFactArguments()
        {
            Assert(_lastExtractedLexem.Token == Token.OpenPar);

            List<bool> argsPassed = new List<bool>();
            NextToken();
            if (_lastExtractedLexem.Token == Token.ClosePar)
                return argsPassed.ToArray();

            while (true)
            {
                try
                {
                    BuildExpression(Token.Comma);
                    argsPassed.Add(true);
                    NextToken();
                    while (_lastExtractedLexem.Token == Token.Comma)
                    {
                        AddCommand(OperationCode.PushDefaultArg, 0);
                        argsPassed.Add(false);
                        NextToken();
                    }

                    if (_lastExtractedLexem.Token == Token.ClosePar)
                    {
                        return argsPassed.ToArray();
                    }
                    
                }
                catch (ExtraClosedParenthesis)
                {
                    argsPassed.Add(true);
                    return argsPassed.ToArray();
                }
            }

        }

        private void BuildNewObjectCreation()
        {
            NextToken();
            if (!IsUserSymbol(ref _lastExtractedLexem))
            {
                throw CompilerException.IdentifierExpected();
            }

            var name = _lastExtractedLexem.Content;
            NextToken();
            bool[] argsPassed;
            if (_lastExtractedLexem.Token == Token.OpenPar)
                argsPassed = PushFactArguments();
            else if (_lastExtractedLexem.Token == Token.Semicolon)
                argsPassed = new bool[0];
            else
                throw CompilerException.ExpressionSyntax();

            var cDef = new ConstDefinition()
            {
                Type=DataType.String,
                Presentation = name
            };

            AddCommand(OperationCode.PushConst, GetConstNumber(ref cDef));
            AddCommand(OperationCode.NewInstance, argsPassed.Length);

        }

        private void BuildExpression(Token stopToken)
        {
            const int STATE_UNDEF = 0;
            const int STATE_OPERAND_EXPECTED = 1;
            const int STATE_FUNC_WAIT = 2;
            const int STATE_OPERATOR_EXPECTED = 3;

            Stack<Token> operators = new Stack<Token>();
            
            int currentState = STATE_UNDEF;
            string waitingIdentifier = null;
            int parCount = 0;

            while (true)
            {
                bool success = false;

                if (_lastExtractedLexem.Token == stopToken)
                {
                    if (currentState == STATE_FUNC_WAIT)
                    {
                        BuildPushVariable(waitingIdentifier);
                    }
                    else if (currentState != STATE_OPERATOR_EXPECTED)
                    {
                        throw CompilerException.ExpressionSyntax();
                    }
                    break;
                }

                switch (currentState)
                {
                    case STATE_UNDEF:
                    case STATE_OPERAND_EXPECTED:
                        if (IsLiteral(ref _lastExtractedLexem))
                        {
                            BuildPushConstant();
                            currentState = STATE_OPERATOR_EXPECTED;
                            success = true;
                        }
                        else if (_lastExtractedLexem.Type == LexemType.Identifier)
                        {
                            if (_lastExtractedLexem.Token == Token.NotAToken)
                            {
                                waitingIdentifier = _lastExtractedLexem.Content;
                                currentState = STATE_FUNC_WAIT;
                                success = true;
                            }
                            else if (LanguageDef.IsBuiltInFunction(_lastExtractedLexem.Token))
                            {
                                BuildBuiltinFunction();
                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else if (_lastExtractedLexem.Token == Token.NewObject)
                            {
                                BuildNewObjectCreation();
                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else
                            {
                                throw CompilerException.TokenExpected(stopToken);
                            }
                        }
                        else if (_lastExtractedLexem.Token == Token.Minus)
                        {
                            operators.Push(Token.UnaryMinus);
                            currentState = STATE_OPERAND_EXPECTED;
                            success = true;
                        }
                        else if (_lastExtractedLexem.Token == Token.OpenPar)
                        {
                            operators.Push(Token.OpenPar);
                            parCount++;
                            currentState = STATE_UNDEF;
                            success = true;
                        }
                        else if (_lastExtractedLexem.Token == Token.Not)
                        {
                            operators.Push(Token.Not);
                            currentState = STATE_OPERAND_EXPECTED;
                            success = true;
                        }

                        break;
                    case STATE_FUNC_WAIT:
                        if (_lastExtractedLexem.Token == Token.OpenPar)
                        {
                            BuildFunctionCall(waitingIdentifier);
                            if(_lastExtractedLexem.Token == Token.ClosePar)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else if (_lastExtractedLexem.Type == LexemType.Operator)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                continue;
                            }
                        }
                        else if (_lastExtractedLexem.Token == Token.OpenBracket)
                        {
                            BuildIndexedAccess(waitingIdentifier);
                            currentState = STATE_OPERATOR_EXPECTED;
                            success = true;
                        }   
                        else if (_lastExtractedLexem.Token == Token.Dot)
                        {
                            BuildPushVariable(waitingIdentifier);
                            int identifier;
                            BuildResolveChain(out identifier);
                            ProcessResolvedItem(identifier, stopToken);

                            if (_lastExtractedLexem.Type == LexemType.Operator || _lastExtractedLexem.Token == stopToken)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                continue;
                            }
                        }
                        else if (_lastExtractedLexem.Type == LexemType.Operator && IsBinaryOperator(_lastExtractedLexem.Token))
                        {
                            BuildPushVariable(waitingIdentifier);
                            ProcessExpressionOperator(ref operators);
                            currentState = STATE_OPERAND_EXPECTED;
                            success = true;
                        }
                        else if (_lastExtractedLexem.Token == Token.ClosePar)
                        {
                            BuildPushVariable(waitingIdentifier);
                            currentState = STATE_OPERATOR_EXPECTED;
                            continue;
                        }

                        break;
                    case STATE_OPERATOR_EXPECTED:
                        if (_lastExtractedLexem.Type == LexemType.Operator && IsBinaryOperator(_lastExtractedLexem.Token))
                        {
                            ProcessExpressionOperator(ref operators);
                            currentState = STATE_OPERAND_EXPECTED;
                            success = true;
                        }
                        else if(_lastExtractedLexem.Token == Token.ClosePar)
                        {
                            Token current;
                            parCount--;
                            if (parCount < 0)
                            {
                                UnwindOperators(ref operators);
                                throw new ExtraClosedParenthesis(_parser.CurrentLine);
                            }

                            if (operators.Count > 0)
                            {
                                while ((current = operators.Pop()) != Token.OpenPar)
                                {
                                    AddCommand(TokenToOperationCode(current), 0);
                                    if (operators.Count == 0)
                                    {
                                        throw CompilerException.TokenExpected(Token.OpenPar);
                                    }
                                }

                                currentState = STATE_OPERATOR_EXPECTED;
                                success = true;
                            }
                            else
                            {
                                throw CompilerException.TokenExpected(Token.OpenPar);
                            }
                        }
                        else if (_lastExtractedLexem.Token == Token.OpenBracket)
                        {
                            NextToken();
                            BuildExpression(Token.CloseBracket);
                            AddCommand(OperationCode.PushIndexed, 0);
                            currentState = STATE_OPERATOR_EXPECTED;
                            success = true;
                        }
                        else if (_lastExtractedLexem.Token == Token.Dot)
                        {
                            int identifier;
                            BuildResolveChain(out identifier);
                            ProcessResolvedItem(identifier, stopToken);

                            if (_lastExtractedLexem.Type == LexemType.Operator || _lastExtractedLexem.Token == stopToken)
                            {
                                currentState = STATE_OPERATOR_EXPECTED;
                                continue;
                            }
                            
                        }
                        else if (_lastExtractedLexem.Token != stopToken)
                        {
                            throw CompilerException.TokenExpected(stopToken);
                        }
                        break;
                }

                if (success)
                {
                    if(_lastExtractedLexem.Token != stopToken)
                        NextToken();
                }
                else
                {
                    throw CompilerException.ExpressionSyntax();
                }
            }

            UnwindOperators(ref operators);

        }

        private void UnwindOperators(ref Stack<Token> operators)
        {
            while (operators.Count > 0)
            {
                var oper = operators.Pop();
                if (oper == Token.OpenPar)
                    throw CompilerException.ExpressionSyntax();

                AddCommand(TokenToOperationCode(oper), 0);
            }
        }

        private void BuildPushConstant()
        {
            var cDef = CreateConstDefinition(ref _lastExtractedLexem);
            var num = GetConstNumber(ref cDef);
            AddCommand(OperationCode.PushConst, num);
        }

        private void ProcessExpressionOperator(ref Stack<Token> operators)
        {
            if (operators.Count == 0)
            {
                operators.Push(_lastExtractedLexem.Token);
                return;
            }

            var opOnStack = operators.Peek();
            if (opOnStack != Token.OpenPar)
            {
                var currentPriority = LanguageDef.GetPriority(_lastExtractedLexem.Token);
                var stackPriority = LanguageDef.GetPriority(opOnStack);

                while (stackPriority >= currentPriority && operators.Count > 0)
                {
                    var stackOp = operators.Pop();
                    if (stackOp != Token.OpenPar)
                    {
                        OperationCode opCode = TokenToOperationCode(stackOp);
                        AddCommand(opCode, 0);
                    }
                    else
                    {
                        operators.Push(stackOp);
                        break;
                    }
                }

                operators.Push(_lastExtractedLexem.Token);

            }
            else
            {
                operators.Push(_lastExtractedLexem.Token);
            }
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

        private void BuildPushVariable(VariableBinding varBinding)
        {
            if (varBinding.type == SymbolType.ContextProperty)
            {
                PushReference(varBinding.binding);
            }
            else
            {
                PushSimpleVariable(varBinding.binding);
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

            var parameters = BuiltinFunctions.ParametersInfo(funcId);
            var passedArgs = PushFactArguments();
            CheckFactArguments(parameters, passedArgs);

            AddCommand(funcId, passedArgs.Length);

        }

        #region Helper methods

        private bool IsUserSymbol(ref Lexem lex)
        {
            return lex.Type == LexemType.Identifier && lex.Token == Token.NotAToken;
        }

        private bool IsLiteral(ref Lexem lex)
        {
            return lex.Type == LexemType.StringLiteral
                || lex.Type == LexemType.NumberLiteral
                || lex.Type == LexemType.BooleanLiteral
                || lex.Type == LexemType.DateLiteral
                || lex.Type == LexemType.UndefinedLiteral;
        }

        private bool IsBinaryOperator(Token token)
        {
            return token == Token.Plus
                || token == Token.Minus
                || token == Token.Multiply
                || token == Token.Division
                || token == Token.Modulo
                || token == Token.And
                || token == Token.Or
                || token == Token.Not
                || token == Token.LessThan
                || token == Token.LessOrEqual
                || token == Token.MoreThan
                || token == Token.MoreOrEqual
                || token == Token.Equal
                || token == Token.NotEqual;
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

        private OperationCode BuiltInFunctionCode(Token token)
        {
            switch (token)
            {
                case Token.Question:
                    return OperationCode.Question;
                case Token.Bool:
                    return OperationCode.Bool;
                case Token.Number:
                    return OperationCode.Number;
                case Token.Str:
                    return OperationCode.Str;
                case Token.Date:
                    return OperationCode.Date;
                case Token.StrLen:
                    return OperationCode.StrLen;
                case Token.TrimL:
                    return OperationCode.TrimL;
                case Token.TrimR:
                    return OperationCode.TrimR;
                case Token.TrimLR:
                    return OperationCode.TrimLR;
                case Token.Left:
                    return OperationCode.Left;
                case Token.Right:
                    return OperationCode.Right;
                case Token.Mid:
                    return OperationCode.Mid;
                case Token.StrPos:
                    return OperationCode.StrPos;
                case Token.UCase:
                    return OperationCode.UCase;
                case Token.LCase:
                    return OperationCode.LCase;
                case Token.Chr:
                    return OperationCode.Chr;
                case Token.ChrCode:
                    return OperationCode.ChrCode;
                case Token.EmptyStr:
                    return OperationCode.EmptyStr;
                case Token.StrReplace:
                    return OperationCode.StrReplace;
                case Token.Year:
                    return OperationCode.Year;
                case Token.Month:
                    return OperationCode.Month;
                case Token.Day:
                    return OperationCode.Day;
                case Token.Hour:
                    return OperationCode.Hour;
                case Token.Minute:
                    return OperationCode.Minute;
                case Token.Second:
                    return OperationCode.Second;
                case Token.BegOfYear:
                    return OperationCode.BegOfYear;
                case Token.BegOfMonth:
                    return OperationCode.BegOfMonth;
                case Token.BegOfDay:
                    return OperationCode.BegOfDay;
                case Token.BegOfHour:
                    return OperationCode.BegOfHour;
                case Token.BegOfMinute:
                    return OperationCode.BegOfMinute;
                case Token.BegOfQuarter:
                    return OperationCode.BegOfQuarter;
                case Token.EndOfYear:
                    return OperationCode.EndOfYear;
                case Token.EndOfMonth:
                    return OperationCode.EndOfMonth;
                case Token.EndOfDay:
                    return OperationCode.EndOfDay;
                case Token.EndOfHour:
                    return OperationCode.EndOfHour;
                case Token.EndOfMinute:
                    return OperationCode.EndOfMinute;
                case Token.EndOfQuarter:
                    return OperationCode.EndOfQuarter;
                case Token.WeekOfYear:
                    return OperationCode.WeekOfYear;
                case Token.DayOfYear:
                    return OperationCode.DayOfYear;
                case Token.DayOfWeek:
                    return OperationCode.DayOfWeek;
                case Token.AddMonth:
                    return OperationCode.AddMonth;
                case Token.CurrentDate:
                    return OperationCode.CurrentDate;
                case Token.Integer:
                    return OperationCode.Integer;
                case Token.Round:
                    return OperationCode.Round;
                case Token.Pow:
                    return OperationCode.Pow;
                case Token.Sqrt:
                    return OperationCode.Sqrt;
                case Token.ExceptionInfo:
                    return OperationCode.ExceptionInfo;
                case Token.ExceptionDescr:
                    return OperationCode.ExceptionDescr;
                default:
                    throw new ArgumentException("Token is not a built-in function");
            }
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
            return _tokenStack.Pop();
        }

        private void CheckStructureToken(Token tok)
        {
            if (_tokenStack.Count > 0)
            {
                var toks = PopStructureToken();
                if (!toks.Contains(tok))
                {
                    throw CompilerException.TokenExpected(tok);
                }
            }
            else
            {
                throw CompilerException.UnexpectedOperation();
            }
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
        private void Assert(bool condition)
        {
            System.Diagnostics.Debug.Assert(condition);
        }
    }
}
