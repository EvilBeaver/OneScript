/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using FluentAssertions;
using OneScript.Contexts;
using ScriptEngine.Hosting;
using ScriptEngine.Machine.Contexts;
using Xunit;

namespace OneScript.Core.Tests
{
    [GlobalContext(ManualRegistration = true)]
    public class EvalScopesTestGlobals : GlobalContextBase<EvalScopesTestGlobals>
    {
        [ContextProperty("СвойствоТестовогоКонтекста")]
        public string Property => "свойство контекста";

        [ContextMethod("МетодТестовогоКонтекста")]
        public string Method() => "метод контекста";
    }

    [GlobalContext(ManualRegistration = true)]
    public class EvalScopesTestHost : GlobalContextBase<EvalScopesTestHost>
    {
        private readonly IRuntimeEnvironment _environment;

        public EvalScopesTestHost(IRuntimeEnvironment environment)
        {
            _environment = environment;
        }

        // Как ПодключитьВнешнююКомпоненту со своим глобальным контекстом
        [ContextMethod("ПодключитьТестовыйКонтекст")]
        public void Attach() => _environment.InjectObject(new EvalScopesTestGlobals());
    }

    public class EvalScopesTests
    {
        private const string Script =
            "Перем МояПеременная;\n" +
            "Перем Итог Экспорт;\n" +
            "\n" +
            "Функция МояФункция()\n" +
            "	Возврат \"функция модуля\";\n" +
            "КонецФункции\n" +
            "\n" +
            "МояПеременная = \"переменная модуля\";\n" +
            "Итог = \"\";\n" +
            "Значение = Неопределено;\n" +
            "Для Номер = 1 По 2 Цикл\n" +
            "	Итог = Итог + Вычислить(\"МояПеременная\") + \"; \" + Вычислить(\"МояФункция()\") + \"; \";\n" +
            "	Выполнить(\"Значение = МояПеременная\");\n" +
            "	Итог = Итог + Значение + \"; \";\n" +
            "	Если Номер = 1 Тогда\n" +
            "		ПодключитьТестовыйКонтекст();\n" +
            "	КонецЕсли;\n" +
            "КонецЦикла;\n";

        [Fact]
        public void CachedExpressionsSeeModuleAfterGlobalContextIsAttached()
        {
            var engine = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            engine.Initialize();
            engine.Environment.InjectObject(new EvalScopesTestHost(engine.Environment));

            // Вычислить и Выполнить кэшируют выражения у машины процесса, поэтому все в одном запуске
            var instance = engine.AttachedScriptsFactory.LoadFromString(engine.GetCompilerService(), Script, engine.NewProcess());

            instance.GetPropValue(instance.GetPropertyNumber("Итог")).ToString().Should().Be(
                "переменная модуля; функция модуля; переменная модуля; " +
                "переменная модуля; функция модуля; переменная модуля; ");
        }
    }
}
