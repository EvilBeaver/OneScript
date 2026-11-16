/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using FluentAssertions;
using OneScript.StandardLibrary.Tasks;
using ScriptEngine.Hosting;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class BackgroundTaskExceptionTests
    {
        private const string Script = @"
            Процедура ГлубокаяОшибка()
                ВызватьИсключение ""Исходная ошибка"";
            КонецПроцедуры

            Процедура ПромежуточныйМетод()
                ГлубокаяОшибка();
            КонецПроцедуры

            Процедура БезПерехвата() Экспорт
                ПромежуточныйМетод();
            КонецПроцедуры

            Процедура СПовторнымВыбросом() Экспорт
                Попытка
                    ПромежуточныйМетод();
                Исключение
                    ВызватьИсключение;
                КонецПопытки;
            КонецПроцедуры

            Функция ФункцияСПовторнымВыбросом() Экспорт
                Попытка
                    ПромежуточныйМетод();
                Исключение
                    ВызватьИсключение;
                КонецПопытки;

                Возврат Неопределено;
            КонецФункции

            Процедура СДвумяПовторнымиВыбросами() Экспорт
                Попытка
                    СПовторнымВыбросом();
                Исключение
                    ВызватьИсключение;
                КонецПопытки;
            КонецПроцедуры";

        [Theory]
        [InlineData("БезПерехвата", "ГлубокаяОшибка|ПромежуточныйМетод|БезПерехвата")]
        [InlineData("СПовторнымВыбросом", "ГлубокаяОшибка|ПромежуточныйМетод|СПовторнымВыбросом")]
        [InlineData("ФункцияСПовторнымВыбросом", "ГлубокаяОшибка|ПромежуточныйМетод|ФункцияСПовторнымВыбросом")]
        [InlineData("СДвумяПовторнымиВыбросами",
            "ГлубокаяОшибка|ПромежуточныйМетод|СПовторнымВыбросом|СДвумяПовторнымиВыбросами")]
        public void PreservesOriginalStack(string methodName, string expectedMethods)
        {
            var engine = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            engine.Initialize();

            var target = engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), Script, engine.NewProcess());
            var task = new BackgroundTask(target, methodName);

            task.ExecuteOnCurrentThread(engine.NewProcess());

            task.State.Should().Be(TaskStateEnum.CompletedWithErrors);
            task.ExceptionInfo.Should().NotBeNull();
            task.ExceptionInfo.Description.Should().Be("Исходная ошибка");

            var stack = task.ExceptionInfo.GetStackTrace().Should().BeOfType<StackTraceCollectionContext>().Subject;
            stack.Select(frame => frame.Method).Should().Equal(expectedMethods.Split('|'));

            var originalFrame = stack.First();
            originalFrame.LineNumber.Should().BeGreaterThan(0);
            originalFrame.LineNumber.Should().Be(task.ExceptionInfo.LineNumber);
            originalFrame.ModuleName.Should().Be(task.ExceptionInfo.ModuleName);
        }
    }
}
