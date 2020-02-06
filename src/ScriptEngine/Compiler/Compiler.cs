/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using ScriptEngine.Machine;

namespace ScriptEngine.Compiler
{
    [Flags]
    public enum CodeGenerationFlags
    {
        Always,
        CodeStatistics,
        DebugCode
    }

    class Compiler    {
        public const string BODY_METHOD_NAME = "$entry";

        private const int DUMMY_ADDRESS = -1;
        private static readonly Dictionary<Token, OperationCode> _tokenToOpCode;

        private ILexemGenerator _lexer;
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
        private readonly List<AnnotationDefinition> _annotations = new List<AnnotationDefinition>();

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
                    startPoint = DUMMY_ADDRESS,
                    breakStatements = new List<int>()
                };
            }

            public int startPoint;
            public List<int> breakStatements;
        }

        public CompilerDirectiveHandler DirectiveHandler { get; set; }
    
        public CodeGenerationFlags ProduceExtraCode { get; set; }

        public ModuleImage Compile(ILexemGenerator lexer, ICompilerContext context)
        {
            _module = new ModuleImage();
            _ctx = context;
            _lexer = lexer;
            
            BuildModule();
            CheckForwardedDeclarations();

            _module.LoadAddress = _ctx.TopIndex();
            return _module;
        }

        public ModuleImage CompileExpression(ILexemGenerator lexer, ICompilerContext context)
        {
            _module = new ModuleImage();
            _ctx = context;
            _lexer = lexer;
            NextToken();
            BuildExpressionUpTo(Token.EndOfText);

            _module.LoadAddress = _ctx.TopIndex();
            return _module;
        }

        public ModuleImage CompileExecBatch(ILexemGenerator lexer, ICompilerContext context)
        {
            _module = new ModuleImage();
            _ctx = context;
            _lexer = lexer;
            NextToken();
            PushStructureToken(Token.EndOfText);
            BuildModuleBody();

            _module.LoadAddress = _ctx.TopIndex();
            return _module;
        }

        private AnnotationParameter BuildAnnotationParameter()
        {
            // id | id = value | value
            var result = new AnnotationParameter();
            if (_lastExtractedLexem.Type == LexemType.Identifier)
            {
                result.Name = _lastExtractedLexem.Content;
                NextToken();
                if (_lastExtractedLexem.Token != Token.Equal)
                {
                    result.ValueIndex = AnnotationParameter.UNDEFINED_VALUE_INDEX;
                    return result;
                }
                NextToken();
            }
            
            var cDef = CreateConstDefinition(ref _lastExtractedLexem);
            result.ValueIndex = GetConstNumber(ref cDef);
            
            NextToken();
            
            return result;
        }

        private IList<AnnotationParameter> BuildAnnotationParameters()
        {
            var parameters = new List<AnnotationParameter>();
            while (_lastExtractedLexem.Token != Token.EndOfText)
            {
                parameters.Add(BuildAnnotationParameter());
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextToken();
                    continue;
                }
                if (_lastExtractedLexem.Token == Token.ClosePar)
                {
                    NextToken();
                    break;
                }
                throw CompilerException.UnexpectedOperation();
            }
            return parameters;
        }

        private void BuildAnnotations()
        {
            while (_lastExtractedLexem.Type == LexemType.Annotation)
            {
                var annotation = new AnnotationDefinition() {Name = _lastExtractedLexem.Content};

                NextToken();
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    NextToken();
                    annotation.Parameters = BuildAnnotationParameters().ToArray();
                }
                
                _annotations.Add(annotation);
            }
        }

        private AnnotationDefinition[] ExtractAnnotations()
        {
            var result = _annotations.ToArray();
            _annotations.Clear();
            return result;
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
                if(exc.LineNumber == CodePositionInfo.OUT_OF_TEXT)
                    AppendCodeInfo(_lexer.GetCodePosition(), exc);
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
                        AppendCodeInfo(exc, item.codeLine, CodePositionInfo.OUT_OF_TEXT);
                        throw;
                    }

                    var scope = _ctx.GetScope(methN.ContextIndex);

                    var methInfo = scope.GetMethod(methN.CodeIndex);
                    System.Diagnostics.Debug.Assert(StringComparer.OrdinalIgnoreCase.Compare(methInfo.Name, item.identifier) == 0);
                    if (item.asFunction && !methInfo.IsFunction)
                    {
                        var exc = CompilerException.UseProcAsFunction();
                        AppendCodeInfo(exc, item.codeLine, CodePositionInfo.OUT_OF_TEXT);
                        throw exc;
                    }

                    try
                    {
                        CheckFactArguments(methInfo, item.factArguments);
                    }
                    catch (CompilerException exc)
                    {
                        AppendCodeInfo(exc, item.codeLine, CodePositionInfo.OUT_OF_TEXT);
                        throw;
                    }

                    CorrectCommandArgument(item.commandIndex, GetMethodRefNumber(ref methN));
                }
            }
        }

        private void AppendCodeInfo(CodePositionInfo info, CompilerException exc)
        {
            CompilerException.AppendCodeInfo(exc, info);
        }
        
        private void AppendCodeInfo(CompilerException exc, int line, int column)
        {
            var info = _lexer.GetCodePosition();
            info.LineNumber = line;
            info.ColumnNumber = column;
            info.Code = _lexer.Iterator.GetCodeLine(line);
            
            CompilerException.AppendCodeInfo(exc, info);
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
                else if (_lastExtractedLexem.Type == LexemType.EndOperator)
                {
                    isCodeEntered = true;
                    BuildModuleBody();
                }
                else if (_lastExtractedLexem.Type == LexemType.PreprocessorDirective)
                {
                    HandleDirective(isCodeEntered);
                    UpdateCompositeContext(); // костыль для #330
                }
                else if (_lastExtractedLexem.Type == LexemType.Annotation)
                {
                    BuildAnnotations();
                }
                else
                {
                    throw CompilerException.UnexpectedOperation();
                }
            }
        }

        private void UpdateCompositeContext()
        {
            var modCtx = _ctx as ModuleCompilerContext;
            if (modCtx != null)
                modCtx.Update();
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
                        var annotations = ExtractAnnotations();
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
                            _module.Variables.Add(new VariableInfo()
                            {
                                Identifier = symbolicName,
                                Annotations = annotations,
                                CanGet = true,
                                CanSet = true,
                                Index = definition.CodeIndex
                            });
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
            var entry = _module.Code.Count;
            _ctx.PushScope(new SymbolScope());

            try
            {
                BuildCodeBatch(Token.EndOfText);
            }
            catch
            {
                _ctx.PopScope();
                throw;
            }

            var localCtx = _ctx.PopScope();
            
            var topIdx = _ctx.TopIndex();

            if (entry != _module.Code.Count)
            {
                var bodyMethod = new MethodInfo();
                bodyMethod.Name = BODY_METHOD_NAME;
                var descriptor = new MethodDescriptor();
                descriptor.EntryPoint = entry;
                descriptor.Signature = bodyMethod;
                FillVariablesFrame(ref descriptor, localCtx);

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

        private static void FillVariablesFrame(ref MethodDescriptor descriptor, SymbolScope localCtx)
        {
            descriptor.Variables = new VariablesFrame();

            for (int i = 0; i < localCtx.VariableCount; i++)
            {
                descriptor.Variables.Add(localCtx.GetVariable(i));
            }
        }

        private void HandleDirective(bool codeEntered)
        {
            var directive = _lastExtractedLexem.Content;
            
            ReadToLineEnd();

            var value = _lexer.Iterator.GetContents().Trim();
            NextToken();

            if (DirectiveHandler == null || !DirectiveHandler(directive, value, codeEntered))
                throw new CompilerException(String.Format("Неизвестная директива: {0}({1})", directive, value));
        }

        private void ReadToLineEnd()
        {
            char cs;
            do
            {
                cs = _lexer.Iterator.CurrentSymbol;
            } while (cs != '\n' && _lexer.Iterator.MoveNext());
        }

        private void BuildSingleMethod()
        {
            var entryPoint = _module.Code.Count;
            AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics);

            if (_lastExtractedLexem.Token == Token.Procedure)
            {
                _isFunctionProcessed = false;
            }
            else if (_lastExtractedLexem.Token == Token.Function)
            {
                _isFunctionProcessed = true;
            }
            else
            {
                throw CompilerException.UnexpectedOperation();
            }
            NextToken();

            #region Method signature
            // сигнатура
            if (!IsUserSymbol(ref _lastExtractedLexem))
            {
                throw CompilerException.IdentifierExpected();
            }

            int definitionLine = _lexer.CurrentLine;
            int definitionColumn = _lexer.CurrentColumn;
            MethodInfo method = new MethodInfo();
            method.Name = _lastExtractedLexem.Content;
            method.IsFunction = _isFunctionProcessed;
            method.Annotations = ExtractAnnotations();

            NextToken();
            if (_lastExtractedLexem.Token != Token.OpenPar)
            {
                throw CompilerException.TokenExpected(Token.OpenPar);
            } 
            #endregion
            
            #region Parameters list
            var paramsList = new List<ParameterDefinition>();
            var methodCtx = new SymbolScope();

            BuildMethodParametersList(paramsList, methodCtx);

            method.Params = paramsList.ToArray();
 
            #endregion
            
            bool isExportedMethod = false;
            if (_lastExtractedLexem.Token == Token.Export)
            {
                isExportedMethod = true;
                NextToken();
            }

            method.IsExport = isExportedMethod;

            #region Body
            // тело

            try
            {
                _ctx.PushScope(methodCtx);
                DispatchMethodBody();
            }
            finally
            {
                _ctx.PopScope();
            }
            
            var descriptor = new MethodDescriptor();
            descriptor.EntryPoint = entryPoint;
            descriptor.Signature = method;
            FillVariablesFrame(ref descriptor, methodCtx);

            SymbolBinding binding;
            try
            {
                binding = _ctx.DefineMethod(method);
            }
            catch (CompilerException)
            {
                var exc = new CompilerException("Метод с таким именем уже определен: " + method.Name);
                AppendCodeInfo(exc, definitionLine, definitionColumn);
                throw exc;
            }
            _module.MethodRefs.Add(binding);
            _module.Methods.Add(descriptor);

            // TODO: deprecate?
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

        private void BuildMethodParametersList(List<ParameterDefinition> paramsList, SymbolScope methodCtx)
        {
            NextToken(); // (
            while (_lastExtractedLexem.Token != Token.ClosePar)
            {
                // [Знач] Идентификатор[= Литерал][,[Знач] Идентификатор[= Литерал]]
                var param = BuildParameterDefinition();

                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    paramsList.Add(param);
                    methodCtx.DefineVariable(param.Name);
                    NextToken();

                    if (_lastExtractedLexem.Token == Token.ClosePar) // сразу после запятой не можем выйти из цикла, т.к. ждем в этом месте параметр
                    {
                        throw CompilerException.IdentifierExpected();
                    }

                    continue;
                }
                
                paramsList.Add(param);
                methodCtx.DefineVariable(param.Name);
            }
            
            NextToken(); // )
        }

        private ParameterDefinition BuildParameterDefinition()
        {
            var param = new ParameterDefinition();
            
            if (_lastExtractedLexem.Type == LexemType.Annotation)
            {
                BuildAnnotations();
                param.Annotations = ExtractAnnotations();
            }
            
            if (_lastExtractedLexem.Token == Token.ByValParam)
            {
                param.IsByValue = true;
                NextToken();
                if (IsUserSymbol(ref _lastExtractedLexem))
                {
                    param.Name = _lastExtractedLexem.Content;
                }
                else
                {
                    throw CompilerException.IdentifierExpected();
                }
            }
            else if (IsUserSymbol(ref _lastExtractedLexem))
            {
                param.IsByValue = false;
                param.Name = _lastExtractedLexem.Content;
            }
            else
            {
                throw CompilerException.IdentifierExpected();
            }

            NextToken();
            if (_lastExtractedLexem.Token == Token.Equal)
            {
                param.HasDefaultValue = true;
                param.DefaultValueIndex = BuildDefaultParameterValue();
            }

            return param;
        }

        private int BuildDefaultParameterValue()
        {
            NextToken();

            bool hasSign = false;
            bool signIsMinus = _lastExtractedLexem.Token == Token.Minus;
            if (signIsMinus || _lastExtractedLexem.Token == Token.Plus)
            {
                hasSign = true;
                NextToken();
            }

            if (IsLiteral(ref _lastExtractedLexem))
            {
                var cd = CreateConstDefinition(ref _lastExtractedLexem);
                if (hasSign)
                {
                    if (_lastExtractedLexem.Type == LexemType.NumberLiteral && signIsMinus)
                    {
                        cd.Presentation = '-' + cd.Presentation;
                    }
                    else if (_lastExtractedLexem.Type == LexemType.StringLiteral
                          || _lastExtractedLexem.Type == LexemType.DateLiteral)
                    {
                        throw CompilerException.NumberExpected();
                    }
                }

                NextToken();
                return GetConstNumber(ref cd);
            }
            else
            {
                throw CompilerException.LiteralExpected();
            }
        }

        private void DispatchMethodBody()
        {
            _inMethodScope = true;
            BuildVariableDefinitions();
            _isStatementsDefined = true;

            var codeStart = _module.Code.Count;

            BuildCodeBatch(_isFunctionProcessed? Token.EndFunction : Token.EndProcedure);

            if (_isFunctionProcessed)
            {
                var undefConst = new ConstDefinition()
                {
                    Type = DataType.Undefined,
                    Presentation = "Неопределено"
                };

                AddCommand(OperationCode.PushConst, GetConstNumber(ref undefConst));
            }

            var codeEnd = _module.Code.Count;

            if (_lastExtractedLexem.Token == Token.EndProcedure
                || _lastExtractedLexem.Token == Token.EndFunction)
            {
                AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics|CodeGenerationFlags.DebugCode);
            }

            AddCommand(OperationCode.Return);

            {
                // заменим Return на Jmp <сюда>
                for (var i = codeStart; i < codeEnd; i++)
                {
                    if (_module.Code[i].Code == OperationCode.Return)
                    {
                        _module.Code[i] = new Command() { Code = OperationCode.Jmp, Argument = codeEnd };
                    }
                }
            }

            _isStatementsDefined = false;
            _inMethodScope = false;
        }

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

                if (_lastExtractedLexem.Token != Token.Semicolon)
                {
                    if (endTokens.Contains(_lastExtractedLexem.Token) || LanguageDef.IsEndOfBlockToken(_lastExtractedLexem.Token))
                    {
                        break;
                    }
                    throw CompilerException.SemicolonExpected();
                }
                NextToken();
            }
            PopStructureToken();
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
                    var expected = PopStructureToken();
                    throw CompilerException.TokenExpected(expected);
            }
        }

        private void BuildIfStatement()
        {
            AddLineNumber(_lexer.CurrentLine);

            var exitIndices = new List<int>();
            NextToken();
            BuildExpressionUpTo(Token.Then);
            var jumpFalseIndex = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);

            NextToken();
            BuildCodeBatch(Token.Else, Token.ElseIf, Token.EndIf);
            exitIndices.Add(AddCommand(OperationCode.Jmp, DUMMY_ADDRESS));

            bool hasAlternativeBranches = false;

            while (_lastExtractedLexem.Token == Token.ElseIf)
            {
                CorrectCommandArgument(jumpFalseIndex, _module.Code.Count);
                AddLineNumber(_lastExtractedLexem.LineNumber);

                NextToken();
                BuildExpressionUpTo(Token.Then);
                jumpFalseIndex = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);

                NextToken();
                BuildCodeBatch(Token.Else, Token.ElseIf, Token.EndIf);
                exitIndices.Add(AddCommand(OperationCode.Jmp, DUMMY_ADDRESS));
            }

            if (_lastExtractedLexem.Token == Token.Else)
            {
                hasAlternativeBranches = true;
                CorrectCommandArgument(jumpFalseIndex, _module.Code.Count);
                AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics);

                NextToken();
                BuildCodeBatch(Token.EndIf);
            }

            int exitIndex = AddLineNumber(_lastExtractedLexem.LineNumber);

            if (!hasAlternativeBranches)
            {
                CorrectCommandArgument(jumpFalseIndex, exitIndex);
            }

            foreach (var indexToWrite in exitIndices)
            {
                CorrectCommandArgument(indexToWrite, exitIndex);
            }
            NextToken();
        }

        private void BuildForStatement()
        {
            AddLineNumber(_lexer.CurrentLine);

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
            BuildExpressionUpTo(Token.Loop);
            AddCommand(OperationCode.PushIterator);
            var loopBegin = AddLineNumber(_lastExtractedLexem.LineNumber);
            AddCommand(OperationCode.IteratorNext);
            var condition = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);
            BuildLoadVariable(identifier);

            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = loopBegin;
            _nestedLoops.Push(loopRecord);

            NextToken();
            bool savedTryFlag = SetTryBlockFlag(false);
            BuildCodeBatch(Token.EndLoop);
            SetTryBlockFlag(savedTryFlag);

            if (_lastExtractedLexem.Token == Token.EndLoop)
            {
                AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics | CodeGenerationFlags.DebugCode);
            }

            AddCommand(OperationCode.Jmp, loopBegin);

            var indexLoopEnd = AddCommand(OperationCode.StopIterator);
            CorrectCommandArgument(condition, indexLoopEnd);
            CorrectBreakStatements(_nestedLoops.Pop(), indexLoopEnd);
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
            BuildExpressionUpTo(Token.To);
            BuildLoadVariable(counter);
            NextToken();
            BuildExpressionUpTo(Token.Loop);
            AddCommand(OperationCode.MakeRawValue);
            AddCommand(OperationCode.PushTmp);

            var jmpIndex = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS);
            var indexLoopBegin = AddLineNumber(_lastExtractedLexem.LineNumber);

            // increment
            BuildPushVariable(counter);
            AddCommand(OperationCode.Inc);
            BuildLoadVariable(counter);

            var counterIndex = BuildPushVariable(counter);
            CorrectCommandArgument(jmpIndex, counterIndex);
            var conditionIndex = AddCommand(OperationCode.JmpCounter, DUMMY_ADDRESS);

            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = indexLoopBegin;
            _nestedLoops.Push(loopRecord);

            NextToken();
            bool savedTryFlag = SetTryBlockFlag(false);
            BuildCodeBatch(Token.EndLoop);
            SetTryBlockFlag(savedTryFlag);

            if (_lastExtractedLexem.Token == Token.EndLoop)
            {
                AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics | CodeGenerationFlags.DebugCode);
            }

            // jmp to start
            AddCommand(OperationCode.Jmp, indexLoopBegin);

            var indexLoopEnd = AddCommand(OperationCode.PopTmp, 1);
            CorrectCommandArgument(conditionIndex, indexLoopEnd);
            CorrectBreakStatements(_nestedLoops.Pop(), indexLoopEnd);
            NextToken();
        }

        private void BuildWhileStatement()
        {
            AddLineNumber(_lexer.CurrentLine);

            NextToken();
            var conditionIndex = _module.Code.Count;
            var loopRecord = NestedLoopInfo.New();
            loopRecord.startPoint = conditionIndex;
            _nestedLoops.Push(loopRecord);
            BuildExpressionUpTo(Token.Loop);

            var jumpFalseIndex = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);
            NextToken();
            bool savedTryFlag = SetTryBlockFlag(false);
            BuildCodeBatch(Token.EndLoop);
            SetTryBlockFlag(savedTryFlag);

            if (_lastExtractedLexem.Token == Token.EndLoop)
            {
                AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics | CodeGenerationFlags.DebugCode);
            }

            AddCommand(OperationCode.Jmp, conditionIndex);

            var endLoop = AddCommand(OperationCode.Nop);
            CorrectCommandArgument(jumpFalseIndex, endLoop);
            CorrectBreakStatements(_nestedLoops.Pop(), endLoop);

            NextToken();
        }

        private void BuildBreakStatement()
        {
            if (_nestedLoops.Count == 0)
            {
                throw CompilerException.BreakOutsideOfLoop();
            }
            AddLineNumber(_lexer.CurrentLine);

            var loopInfo = _nestedLoops.Peek();
            if(_isInTryBlock)
                AddCommand(OperationCode.EndTry);
            var idx = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS);
            loopInfo.breakStatements.Add(idx);
            NextToken();
        }

        private void BuildContinueStatement()
        {
            if (_nestedLoops.Count == 0)
            {
                throw CompilerException.ContinueOutsideOfLoop();
            }

            AddLineNumber(_lexer.CurrentLine);

            var loopInfo = _nestedLoops.Peek();
            if(_isInTryBlock)
                AddCommand(OperationCode.EndTry);
            AddCommand(OperationCode.Jmp, loopInfo.startPoint);
            NextToken();
        }

        private void BuildReturnStatement()
        {
            AddLineNumber(_lexer.CurrentLine);

            if (_isFunctionProcessed)
            {
                NextToken();
                if (_lastExtractedLexem.Token == Token.Semicolon
                    || LanguageDef.IsEndOfBlockToken(_lastExtractedLexem.Token))
                {
                    throw CompilerException.FuncEmptyReturnValue();
                }
                BuildExpression(Token.Semicolon);
                AddCommand(OperationCode.MakeRawValue);
            }
            else if (_inMethodScope)
            {
                NextToken();
                if (_lastExtractedLexem.Token != Token.Semicolon
                    && !LanguageDef.IsEndOfBlockToken(_lastExtractedLexem.Token))
                {
                    throw CompilerException.ProcReturnsAValue();
                }
            }
            else
            {
                throw CompilerException.ReturnOutsideOfMethod();
            }

            AddCommand(OperationCode.Return);
        }

        private void BuildTryExceptStatement()
        {
            AddLineNumber(_lexer.CurrentLine, CodeGenerationFlags.CodeStatistics);

            var beginTryIndex = AddCommand(OperationCode.BeginTry, DUMMY_ADDRESS);
            bool savedTryFlag = SetTryBlockFlag(true);
            NextToken();
            BuildCodeBatch(Token.Exception);
            SetTryBlockFlag(savedTryFlag);
            var jmpIndex = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS);

            Assert(_lastExtractedLexem.Token == Token.Exception);
            
            var beginHandler = AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics);

            CorrectCommandArgument(beginTryIndex, beginHandler);

            NextToken();
            BuildCodeBatch(Token.EndTry);

            var endIndex = AddLineNumber(_lastExtractedLexem.LineNumber, CodeGenerationFlags.CodeStatistics | CodeGenerationFlags.DebugCode);
            AddCommand(OperationCode.EndTry);
            CorrectCommandArgument(jmpIndex, endIndex);
            
            NextToken();
        }

        private void BuildRaiseExceptionStatement()
        {
            AddLineNumber(_lexer.CurrentLine);

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
                AddCommand(OperationCode.RaiseException);
            }
        }

        private void BuildExecuteStatement()
        {
            AddLineNumber(_lexer.CurrentLine);
            NextToken();

            BuildExpression(Token.Semicolon);
            AddCommand(OperationCode.Execute);
        }

        private void BuildEventHandlerOperation(Token token)
        {
            var identifier = BuildContinuationLeftHand();
            // BuildContinuationInternal(ContinuationBuildMode.EventHandler, eventName =>
            // {
            //     if (eventName == null)
            //     {
            //         throw CompilerException.IdentifierExpected();
            //     }
            //     BuildPushConstant();
            // });
            //
            // NextToken();
            // if (_lastExtractedLexem.Token != Token.Comma)
            // {
            //     throw CompilerException.TokenExpected(Token.Comma);
            // }
            // NextToken();
            //
            // BuildContinuationInternal(ContinuationBuildMode.EventHandler, handlerName =>
            // {
            //     if (handlerName == null)
            //     {
            //         throw CompilerException.IdentifierExpected();
            //     }
            //     BuildPushConstant();
            // });
            //
            // var opCode = token == Token.AddHandler ? OperationCode.AddHandler : OperationCode.RemoveHandler;
            // AddCommand(opCode, 0);
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
            AddLineNumber(_lexer.CurrentLine);
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
                    var expectAssignment = BuildContinuationLeftHand();
                    if (expectAssignment)
                    {
                        BuildAssignment();
                    }
                    
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
                BuildLocalMethodCall(identifier, args, true);
                BuildContinuationLeftHand();
            }
            else
            {
                BuildLocalMethodCall(identifier, args, false);
            }
        }

        private void BuildAssignment()
        {
            if (_lastExtractedLexem.Token != Token.Equal)
                throw CompilerException.UnexpectedOperation();

            NextToken(); // перешли к выражению
            BuildExpression(Token.Semicolon);
            AddCommand(OperationCode.AssignRef);
        }

        private bool IsContinuationToken(ref Lexem lex)
        {
            return lex.Token == Token.Dot || lex.Token == Token.OpenBracket;
        }

        private void BuildExpressionUpTo(Token stopToken)
        {
            BuildPrimaryNode();
            BuildOperation(0);
            if (_lastExtractedLexem.Token == stopToken)
                return;

            if (_lastExtractedLexem.Token == Token.EndOfText)
                throw CompilerException.UnexpectedEndOfText();
            else
                throw CompilerException.ExpressionSyntax();
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

            if (_lastExtractedLexem.Token == Token.EndOfText)
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
                bool isLogical = LanguageDef.IsLogicalBinaryOperator(currentOp);
                int logicalCmdIndex = DUMMY_ADDRESS;

                if (isLogical)
                {
                    logicalCmdIndex = AddCommand(TokenToOperationCode(currentOp));
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

                    AddCommand(OperationCode.MakeBool);
                    CorrectCommandArgument(logicalCmdIndex, _module.Code.Count);
                }
                else
                {
                    var opCode = TokenToOperationCode(currentOp);
                    AddCommand(opCode);
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
                case Token.UnaryPlus:
                    opCode = OperationCode.Number;
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
            else if (_lastExtractedLexem.Token == Token.Plus)
            {
                ProcessPrimaryUnaryPlus();
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
                BuildContinuationRightHand();
            }
            else
            {
                throw CompilerException.ExpressionSyntax();
            }
        }
        private void ProcessPrimaryIdentifier()
        {
            var identifier = _lastExtractedLexem.Content;
            var lineNumber = _lastExtractedLexem.LineNumber;
            NextToken();
            if (IsContinuationToken(ref _lastExtractedLexem))
            {
                BuildPushVariable(identifier);
                BuildContinuationRightHand();
            }
            else if (_lastExtractedLexem.Token == Token.OpenPar)
            {
                BuildFunctionCall(identifier, lineNumber);
                BuildContinuationRightHand();
            }
            else
            {
                BuildPushVariable(identifier);
            }
        }

        private bool LastExtractedIsPimary()
        {
            return LanguageDef.IsLiteral(ref _lastExtractedLexem)
                || LanguageDef.IsIdentifier(ref _lastExtractedLexem)
                || _lastExtractedLexem.Token == Token.OpenPar;
        }

        private void ProcessPrimaryUnaryMinus()
        {
            NextToken();
            if (!LastExtractedIsPimary())
            {
                throw CompilerException.ExpressionExpected();
            }

            BuildPrimaryNode();
            AddCommand(OperationCode.Neg);
        }

        private void ProcessPrimaryUnaryPlus()
        {
            NextToken();
            if (!LastExtractedIsPimary())
            {
                throw CompilerException.ExpressionExpected();
            }

            BuildPrimaryNode();
            AddCommand(OperationCode.Number);
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
            AddCommand(OperationCode.Not);
        }

        private void BuildContinuationRightHand()
        {
            var memberName = BuildIndexOrGetMemberIdentifier();
            if (memberName != null)
            {
                NextToken();
                if (_lastExtractedLexem.Token == Token.OpenPar)
                {
                    ResolveFunction(memberName);
                }
                else
                {
                    ResolveProperty(memberName);
                }
            }

            if(IsContinuationToken(ref _lastExtractedLexem))
                BuildContinuationRightHand();
        }

        private void ResolveFunction(string memberName)
        {
            PushMethodArgumentsBeforeCall();

            var cDef = new ConstDefinition
            {
                Type = DataType.String,
                Presentation = memberName
            };

            AddCommand(OperationCode.ResolveMethodFunc, GetConstNumber(ref cDef));
        }

        private string BuildIndexOrGetMemberIdentifier()
        {
            if (_lastExtractedLexem.Token == Token.Dot)
            {
                NextToken();
                if (!IsValidPropertyName(ref _lastExtractedLexem))
                    throw CompilerException.IdentifierExpected();

                return _lastExtractedLexem.Content;
            }
            
            if (_lastExtractedLexem.Token == Token.OpenBracket)
            {
                BuildIndexedAccess();
            }

            return null;
        }

        private bool BuildContinuationLeftHand()
        {
            var expectAssignment = false;
            while (true)
            {
                var memberName = BuildIndexOrGetMemberIdentifier();
                if (memberName != null)
                {
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.OpenPar)
                    {
                        PushMethodArgumentsBeforeCall();

                        var cDef = new ConstDefinition {Type = DataType.String, Presentation = memberName};

                        if (IsContinuationToken(ref _lastExtractedLexem))
                        {
                            AddCommand(OperationCode.ResolveMethodFunc, GetConstNumber(ref cDef));
                            continue; // чтобы не проверять 2 раза условие IsContinuationToken внизу
                        }
                        else
                        {
                            AddCommand(OperationCode.ResolveMethodProc, GetConstNumber(ref cDef));
                            expectAssignment = false;
                            break;
                        }
                    }
                    else
                    {
                        ResolveProperty(memberName);
                        expectAssignment = true;
                    }
                }
                else
                {
                    expectAssignment = true;
                }

                if (!IsContinuationToken(ref _lastExtractedLexem))
                    break;
            }

            return expectAssignment;
        }

        private void BuildIndexedAccess()
        {
            NextToken();
            if (_lastExtractedLexem.Token == Token.CloseBracket)
                throw CompilerException.ExpressionExpected();

            BuildExpressionUpTo(Token.CloseBracket);
            System.Diagnostics.Debug.Assert(_lastExtractedLexem.Token == Token.CloseBracket);
            NextToken();

            AddCommand(OperationCode.PushIndexed);
        }
        
        private string FindLastIdentifierOnChain()
        {
            string lastIdentifier = null;
            while (true)
            {
                if (_lastExtractedLexem.Token == Token.Dot)
                {
                    NextToken();
                    if (!IsValidPropertyName(ref _lastExtractedLexem))
                        throw CompilerException.IdentifierExpected();

                    lastIdentifier = _lastExtractedLexem.Content;
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.OpenPar)
                    {
                        PushMethodArgumentsBeforeCall();
                        
                        var cDef = new ConstDefinition();
                        cDef.Type = DataType.String;
                        cDef.Presentation = lastIdentifier;
                        int lastIdentifierConst = GetConstNumber(ref cDef);
                        
                        NextToken();
                        if (IsContinuationToken(ref _lastExtractedLexem))
                        {
                            AddCommand(OperationCode.ResolveMethodFunc, lastIdentifierConst);
                        }
                        else
                        {
                            AddCommand(OperationCode.ResolveMethodProc, lastIdentifierConst);
                            lastIdentifier = null;
                            break;
                        }
                    }
                    else
                    {
                       ResolveProperty(lastIdentifier);
                    }
                }
                else if (_lastExtractedLexem.Token == Token.OpenBracket)
                {
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.CloseBracket)
                        throw CompilerException.ExpressionExpected();

                    BuildExpressionUpTo(Token.CloseBracket);
                    System.Diagnostics.Debug.Assert(_lastExtractedLexem.Token == Token.CloseBracket);
                    NextToken();

                    AddCommand(OperationCode.PushIndexed);
                }
                else
                {
                    break;
                }
            }

            return lastIdentifier;
        }

        private bool IsValidPropertyName(ref Lexem lex)
        {
            return LanguageDef.IsIdentifier(ref lex) 
                || lex.Type == LexemType.BooleanLiteral
                || lex.Type == LexemType.NullLiteral
                || lex.Type == LexemType.UndefinedLiteral
                || lex.Token == Token.And
                || lex.Token == Token.Or
                || lex.Token == Token.Not;
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
            BuildExpressionUpTo(Token.Comma);
            
            AddCommand(OperationCode.MakeBool);
            var addrOfCondition = AddCommand(OperationCode.JmpFalse, DUMMY_ADDRESS);

            NextToken();
            BuildExpressionUpTo(Token.Comma); // построили true-part

            var endOfTruePart = AddCommand(OperationCode.Jmp, DUMMY_ADDRESS); // уход в конец оператора
            
            CorrectCommandArgument(addrOfCondition, _module.Code.Count); // отметили, куда переходить по false
            NextToken();
            BuildExpressionUpTo(Token.ClosePar); // построили false-part
            
            CorrectCommandArgument(endOfTruePart, _module.Code.Count);
            
            NextToken();
        }

        private bool[] BuildArgumentList()
        {
            System.Diagnostics.Debug.Assert(_lastExtractedLexem.Token == Token.OpenPar);

            List<bool> arguments = new List<bool>();

            PushStructureToken(Token.ClosePar);
            try
            {
                NextToken(); // съели открывающую скобку
                while (_lastExtractedLexem.Token != Token.ClosePar)
                {
                    PushPassedArgument(arguments);
                }

                if (_lastExtractedLexem.Token != Token.ClosePar)
                    throw CompilerException.TokenExpected(")");

                NextToken(); // съели закрывающую скобку
            }
            finally
            {
                PopStructureToken();
            }
            
            return arguments.ToArray();
        }

        private void PushPassedArgument(IList<bool> arguments)
        {
            if (_lastExtractedLexem.Token == Token.Comma)
            {
                AddCommand(OperationCode.PushDefaultArg);
                arguments.Add(false);
                NextToken();
                if (_lastExtractedLexem.Token == Token.ClosePar)
                {
                    AddCommand(OperationCode.PushDefaultArg);
                    arguments.Add(false);
                }
            }
            else if (_lastExtractedLexem.Token != Token.ClosePar)
            {
                BuildExpression(Token.Comma);
                arguments.Add(true);
                if (_lastExtractedLexem.Token == Token.Comma)
                {
                    NextToken();
                    if (_lastExtractedLexem.Token == Token.ClosePar)
                    {
                        AddCommand(OperationCode.PushDefaultArg);
                        arguments.Add(false);
                    }
                }
            }
        }

        private void BuildLoadVariable(string identifier)
        {
            var hasVar = _ctx.TryGetVariable(identifier, out var varBinding);
            if (hasVar)
            {
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
            else
            {
                // can create variable
                var binding = _ctx.DefineVariable(identifier);
                AddCommand(OperationCode.LoadLoc, binding.CodeIndex);
            }
        }
        
        private void BuildFunctionCall(string identifier, int callLineNumber)
        {
            bool[] args = PushMethodArgumentsBeforeCall();
            AddLineNumber(callLineNumber, CodeGenerationFlags.CodeStatistics);
            BuildLocalMethodCall(identifier, args, true);
            AddLineNumber(callLineNumber, CodeGenerationFlags.DebugCode);
        }

        private bool[] PushMethodArgumentsBeforeCall()
        {
            var argsPassed = BuildArgumentList();
            AddCommand(OperationCode.ArgNum, argsPassed.Length);
            return argsPassed;
        }

        private void BuildLocalMethodCall(string identifier, bool[] argsPassed, bool asFunction)
        {
            var hasMethod = _ctx.TryGetMethod(identifier, out var methBinding);
            if (hasMethod)
            {
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

                if (asFunction)
                    AddCommand(OperationCode.CallFunc, GetMethodRefNumber(ref methBinding));
                else
                    AddCommand(OperationCode.CallProc, GetMethodRefNumber(ref methBinding)); 
            }
            else
            {
                // can be defined later
                var forwarded = new ForwardedMethodDecl();
                forwarded.identifier = identifier;
                forwarded.asFunction = asFunction;
                forwarded.codeLine = _lexer.CurrentLine;
                forwarded.factArguments = argsPassed;

                var opCode = asFunction ? OperationCode.CallFunc : OperationCode.CallProc;
                forwarded.commandIndex = AddCommand(opCode, DUMMY_ADDRESS);
                _forwardedMethods.Add(forwarded);
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

            if (parameters.Skip(argsPassed.Length).Any(param => !param.HasDefaultValue))
            {
                throw CompilerException.TooFewArgumentsPassed();
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
                    throw CompilerException.TooFewArgumentsPassed();
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
                _lastExtractedLexem = _lexer.NextLexem();
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

        private int AddCommand(OperationCode code, int arg = 0)
        {
            var addr = _module.Code.Count;
            _module.Code.Add(new Command() { Code = code, Argument = arg });
            return addr;
        }

        private int AddLineNumber(int linenum, CodeGenerationFlags emitConditions = CodeGenerationFlags.Always)
        {
            var addr = _module.Code.Count;
            bool emit = emitConditions == CodeGenerationFlags.Always || ExtraCodeConditionsMet(emitConditions);
            if (emit)
            {
                _module.Code.Add(new Command() { Code = OperationCode.LineNum, Argument = linenum });
            }
            return addr;
        }

        private bool ExtraCodeConditionsMet(CodeGenerationFlags emitConditions)
        {
            return (((int)ProduceExtraCode) & (int)emitConditions) != 0;
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
