/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Threading.Tasks;
using OneScript.StandardLibrary;
using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Extensions;
using ScriptEngine.Hosting;
using Xunit;

namespace OneScript.Core.Tests
{
    public class BackgroundTaskThreadTests
    {
        private const string Script =
            "Перем Блок;\n" +
            "\n" +
            "Функция ВзятьБлокировку() Экспорт\n" +
            "	Если Блок.Заблокировать(50) Тогда\n" +
            "		Блок.Разблокировать();\n" +
            "		Возврат Истина;\n" +
            "	КонецЕсли;\n" +
            "	Возврат Ложь;\n" +
            "КонецФункции\n" +
            "\n" +
            "Блок = Новый БлокировкаРесурса;\n" +
            "Для Номер = 1 По 20 Цикл\n" +
            "	Блок.Заблокировать();\n" +
            "	Задание = ФоновыеЗадания.Выполнить(ЭтотОбъект, \"ВзятьБлокировку\");\n" +
            "	Задание.ОжидатьЗавершения();\n" +
            "	Блок.Разблокировать();\n" +
            "	Если Задание.Результат Тогда\n" +
            "		ВызватьИсключение \"Задание получило блокировку, которую держит ожидающий\";\n" +
            "	КонецЕсли;\n" +
            "КонецЦикла;\n";

        [Fact]
        public void TaskAwaitedFromThreadPoolRunsInItsOwnThread()
        {
            var builder = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .UseImports()
                .UseDefaultHosting()
                .SetupEnvironment(e => e.AddStandardLibrary());
            var engine = new HostedScriptEngine(builder.Build());
            engine.Initialize();

            // Как запрос веб-сервера: процесс без отмены в потоке пула. Если задание выполнится
            // в этом же потоке, оно войдет в чужую блокировку и при завершении отпустит ее
            Task.Run(() => engine.Engine.AttachedScriptsFactory.LoadFromString(
                engine.GetCompilerService(), Script, engine.Engine.NewProcess())).GetAwaiter().GetResult();
        }
    }
}
