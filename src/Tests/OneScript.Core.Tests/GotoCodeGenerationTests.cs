/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using OneScript.Compilation.Binding;
using OneScript.Execution;
using OneScript.Language;
using OneScript.Language.LexicalAnalysis;
using OneScript.Language.SyntaxAnalysis;
using OneScript.Language.SyntaxAnalysis.AstNodes;
using OneScript.Sources;
using ScriptEngine;
using ScriptEngine.Compiler;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class GotoCodeGenerationTests
    {
        private static StackRuntimeModule BuildModule(string code)
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = SourceCodeBuilder.Create().FromString(code).Build().CreateIterator();
            var errSink = new ThrowingErrorSink();
            var parser = new DefaultBslParser(
                lexer,
                errSink,
                Mock.Of<PreprocessorHandlers>());

            var node = parser.ParseStatefulModule() as ModuleNode;
            var ctx = new SymbolTable();
            ctx.PushScope(new SymbolScope(), ScopeBindingDescriptor.Static(null));
            var compiler = new StackMachineCodeGenerator(errSink, ExplicitImportsBehavior.Disabled);
            return compiler.CreateModule(node, lexer.Iterator.Source, ctx, Mock.Of<IBslProcess>());
        }

        private static StackRuntimeModule BuildModuleWithErrors(string code, out List<CodeError> errors)
        {
            var lexer = new DefaultLexer();
            lexer.Iterator = SourceCodeBuilder.Create().FromString(code).Build().CreateIterator();
            var errSink = new ListErrorSink();
            var parser = new DefaultBslParser(
                lexer,
                errSink,
                Mock.Of<PreprocessorHandlers>());

            var node = parser.ParseStatefulModule() as ModuleNode;
            var ctx = new SymbolTable();
            ctx.PushScope(new SymbolScope(), ScopeBindingDescriptor.Static(null));
            var compiler = new StackMachineCodeGenerator(errSink, ExplicitImportsBehavior.Disabled);
            var module = compiler.CreateModule(node, lexer.Iterator.Source, ctx, Mock.Of<IBslProcess>());
            errors = new List<CodeError>(errSink.Errors);
            return module;
        }

        [Fact]
        public void Forward_Goto_Compiles_Successfully()
        {
            var code = @"
                А = 1;
                Перейти ~Метка;
                А = 2;
                ~Метка:
                А = 3;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
            module.Code.Should().Contain(c => c.Code == OperationCode.Jmp);
        }

        [Fact]
        public void Backward_Goto_Compiles_Successfully()
        {
            var code = @"
                А = 0;
                ~Начало:
                А = А + 1;
                Если А < 5 Тогда
                    Перейти ~Начало;
                КонецЕсли;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
            module.Code.Should().Contain(c => c.Code == OperationCode.Jmp);
        }

        [Fact]
        public void Goto_Out_Of_Loop_Compiles_Successfully()
        {
            var code = @"
                Для Инд = 1 По 10 Цикл
                    Для Инд2 = 1 По 10 Цикл
                        Если Инд2 = 5 Тогда
                            Перейти ~ВыходИзЦиклов;
                        КонецЕсли;
                    КонецЦикла;
                КонецЦикла;
                ~ВыходИзЦиклов:
                А = 1;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
        }

        [Fact]
        public void Goto_Out_Of_If_Compiles_Successfully()
        {
            var code = @"
                А = 1;
                Если А = 1 Тогда
                    Перейти ~ПослеУсловия;
                    А = 2;
                КонецЕсли;
                ~ПослеУсловия:
                А = 3;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
        }

        [Fact]
        public void Goto_Out_Of_Try_Compiles_With_ExitTry()
        {
            var code = @"
                Попытка
                    Перейти ~ПослеПопытки;
                Исключение
                КонецПопытки;
                ~ПослеПопытки:
                А = 1;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
            module.Code.Should().Contain(c => c.Code == OperationCode.ExitTry);
        }

        [Fact]
        public void Goto_In_Procedure_Compiles_Successfully()
        {
            var code = @"
                Процедура Тест()
                    Перейти ~Конец;
                    А = 1;
                    ~Конец:
                КонецПроцедуры";

            var module = BuildModule(code);
            module.Should().NotBeNull();
        }

        [Fact]
        public void Goto_Into_Loop_Is_Error()
        {
            var code = @"
                Перейти ~Внутри;
                Для Инд = 1 По 10 Цикл
                    ~Внутри:
                    А = 1;
                КонецЦикла;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.InvalidGotoTarget));
        }

        [Fact]
        public void Goto_Into_If_Is_Error()
        {
            var code = @"
                Перейти ~Внутри;
                Если Истина Тогда
                    ~Внутри:
                    А = 1;
                КонецЕсли;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.InvalidGotoTarget));
        }

        [Fact]
        public void Goto_Into_Try_Is_Error()
        {
            var code = @"
                Перейти ~Внутри;
                Попытка
                    ~Внутри:
                    А = 1;
                Исключение
                КонецПопытки;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.InvalidGotoTarget));
        }

        [Fact]
        public void Goto_Into_Except_Is_Error()
        {
            var code = @"
                Перейти ~Внутри;
                Попытка
                Исключение
                    ~Внутри:
                    А = 1;
                КонецПопытки;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.InvalidGotoTarget));
        }

        [Fact]
        public void Undefined_Label_Is_Error()
        {
            var code = @"
                Перейти ~НесуществующаяМетка;
                А = 1;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.UndefinedLabel));
        }

        [Fact]
        public void Duplicate_Label_Is_Error()
        {
            var code = @"
                ~Метка:
                А = 1;
                ~Метка:
                А = 2;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.DuplicateLabelDefinition));
        }

        [Fact]
        public void Goto_Between_Sibling_Blocks_Is_Error()
        {
            var code = @"
                Для Инд = 1 По 10 Цикл
                    Перейти ~Цель;
                КонецЦикла;
                Пока Истина Цикл
                    ~Цель:
                    Прервать;
                КонецЦикла;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.InvalidGotoTarget));
        }

        [Fact]
        public void Labels_Do_Not_Leak_Between_Methods()
        {
            var code = @"
                Процедура Первая()
                    ~Метка:
                    А = 1;
                КонецПроцедуры

                Процедура Вторая()
                    Перейти ~Метка;
                КонецПроцедуры";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.UndefinedLabel));
        }

        [Fact]
        public void Goto_Out_Of_Except_Compiles_Successfully()
        {
            var code = @"
                Попытка
                    А = 1 / 0;
                Исключение
                    Перейти ~ПослеОбработки;
                КонецПопытки;
                ~ПослеОбработки:
                А = 1;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
        }

        [Fact]
        public void Goto_From_Except_To_Try_Of_Same_Block_Is_Error()
        {
            var code = @"
                Попытка
                    ~Внутри:
                    А = 1;
                Исключение
                    Перейти ~Внутри;
                КонецПопытки;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.InvalidGotoTarget));
        }

        [Fact]
        public void Goto_Out_Of_Nested_Try_Generates_ExitTry_With_Correct_Depth()
        {
            var code = @"
                Попытка
                    Попытка
                        Перейти ~Снаружи;
                    Исключение
                    КонецПопытки;
                Исключение
                КонецПопытки;
                ~Снаружи:
                А = 1;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
            // должен быть ExitTry с аргументом 2 (выход из двух вложенных try)
            module.Code.Should().Contain(c => c.Code == OperationCode.ExitTry && c.Argument == 2);
        }

        [Fact]
        public void Forward_Goto_Outside_Try_Does_Not_Generate_ExitTry()
        {
            var code = @"
                А = 1;
                Перейти ~Метка;
                А = 2;
                ~Метка:
                А = 3;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
            // ExitTry не должен генерироваться — goto вне try-блока
            module.Code.Should().NotContain(c => c.Code == OperationCode.ExitTry);
        }

        [Fact]
        public void Goto_Out_Of_ForEach_Generates_StopIterator()
        {
            var code = @"
                Массив = Новый Массив;
                Для Каждого Элемент Из Массив Цикл
                    Перейти ~ПослеЦикла;
                КонецЦикла;
                ~ПослеЦикла:
                А = 1;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
            // goto из ForEach должен генерировать StopIterator для очистки итератора
            module.Code.Should().Contain(c => c.Code == OperationCode.StopIterator);
        }

        [Fact]
        public void Goto_Out_Of_For_Generates_PopTmp()
        {
            var code = @"
                Для Инд = 1 По 10 Цикл
                    Перейти ~ПослеЦикла;
                КонецЦикла;
                ~ПослеЦикла:
                А = 1;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
            // goto из Для должен генерировать PopTmp для очистки верхней границы
            module.Code.Where(c => c.Code == OperationCode.PopTmp).Should().HaveCountGreaterThan(1);
        }

        [Fact]
        public void Goto_Between_Same_Type_Sibling_Blocks_Is_Error()
        {
            var code = @"
                Если Истина Тогда
                    Перейти ~Цель;
                КонецЕсли;
                Если Истина Тогда
                    ~Цель:
                    А = 1;
                КонецЕсли;";

            BuildModuleWithErrors(code, out var errors);
            errors.Should().Contain(e => e.ErrorId == nameof(CompilerErrors.InvalidGotoTarget));
        }

        [Fact]
        public void Case_Insensitive_Labels_Work()
        {
            var code = @"
                Перейти ~метка;
                А = 999;
                ~Метка:
                А = 1;";

            var module = BuildModule(code);
            module.Should().NotBeNull();
        }
    }
}
