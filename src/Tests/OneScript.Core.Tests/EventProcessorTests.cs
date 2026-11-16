/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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

            Процедура ПадающийОбработчик() Экспорт
                ВызватьИсключение ""ошибка обработчика"";
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

        [Fact]
        public void HandleEvent_WithErrorCallback_CallsHandlersAfterFailedOne()
        {
            var (handler, process) = CreateHandler();
            var eventSource = new ArrayImpl();
            IEventProcessor processor = new DefaultEventProcessor();
            var errors = new List<Exception>();

            processor.AddHandler(eventSource, "ПриЗавершении", handler, "ПадающийОбработчик");
            processor.AddHandler(eventSource, "ПриЗавершении", handler, "Обработчик");
            processor.HandleEvent(eventSource, "ПриЗавершении", Array.Empty<IValue>(), process, errors.Add);

            errors.Should().ContainSingle();
            CallCount(handler).Should().Be(1, "ошибка первого обработчика не должна отменять второй");
        }

        [Fact]
        public void HandleEvent_WithoutErrorCallback_StopsOnFailedHandler()
        {
            var (handler, process) = CreateHandler();
            var eventSource = new ArrayImpl();
            IEventProcessor processor = new DefaultEventProcessor();

            processor.AddHandler(eventSource, "Событие", handler, "ПадающийОбработчик");
            processor.AddHandler(eventSource, "Событие", handler, "Обработчик");
            var handleEvent = () => processor.HandleEvent(eventSource, "Событие", Array.Empty<IValue>(), process);

            handleEvent.Should().Throw<Exception>("ошибка обработчика обычного события уходит вызывающему");
            CallCount(handler).Should().Be(0);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void UnreachableSource_IsCollected(bool removeHandler)
        {
            var (handler, process) = CreateHandler();
            IEventProcessor processor = new DefaultEventProcessor();
            var source = SubscribeTemporarySource(processor, handler, removeHandler);

            CollectGarbage();

            source.IsAlive.Should().BeFalse("реестр событий не должен продлевать жизнь источника");
            GC.KeepAlive(processor);
            GC.KeepAlive(handler);
            GC.KeepAlive(process);
        }

        [Fact]
        public void SelfSubscribedSource_IsCollected()
        {
            IEventProcessor processor = new DefaultEventProcessor();
            var source = SubscribeSourceToItself(processor);

            CollectGarbage();

            source.IsAlive.Should().BeFalse("ссылка обработчика на источник не должна удерживать весь цикл");
            GC.KeepAlive(processor);
        }

        [Fact]
        public void LiveSource_KeepsHandlerAlive()
        {
            IEventProcessor processor = new DefaultEventProcessor();
            var source = new ArrayImpl();
            var (handlerReference, process) = SubscribeTemporaryHandler(processor, source);

            CollectGarbage();

            handlerReference.IsAlive.Should().BeTrue("пока источник жив, его обработчики должны оставаться подписанными");
            processor.HandleEvent(source, "Событие", Array.Empty<IValue>(), process);
            CallCount((UserScriptContextInstance)handlerReference.Target).Should().Be(1);
            GC.KeepAlive(source);
            GC.KeepAlive(processor);
            GC.KeepAlive(process);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void RemovedEventName_IsCollectedWhileSourceIsAlive(bool keepAnotherEvent)
        {
            var (handler, process) = CreateHandler();
            var source = new ArrayImpl();
            IEventProcessor processor = new DefaultEventProcessor();

            if (keepAnotherEvent)
                processor.AddHandler(source, "ОставшеесяСобытие", handler, "Обработчик");

            var eventName = SubscribeAndRemoveTemporaryEvent(processor, source, handler);

            CollectGarbage();

            eventName.IsAlive.Should().BeFalse("имя события без обработчиков не должно оставаться в реестре");

            if (keepAnotherEvent)
            {
                processor.HandleEvent(source, "ОставшеесяСобытие", Array.Empty<IValue>(), process);

                CallCount(handler).Should().Be(1, "очистка одного события сохраняет остальные подписки");
            }

            GC.KeepAlive(source);
            GC.KeepAlive(processor);
            GC.KeepAlive(handler);
            GC.KeepAlive(process);
        }

        [Fact]
        public void RemoveHandler_PreservesRemainingHandlersAndEvents()
        {
            var (handler, process) = CreateHandler();
            var source = new ArrayImpl();
            IEventProcessor processor = new DefaultEventProcessor();
            var errors = new List<Exception>();

            processor.AddHandler(source, "ПервоеСобытие", handler, "Обработчик");
            processor.AddHandler(source, "ПервоеСобытие", handler, "ПадающийОбработчик");
            processor.AddHandler(source, "ВтороеСобытие", handler, "Обработчик");

            processor.RemoveHandler(source, "ПервоеСобытие", handler, "Обработчик");
            processor.HandleEvent(source, "ПервоеСобытие", Array.Empty<IValue>(), process, errors.Add);

            errors.Should().ContainSingle("второй обработчик того же события остаётся подписанным");
            CallCount(handler).Should().Be(0);

            processor.RemoveHandler(source, "ПервоеСобытие", handler, "ПадающийОбработчик");
            processor.HandleEvent(source, "ПервоеСобытие", Array.Empty<IValue>(), process, errors.Add);
            processor.HandleEvent(source, "ВтороеСобытие", Array.Empty<IValue>(), process);

            errors.Should().ContainSingle("снятый обработчик больше не вызывается");
            CallCount(handler).Should().Be(1, "другое событие источника остаётся подписанным");
        }

        [Fact]
        public void RemoveHandler_LastSubscription_AllowsResubscribe()
        {
            var (handler, process) = CreateHandler();
            var source = new ArrayImpl();
            IEventProcessor processor = new DefaultEventProcessor();

            processor.AddHandler(source, "Событие", handler, "Обработчик");
            processor.RemoveHandler(source, "Событие", handler, "Обработчик");
            processor.RemoveHandler(source, "Событие", handler, "Обработчик");
            processor.HandleEvent(source, "Событие", Array.Empty<IValue>(), process);

            CallCount(handler).Should().Be(0);

            processor.AddHandler(source, "Событие", handler, "Обработчик");
            processor.HandleEvent(source, "Событие", Array.Empty<IValue>(), process);

            CallCount(handler).Should().Be(1, "после удаления последней подписки можно подписаться заново");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference SubscribeAndRemoveTemporaryEvent(IEventProcessor processor,
            ArrayImpl source, UserScriptContextInstance handler)
        {
            var eventName = new string('С', 32);
            processor.AddHandler(source, eventName, handler, "Обработчик");
            processor.RemoveHandler(source, eventName, handler, "Обработчик");

            return new WeakReference(eventName);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference SubscribeTemporarySource(IEventProcessor processor,
            UserScriptContextInstance handler, bool removeHandler)
        {
            var source = new ArrayImpl();
            processor.AddHandler(source, "Событие", handler, "Обработчик");
            if (removeHandler)
                processor.RemoveHandler(source, "Событие", handler, "Обработчик");

            return new WeakReference(source);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference SubscribeSourceToItself(IEventProcessor processor)
        {
            var (handler, _) = CreateHandler();
            processor.AddHandler(handler, "Событие", handler, "Обработчик");
            return new WeakReference(handler);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static (WeakReference Handler, IBslProcess Process) SubscribeTemporaryHandler(
            IEventProcessor processor, ArrayImpl source)
        {
            var (handler, process) = CreateHandler();
            processor.AddHandler(source, "Событие", handler, "Обработчик");
            return (new WeakReference(handler), process);
        }

        private static void CollectGarbage()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}
