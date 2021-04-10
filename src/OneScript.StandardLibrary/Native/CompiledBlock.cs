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
using System.Linq.Expressions;
using System.Text;
using OneScript.Commons;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Native.Compiler;
using OneScript.Native.Runtime;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Values;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Native
{
    [ContextClass("СкомпилированныйФрагмент", "CompiledCodeBlock")]
    public class CompiledBlock
    {
        private string _codeBlock;
        private BslSyntaxNode _ast;
        private ITypeManager _typeManager;
        private IErrorSink _errors;
        private ISourceCodeIndexer _codeLinesReferences;

        public CompiledBlock(ITypeManager tm)
        {
            _typeManager = tm;
        }

        public OneScript.Native.Compiler.SymbolTable Symbols { get; set; }
        
        [ContextProperty("Параметры", "Parameters")]
        public StructureImpl Parameters { get; set; } = new StructureImpl();
        
        [ContextProperty("ФрагментКода", "CodeFragment")]
        public string CodeBlock
        {
            get => _codeBlock ?? string.Empty;
            set
            {
                _codeBlock = value;
                ParseCode();
            }
        }

        private void ParseCode()
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = new SourceCodeIterator(CodeBlock);

            _codeLinesReferences = lexer.Iterator;
            _errors = new ListErrorSink();
            var parser = new DefaultBslParser(lexer, new DefaultAstBuilder(), _errors, new PreprocessorHandlers());

            _ast = parser.ParseCodeBatch(true);
            if (_errors.HasErrors)
            {
                var prefix = Locale.NStr("ru = 'Ошибка комиляции модуля'; en = 'Module compilation error'");
                var text = string.Join('\n', (new[] {prefix}).Concat(_errors.Errors.Select(x => x.Description)));
                throw new RuntimeException(text);
            }
        }

        public Func<BslValue[], BslValue> CreateDelegate()
        {
            var l = MakeExpression();

            var arrayOfValuesParam = Expression.Parameter(typeof(BslValue[]));
            var convertedAccessList = new List<Expression>();

            int index = 0;
            foreach (var parameter in Parameters)
            {
                var targetType = parameter.Value as TypeTypeValue;
                var arrayAccess = Expression.ArrayIndex(arrayOfValuesParam, Expression.Constant(index));
                var convertedParam = ExpressionHelpers.ConvertToType(arrayAccess, ConvertTypeToClrType(targetType));
                convertedAccessList.Add(convertedParam);
                ++index;
            }
            
            var lambdaInvocation = Expression.Invoke(l, convertedAccessList);
            var func = Expression.Lambda<Func<BslValue[], BslValue>>(lambdaInvocation, arrayOfValuesParam);

            return func.Compile();
        }

        public T CreateDelegate<T>() where T:class
        {
            var l = MakeExpression();
            var call = Expression.Invoke(l, l.Parameters);
            var func = Expression.Lambda<T>(call, l.Parameters);

            return func.Compile();
        }

        public LambdaExpression MakeExpression()
        {
            if(_ast == default)
                ParseCode();

            var expression = ReduceAst(_ast);
            
            if (_errors.HasErrors)
            {
                var prefix = Locale.NStr("ru = 'Ошибка комиляции модуля'; en = 'Module compilation error'");
                var sb = new StringBuilder();
                sb.AppendLine(prefix);
                foreach (var error in _errors.Errors)
                {
                    sb.AppendLine($"{error.Description.TrimEnd()} ({error.Position.LineNumber})");
                }

                throw new RuntimeException(sb.ToString());
            }

            

            return expression;
        }

        private LambdaExpression ReduceAst(BslSyntaxNode ast)
        {
            // в параметрах лежат соответствия имени переменной и ее типа
            // блок кода надо скомпилировтаь в лямбду с параметрами по количеству в коллекции Parameters и с типами параметров, как там
            // пробежать по аст 1С и превратить в BlockExpression<IValue>

            if (Symbols == null)
                Symbols = new OneScript.Native.Compiler.SymbolTable();
            
            var methodInfo = CreateMethodInfo();
            var moduleInfo = new ModuleInformation
            {
                Origin = "<compiled code>",
                ModuleName = "<compiled code>",
                CodeIndexer = _codeLinesReferences
            };
            var methodCompiler = new MethodCompiler(new BslWalkerContext
            {
                Errors = _errors,
                Module = moduleInfo,
                Symbols = Symbols
            }, methodInfo);
            
            methodCompiler.CompileModuleBody(methodInfo, _ast);
            return methodInfo.Implementation;
        }

        private BslMethodInfo CreateMethodInfo()
        {
            var methodInfo = new BslMethodInfo();
            methodInfo.SetName("$__compiled");
            methodInfo.SetReturnType(typeof(IValue));

            foreach (var parameter in Parameters)
            {
                var targetType = parameter.Value as TypeTypeValue;
                var pi = new BslParameterInfo(
                    parameter.Key.AsString(),
                    ConvertTypeToClrType(targetType));

                methodInfo.Parameters.Add(pi);
            }

            return methodInfo;
        }

        private static Type ConvertTypeToClrType(TypeTypeValue typeVal)
        {
            var type = typeVal.TypeValue;
            return ExpressionHelpers_.GetClrType(type);
        }

        [ScriptConstructor]
        public static CompiledBlock Create(TypeActivationContext context)
        {
            return new CompiledBlock(context.TypeManager);
        }
    }
}