/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using OneScript.Contexts;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Binary;
using OneScript.Web.Server;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    [GlobalContext(ManualRegistration = true)]
    public class RequestTraceProbe : GlobalContextBase<RequestTraceProbe>
    {
        [ContextMethod("ТрассировкаЗапроса")]
        public string CurrentActivity() => System.Diagnostics.Activity.Current?.Id ?? "";
    }

    public class WebServerOutputTests
    {
        private const string Script =
            "Перем мСервер;\n" +
            "\n" +
            "Функция ОбработчикЗапроса(Контекст, СледующийОбработчик) Экспорт\n" +
            "	Консоль.ВывестиСтроку(\"from request\");\n" +
            "	Контекст.Ответ.КодСостояния = 200;\n" +
            "КонецФункции\n" +
            "\n" +
            "Процедура ЗапуститьСервер() Экспорт\n" +
            "	мСервер.Запустить();\n" +
            "КонецПроцедуры\n" +
            "\n" +
            "мСервер = Новый ВебСервер(Порт);\n" +
            "мСервер.ДобавитьОбработчикЗапросов(ЭтотОбъект, \"ОбработчикЗапроса\");\n" +
            "Консоль.УстановитьПотокВывода(Вывод);\n" +
            "Задание = ФоновыеЗадания.Выполнить(ЭтотОбъект, \"ЗапуститьСервер\");\n" +
            "Соединение = Новый HTTPСоединение(\"http://127.0.0.1:\" + Формат(Порт, \"ЧГ=\"));\n" +
            "Для Номер = 1 По 100 Цикл\n" +
            "	Попытка\n" +
            "		Соединение.Получить(Новый HTTPЗапрос(\"/\"));\n" +
            "		Прервать;\n" +
            "	Исключение\n" +
            "		Приостановить(100);\n" +
            "	КонецПопытки;\n" +
            "КонецЦикла;\n" +
            "мСервер.Остановить();\n" +
            "Задание.ОжидатьЗавершения(10000);\n";

        [Fact]
        public void RequestHandlersWriteToOutputOfServerStarter()
        {
            var engine = CreateEngine();
            var output = MemoryStreamContext.Constructor();
            var context = new ExternalContextData
            {
                { "Порт", ValueFactory.Create(FreePort()) },
                { "Вывод", output }
            };

            engine.Engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), Script, engine.Engine.NewProcess(), context);

            // Текст латиницей: кодировка консоли на агентах CI может не содержать кириллицу
            var bytes = ((MemoryStream)output.GetUnderlyingStream()).ToArray();
            using var reader = new StreamReader(new MemoryStream(bytes), System.Console.OutputEncoding, true);
            reader.ReadToEnd().Should().Contain("from request");
        }

        [Fact]
        public void RequestHandlersKeepRequestExecutionContext()
        {
            var engine = CreateEngine();
            engine.Engine.Environment.InjectObject(new RequestTraceProbe());
            var context = new ExternalContextData
            {
                { "Порт", ValueFactory.Create(FreePort()) },
                { "Вывод", MemoryStreamContext.Constructor() }
            };

            var script = Script.Replace(
                "	Консоль.ВывестиСтроку(\"from request\");\n",
                "	Трассировка = ТрассировкаЗапроса();\n");
            var instance = engine.Engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), "Перем Трассировка Экспорт;\n" + script, engine.Engine.NewProcess(), context);

            // Активность запроса ASP.NET Core хранится в AsyncLocal контекста запроса:
            // перенос вывода консоли не должен подменять этот контекст контекстом старта сервера
            instance.GetPropValue(instance.GetPropertyNumber("Трассировка")).ToString().Should().NotBeEmpty();
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
