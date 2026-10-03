/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using FluentAssertions;
using OneScript.StandardLibrary;
using OneScript.Web.Server;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.Core.Tests
{
    public class WebServerConcurrencyTests
    {
        private const string WebSocketScript =
            "Перем мСервер Экспорт;\n" +
            "\n" +
            "Функция Прочитать(Сокет) Экспорт\n" +
            "	Возврат Сокет.ПолучитьСтроку();\n" +
            "КонецФункции\n" +
            "\n" +
            "Функция ОбработчикЗапроса(Контекст, СледующийОбработчик) Экспорт\n" +
            "	Сокет = Контекст.ВебСокеты.ПодключитьВебСокет();\n" +
            "	Параметры = Новый Массив;\n" +
            "	Параметры.Добавить(Сокет);\n" +
            "	Задания = Новый Массив;\n" +
            "	// Два задания ждут сообщения из одного сокета одновременно\n" +
            "	Задания.Добавить(ФоновыеЗадания.Выполнить(ЭтотОбъект, \"Прочитать\", Параметры, Истина));\n" +
            "	Задания.Добавить(ФоновыеЗадания.Выполнить(ЭтотОбъект, \"Прочитать\", Параметры, Истина));\n" +
            "	ФоновыеЗадания.ОжидатьВсе(Задания);\n" +
            "	Ответы = Новый Массив;\n" +
            "	Для Каждого Задание Из Задания Цикл\n" +
            "		Если Задание.ИнформацияОбОшибке = Неопределено Тогда\n" +
            "			Ответы.Добавить(Задание.Результат);\n" +
            "		Иначе\n" +
            "			Ответы.Добавить(\"ошибка: \" + Задание.ИнформацияОбОшибке.Описание);\n" +
            "		КонецЕсли;\n" +
            "	КонецЦикла;\n" +
            "	Сокет.ОтправитьСтроку(СтрСоединить(Ответы, \"|\"));\n" +
            "КонецФункции\n" +
            "\n" +
            "Процедура ЗапуститьСервер() Экспорт\n" +
            "	мСервер.Запустить();\n" +
            "КонецПроцедуры\n" +
            "\n" +
            "мСервер = Новый ВебСервер(Порт);\n" +
            "мСервер.ИспользоватьВебСокеты();\n" +
            "мСервер.ДобавитьОбработчикЗапросов(ЭтотОбъект, \"ОбработчикЗапроса\");\n" +
            "Задание = ФоновыеЗадания.Выполнить(ЭтотОбъект, \"ЗапуститьСервер\");\n";

        private const string SecondRunScript =
            "Перем мСервер;\n" +
            "Перем ОшибкаПовторногоЗапуска Экспорт;\n" +
            "Перем Остановлен Экспорт;\n" +
            "\n" +
            "Функция ОбработчикЗапроса(Контекст, СледующийОбработчик) Экспорт\n" +
            "	Контекст.Ответ.КодСостояния = 200;\n" +
            "КонецФункции\n" +
            "\n" +
            "Процедура ЗапуститьСервер() Экспорт\n" +
            "	мСервер.Запустить();\n" +
            "КонецПроцедуры\n" +
            "\n" +
            "мСервер = Новый ВебСервер(0);\n" +
            "мСервер.ДобавитьОбработчикЗапросов(ЭтотОбъект, \"ОбработчикЗапроса\");\n" +
            "Задания = Новый Массив;\n" +
            "Задания.Добавить(ФоновыеЗадания.Выполнить(ЭтотОбъект, \"ЗапуститьСервер\"));\n" +
            "Для Номер = 1 По 200 Цикл\n" +
            "	Если мСервер.Порт <> 0 Тогда\n" +
            "		Прервать;\n" +
            "	КонецЕсли;\n" +
            "	Приостановить(50);\n" +
            "КонецЦикла;\n" +
            "Попытка\n" +
            "	мСервер.Запустить();\n" +
            "	ОшибкаПовторногоЗапуска = \"\";\n" +
            "Исключение\n" +
            "	ОшибкаПовторногоЗапуска = ИнформацияОбОшибке().Описание;\n" +
            "КонецПопытки;\n" +
            "мСервер.Остановить();\n" +
            "Остановлен = ФоновыеЗадания.ОжидатьВсе(Задания, 10000);\n";

        [Fact]
        public void WebSocketMessagesAreReceivedWholeFromManyTasks()
        {
            var engine = CreateEngine();
            var port = FreePort();
            var context = new ExternalContextData
            {
                { "Порт", ValueFactory.Create(port) }
            };
            var instance = engine.Engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), WebSocketScript, engine.Engine.NewProcess(), context);
            var server = (WebServer)instance.GetPropValue(instance.GetPropertyNumber("мСервер"));
            try
            {
                using var client = Connect(new Uri($"ws://127.0.0.1:{port}/"));
                Send(client, "раз");
                Send(client, "два");

                var answers = Receive(client).Split('|');

                answers.Should().BeEquivalentTo("раз", "два");
            }
            finally
            {
                server.Stop();
            }
        }

        [Fact]
        public void SecondRunOfSameServerIsRejected()
        {
            var engine = CreateEngine();
            var instance = engine.Engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), SecondRunScript, engine.Engine.NewProcess());

            instance.GetPropValue(instance.GetPropertyNumber("ОшибкаПовторногоЗапуска")).ToString()
                .Should().Contain("уже запущен");
            instance.GetPropValue(instance.GetPropertyNumber("Остановлен")).AsBoolean()
                .Should().BeTrue("Остановить должен остановить первый запуск");
        }

        private static ClientWebSocket Connect(Uri uri)
        {
            for (var attempt = 0; ; attempt++)
            {
                var client = new ClientWebSocket();
                try
                {
                    client.ConnectAsync(uri, CancellationToken.None).Wait();
                    return client;
                }
                catch (AggregateException) when (attempt < 100)
                {
                    // Сервер еще запускается в фоновом задании
                    client.Dispose();
                    Thread.Sleep(100);
                }
            }
        }

        private static void Send(ClientWebSocket client, string text)
        {
            client.SendAsync(Encoding.UTF8.GetBytes(text), WebSocketMessageType.Text, true, CancellationToken.None).Wait();
        }

        private static string Receive(ClientWebSocket client)
        {
            var buffer = new byte[4096];
            using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var result = client.ReceiveAsync(buffer, cancellation.Token).Result;
            return Encoding.UTF8.GetString(buffer, 0, result.Count);
        }

        private static HostedScriptEngine CreateEngine()
        {
            var builder = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .UseImports()
                .UseDefaultHosting()
                .SetupEnvironment(e => e.AddStandardLibrary().AddWebServer());
            var engine = new HostedScriptEngine(builder.Build());
            engine.Initialize();
            return engine;
        }

        private static int FreePort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
