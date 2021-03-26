/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using OneScript.Commons;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.StandardLibrary.Collections;
using ScriptEngine.Compiler;
using ScriptEngine.Environment;
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

        private CompiledBlock(ITypeManager tm)
        {
            _typeManager = tm;
        }
        
        [ContextProperty("Параметры", "Parameters")]
        public StructureImpl Parameters { get; set; }

        [ContextProperty("ФрагментКода", "CodeFragment")]
        public string CodeBlock
        {
            get => _codeBlock;
            set
            {
                _codeBlock = value;
                ParseCode();
            }
        }

        private void ParseCode()
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = new SourceCodeIterator(_codeBlock);

            _codeLinesReferences = lexer.Iterator;
            _errors = new ListErrorSink();
            var parser = new DefaultBslParser(lexer, new DefaultAstBuilder(), _errors, new PreprocessorHandlers());

            _ast = parser.ParseCodeBatch();
            if (_errors.HasErrors)
            {
                var prefix = Locale.NStr("ru = 'Ошибка комиляции модуля'; en = 'Module compilation error'");
                var text = string.Join('\n', (new[] {prefix}).Concat(_errors.Errors.Select(x => x.Description)));
                throw new RuntimeException(text);
            }
        }

        public Func<IValue> MakeFunction()
        {
            var expr = ReduceAst(_ast);
            return Expression.Lambda<Func<IValue>>(expr).Compile();
        }

        private Expression ReduceAst(BslSyntaxNode ast)
        {
            // в параметрах лежат соответствия имени переменной и ее типа
            // блок кода надо скомпилировтаь в лямбду с параметрами по количеству в коллекции Parameters и с типами параметров, как там
            // пробежать по аст 1С и превратить в BlockExpression<IValue>

            var symbols = new SymbolResolver();
            FillSymbolContext(symbols);

            var blockBuilder = new BlockExpressionGenerator(symbols, _typeManager, _errors);
            return blockBuilder.CreateExpression(_ast.Children[0] as CodeBatchNode, new ModuleInformation
            {
                Origin = "<compiled code>",
                ModuleName = "<compiled code>",
                CodeIndexer = _codeLinesReferences
            });
        }

        private void FillSymbolContext(SymbolResolver symbols)
        {
            foreach (var kv in Parameters)
            {
                var name = kv.Key.AsString();

                if (kv.Value.SystemType != BasicTypes.Type)
                {
                    throw RuntimeException.InvalidArgumentType($"Parameter {name} is not a Type");
                }

                var typeVal = kv.Value.GetRawValue() as TypeTypeValue;
                var type = ConvertTypeToClrType(typeVal);

                symbols.AddVariable(kv.Key.AsString(), type);
            }
        }

        private static Type ConvertTypeToClrType(TypeTypeValue typeVal)
        {
            var type = typeVal.TypeValue;

            Type clrType;
            if (type == BasicTypes.String)
                clrType = typeof(string);
            else if (type == BasicTypes.Date)
                clrType = typeof(DateTime);
            else if (type == BasicTypes.Boolean)
                clrType = typeof(bool);
            else if (type == BasicTypes.Number)
                clrType = typeof(decimal);
            else if (type == BasicTypes.Type)
                clrType = typeof(TypeTypeValue);
            else
                clrType = type.ImplementingClass;

            return clrType;
        }

        [ScriptConstructor]
        public CompiledBlock Create(TypeActivationContext context)
        {
            return new CompiledBlock(context.TypeManager);
        }
    }
}