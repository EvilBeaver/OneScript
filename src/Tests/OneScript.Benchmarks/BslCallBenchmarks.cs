/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using BenchmarkDotNet.Attributes;
using OneScript.Values;
using ScriptEngine.Machine;

namespace OneScript.Benchmarks
{
    /// <summary>
    /// Вызовы из 1Script, исполняемые стековой машиной. Каждый бенчмарк - цикл на ITERATIONS итераций,
    /// время и память показаны на одну итерацию. Цена самого вызова - разница с EmptyLoop.
    /// </summary>
    [MemoryDiagnoser]
    public class BslCallBenchmarks
    {
        private const int ITERATIONS = 10_000;

        private const string SOURCE = @"
Функция Сложить(А, Б) Экспорт
	Возврат А + Б;
КонецФункции

Функция СПоУмолчанию(А, Б = 1, В = 2) Экспорт
	Возврат А;
КонецФункции

Процедура ПустойЦикл(Н) Экспорт
	Для Сч = 1 По Н Цикл
	КонецЦикла;
КонецПроцедуры

Процедура Блокировка(Н) Экспорт
	Блокировка = Новый БлокировкаРесурса;
	Для Сч = 1 По Н Цикл
		Блокировка.Заблокировать();
		Блокировка.Разблокировать();
	КонецЦикла;
КонецПроцедуры

Процедура МассивУстановить(Н) Экспорт
	М = Новый Массив(1);
	Для Сч = 1 По Н Цикл
		М.Установить(0, Сч);
	КонецЦикла;
КонецПроцедуры

Процедура МассивКоличество(Н) Экспорт
	М = Новый Массив(1);
	Для Сч = 1 По Н Цикл
		Р = М.Количество();
	КонецЦикла;
КонецПроцедуры

Процедура МассивДобавить(Н) Экспорт
	М = Новый Массив;
	Для Сч = 1 По Н Цикл
		М.Добавить(Сч);
	КонецЦикла;
КонецПроцедуры

Процедура СтруктураВставить(Н) Экспорт
	С = Новый Структура;
	Для Сч = 1 По Н Цикл
		С.Вставить(""Ключ"", Сч);
	КонецЦикла;
КонецПроцедуры

Процедура СоответствиеПолучить(Н) Экспорт
	С = Новый Соответствие;
	С.Вставить(1, 1);
	Для Сч = 1 По Н Цикл
		Р = С.Получить(1);
	КонецЦикла;
КонецПроцедуры

Процедура ГлобальнаяФункция(Н) Экспорт
	Для Сч = 1 По Н Цикл
		Р = СтрНайти(""строка"", ""к"");
	КонецЦикла;
КонецПроцедуры

Процедура ФункцияСкрипта(Н) Экспорт
	Для Сч = 1 По Н Цикл
		Р = Сложить(Сч, 1);
	КонецЦикла;
КонецПроцедуры

Процедура ФункцияСкриптаПоУмолчанию(Н) Экспорт
	Для Сч = 1 По Н Цикл
		Р = СПоУмолчанию(Сч);
	КонецЦикла;
КонецПроцедуры

Процедура МетодОбъекта(Н) Экспорт
	Для Сч = 1 По Н Цикл
		Р = ЭтотОбъект.Сложить(Сч, 1);
	КонецЦикла;
КонецПроцедуры

Процедура РефлекторВызватьМетод(Н) Экспорт
	Рефлектор = Новый Рефлектор;
	М = Новый Массив(1);
	Аргументы = Новый Массив;
	Аргументы.Добавить(0);
	Аргументы.Добавить(1);
	Для Сч = 1 По Н Цикл
		Рефлектор.ВызватьМетод(М, ""Установить"", Аргументы);
	КонецЦикла;
КонецПроцедуры
";

        private BenchmarkEngine _engine;
        private IValue[] _arguments;

        [GlobalSetup]
        public void Setup()
        {
            _engine = BenchmarkEngine.Load(SOURCE);
            _arguments = new IValue[] { ValueFactory.Create(ITERATIONS) };
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _engine.Dispose();
        }

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void EmptyLoop() => Run("ПустойЦикл");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void LockUnlock() => Run("Блокировка");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void ArraySet() => Run("МассивУстановить");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void ArrayCount() => Run("МассивКоличество");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void ArrayAdd() => Run("МассивДобавить");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void StructureInsert() => Run("СтруктураВставить");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void MapGet() => Run("СоответствиеПолучить");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void GlobalFunction() => Run("ГлобальнаяФункция");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void ScriptFunction() => Run("ФункцияСкрипта");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void ScriptFunctionWithDefaults() => Run("ФункцияСкриптаПоУмолчанию");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void ObjectMethod() => Run("МетодОбъекта");

        [Benchmark(OperationsPerInvoke = ITERATIONS)]
        public void ReflectorCallMethod() => Run("РефлекторВызватьМетод");

        private void Run(string methodName)
        {
            var module = _engine.Module;
            module.CallAsProcedure(module.GetMethodNumber(methodName), _arguments, _engine.Process);
        }
    }
}
