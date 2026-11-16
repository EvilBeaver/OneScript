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

        // Контекст подключается во вложенном Выполнить, и следующий вложенный Выполнить его уже видит
        private const string NestedScript =
            "Перем МояПеременная;\n" +
            "Перем Итог Экспорт;\n" +
            "Перем Внешний;\n" +
            "Перем Внутренний;\n" +
            "Перем ПослеПодключения;\n" +
            "\n" +
            "Функция МояФункция()\n" +
            "	Возврат \"функция модуля\";\n" +
            "КонецФункции\n" +
            "\n" +
            "МояПеременная = \"переменная модуля\";\n" +
            "Итог = \"\";\n" +
            "Внутренний = \"Итог = Итог + МояПеременная + \"\"; \"\" + МояФункция() + \"\"; \"\" + Вычислить(\"\"Локальная\"\") + \"\"; \"\"\";\n" +
            "ПослеПодключения = \"Итог = Итог + МетодТестовогоКонтекста() + \"\"; \"\" + Локальная + \"\"; \"\"\";\n" +
            "Внешний = \"Локальная = \"\"локальная\"\"; Выполнить(Внутренний);" +
            " Если Номер = 1 Тогда ПодключитьТестовыйКонтекст(); Выполнить(ПослеПодключения); КонецЕсли\";\n" +
            "Для Номер = 1 По 2 Цикл\n" +
            "	Выполнить(Внешний);\n" +
            "КонецЦикла;\n";

        [Fact]
        public void CachedExpressionsSeeModuleAfterGlobalContextIsAttached()
        {
            Run(Script).Should().Be(
                "переменная модуля; функция модуля; переменная модуля; " +
                "переменная модуля; функция модуля; переменная модуля; ");
        }

        [Fact]
        public void NestedExecuteSeesModuleAfterGlobalContextIsAttachedInside()
        {
            Run(NestedScript).Should().Be(
                "переменная модуля; функция модуля; локальная; " +
                "метод контекста; локальная; " +
                "переменная модуля; функция модуля; локальная; ");
        }

        private static string Run(string script)
        {
            var engine = DefaultEngineBuilder.Create().SetDefaultOptions().Build();
            engine.Initialize();
            engine.Environment.InjectObject(new EvalScopesTestHost(engine.Environment));

            // Вычислить и Выполнить кэшируют выражения у машины процесса, поэтому все в одном запуске
            var instance = engine.AttachedScriptsFactory.LoadFromString(engine.GetCompilerService(), script, engine.NewProcess());

            return instance.GetPropValue(instance.GetPropertyNumber("Итог")).ToString();
        }
    }
}
