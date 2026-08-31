/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using FluentAssertions;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.HostedScript;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    public class EventProcessorTests
    {
        private const string HandlerScript = @"
            Перем Вызовов Экспорт;

            Процедура Обработчик() Экспорт
                Вызовов = Вызовов + 1;
            КонецПроцедуры

            Вызовов = 0;";

        private static (UserScriptContextInstance Handler, IBslProcess Process) CreateHandler()
        {
            var engine = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            engine.Initialize();

            var process = engine.NewProcess();
            var handler = engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), HandlerScript, process);

            return (handler, process);
        }

        private static decimal CallCount(UserScriptContextInstance handler)
        {
            var propertyIndex = handler.GetPropertyNumber("Вызовов");
            return (decimal)(BslNumericValue)handler.GetPropValue(propertyIndex);
        }

        [Fact]
        public void RemoveAllHandlers_UnsubscribesEverythingOfTheSource()
        {
            var (handler, process) = CreateHandler();
            var eventSource = new ArrayImpl();
            IEventProcessor processor = new DefaultEventProcessor();

            processor.AddHandler(eventSource, "ПриЗавершении", handler, "Обработчик");
            processor.HandleEvent(eventSource, "ПриЗавершении", Array.Empty<IValue>(), process);

            CallCount(handler).Should().Be(1, "подписка должна работать до её снятия");

            processor.RemoveAllHandlers(eventSource);
            processor.HandleEvent(eventSource, "ПриЗавершении", Array.Empty<IValue>(), process);

            CallCount(handler).Should().Be(1, "после снятия подписок обработчик вызываться не должен");
        }

        [Fact]
        public void RemoveAllHandlers_KeepsSubscriptionsOfOtherSources()
        {
            var (handler, process) = CreateHandler();
            var releasedSource = new ArrayImpl();
            var aliveSource = new ArrayImpl();
            IEventProcessor processor = new DefaultEventProcessor();

            processor.AddHandler(releasedSource, "ПриЗавершении", handler, "Обработчик");
            processor.AddHandler(aliveSource, "ПриЗавершении", handler, "Обработчик");

            processor.RemoveAllHandlers(releasedSource);
            processor.HandleEvent(aliveSource, "ПриЗавершении", Array.Empty<IValue>(), process);

            CallCount(handler).Should().Be(1, "снятие подписок одного источника не трогает другие");
        }
    }
}
