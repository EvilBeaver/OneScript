Перем юТест;

////////////////////////////////////////////////////////////////////
// Программный интерфейс

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПолучениеАннотацийМетода");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПолучениеАннотацийПараметров");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьАннотацииПолейЗагрузитьСценарий");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьАннотацииПолейЗагрузитьСценарийИзСтроки");
	
	ВсеТесты.Добавить("ТестДолжен_ПроверитьАннотациюКакЗначениеПараметраАннотации");
	
	Возврат ВсеТесты;
	
КонецФункции

&Аннотация(Параметр = &ТожеАннотация(&СТожеПараметромАннотацией))
Процедура ТестДолжен_ПроверитьАннотациюКакЗначениеПараметраАннотации() Экспорт

	Рефлектор = Новый Рефлектор;
	ТаблицаМетодов = Рефлектор.ПолучитьТаблицуМетодов(ЭтотОбъект);

	юТест.ПроверитьНеРавенство(ТаблицаМетодов.Колонки.Найти("Аннотации"), Неопределено, "Есть колонка Аннотации");

	СтрокаМетода = ТаблицаМетодов.Найти("ТестДолжен_ПроверитьАннотациюКакЗначениеПараметраАннотации", "Имя");
	ПерваяАннотация = СтрокаМетода.Аннотации[0];
	ПервыйПараметрПервойАннотации = ПерваяАннотация.Параметры[0];
	
	юТест.ПроверитьТип(ПервыйПараметрПервойАннотации.Значение, Тип("ТаблицаЗначений"));
	
	юТест.ПроверитьРавенство(ПервыйПараметрПервойАннотации.Значение[0].Имя, "ТожеАннотация");
	юТест.ПроверитьТип(ПервыйПараметрПервойАннотации.Значение[0].Параметры, Тип("ТаблицаЗначений"));
	
	юТест.ПроверитьТип(ПервыйПараметрПервойАннотации.Значение[0].Параметры[0].Значение, Тип("ТаблицаЗначений"));
	юТест.ПроверитьРавенство(ПервыйПараметрПервойАннотации.Значение[0].Параметры[0].Значение[0].Имя, "СТожеПараметромАннотацией");
	
КонецПроцедуры

Процедура САннотированнымиПараметрами(
	
	&АннотацияДляПараметра
	Знач Парам1,

	&АннотацияДляПараметра()
	&АннотацияДляПараметра1
	&АннотацияДляПараметра2(СПараметрами = 3, 4, -5)
	Знач Парам2,

	Парам3,
	Парам4 = Неопределено
) Экспорт

КонецПроцедуры

&НаСервере
&НаКлиентеНаСервереБезКонтекста
&НаЧемУгодно(ДажеСПараметром = "Да", СПараметромБезЗначения, "Значение без параметра")
&НаЧемУгодно(ДажеДважды = Истина)
&НаЧемУгодно()
Процедура ТестДолжен_ПроверитьПолучениеАннотацийМетода() Экспорт

	Рефлектор = Новый Рефлектор;
	ТаблицаМетодов = Рефлектор.ПолучитьТаблицуМетодов(ЭтотОбъект);

	юТест.ПроверитьНеРавенство(ТаблицаМетодов.Колонки.Найти("Аннотации"), Неопределено, "Есть колонка Аннотации");

	СтрокаМетода = ТаблицаМетодов.Найти("ТестДолжен_ПроверитьПолучениеАннотацийМетода", "Имя");
	юТест.ПроверитьНеРавенство(СтрокаМетода, Неопределено, "Метод с аннотациями есть в таблице рефлектора");

	юТест.ПроверитьНеРавенство(СтрокаМетода.Аннотации, Неопределено, "Рефлектор знает про аннотации метода");
	юТест.ПроверитьРавенство(СтрокаМетода.Аннотации.Количество(), 5, "Рефлектор вернул верное количество аннотаций");

	юТест.ПроверитьРавенство(СтрокаМетода.Аннотации[0].Имя, "НаСервере", "Рефлектор сохранил порядок указания аннотаций");
	юТест.ПроверитьРавенство(СтрокаМетода.Аннотации[1].Имя, "НаКлиентеНаСервереБезКонтекста", "Рефлектор сохранил порядок указания аннотаций");
	юТест.ПроверитьРавенство(СтрокаМетода.Аннотации[2].Имя, "НаЧемУгодно", "Рефлектор сохранил порядок указания аннотаций");
	юТест.ПроверитьРавенство(СтрокаМетода.Аннотации[3].Имя, "НаЧемУгодно", "Рефлектор сохранил порядок указания аннотаций");
	юТест.ПроверитьРавенство(СтрокаМетода.Аннотации[4].Имя, "НаЧемУгодно", "Рефлектор сохранил порядок указания аннотаций");

	Аннотация2 = СтрокаМетода.Аннотации[2];
	юТест.ПроверитьНеРавенство(Аннотация2.Параметры,                       Неопределено,             "Есть таблица параметров аннотации");
	юТест.ПроверитьРавенство  (Аннотация2.Параметры.Получить(0).Имя,       "ДажеСПараметром",        "Знаем имя именованного параметра");
	юТест.ПроверитьРавенство  (Аннотация2.Параметры.Получить(0).Значение,  "Да",                     "Знаем значение именованного параметра");
	юТест.ПроверитьРавенство  (Аннотация2.Параметры.Получить(1).Имя,       "СПараметромБезЗначения", "Знаем имя именованного параметра");
	юТест.ПроверитьРавенство  (Аннотация2.Параметры.Получить(1).Значение,  Неопределено,             "Знаем, что значение не определено");
	юТест.ПроверитьРавенство  (Аннотация2.Параметры.Получить(2).Имя,       Неопределено,             "Знаем, что имя не определно");
	юТест.ПроверитьРавенство  (Аннотация2.Параметры.Получить(2).Значение,  "Значение без параметра", "Знаем значение параметра без имени");

КонецПроцедуры

Процедура ТестДолжен_ПроверитьПолучениеАннотацийПараметров() Экспорт

	Рефлектор = Новый Рефлектор;
	ТаблицаМетодов = Рефлектор.ПолучитьТаблицуМетодов(ЭтотОбъект);

	юТест.ПроверитьНеРавенство(ТаблицаМетодов.Колонки.Найти("Параметры"), Неопределено, "Рефлектор знает о параметрах");

	СтрокаМетода = ТаблицаМетодов.Найти("САннотированнымиПараметрами", "Имя");
	юТест.ПроверитьРавенство(СтрокаМетода.Параметры.Количество(), 4, "Правильное количество параметров");

	Парам1 = СтрокаМетода.Параметры.Получить(0);
	Парам2 = СтрокаМетода.Параметры.Получить(1);
	Парам3 = СтрокаМетода.Параметры.Получить(2);
	Парам4 = СтрокаМетода.Параметры.Получить(3);

	юТест.ПроверитьРавенство(Парам1.Аннотации.Получить(0).Имя, "АннотацияДляПараметра", "Аннотации параметров");
	юТест.ПроверитьРавенство(Парам2.Аннотации.Получить(0).Имя, "АннотацияДляПараметра", "Аннотации параметров");
	юТест.ПроверитьРавенство(Парам2.Аннотации.Получить(1).Имя, "АннотацияДляПараметра1", "Аннотации параметров");
	юТест.ПроверитьРавенство(Парам2.Аннотации.Получить(2).Имя, "АннотацияДляПараметра2", "Аннотации параметров");
	юТест.ПроверитьРавенство(Парам2.Аннотации.Получить(2).Параметры.Количество(), 3, "Параметры аннотации параметров");
	юТест.ПроверитьРавенство(Парам2.Аннотации.Получить(2).Параметры[0].Значение, 3, "Значения параметров аннотации параметров");
	юТест.ПроверитьРавенство(Парам2.Аннотации.Получить(2).Параметры[1].Значение, 4, "Значения параметров аннотации параметров");
	юТест.ПроверитьРавенство(Парам2.Аннотации.Получить(2).Параметры[2].Значение, -5, "Значения параметров аннотации параметров");
	юТест.ПроверитьРавенство(Парам3.Аннотации.Количество(), 0);
	юТест.ПроверитьРавенство(Парам4.Аннотации.Количество(), 0);
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьАннотацииПолейЗагрузитьСценарий() Экспорт
	
	Файл = ПолучитьИмяВременногоФайла(".os");

	Запись = Новый ЗаписьТекста(Файл);
	Запись.Записать(ТекстСценария());
	Запись.Закрыть();

	Сценарий = ЗагрузитьСценарий(Файл);

	УдалитьФайлы(Файл);

	ПроверитьАннотацииПоляСценария(Сценарий);
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьАннотацииПолейЗагрузитьСценарийИзСтроки() Экспорт
	
	Сценарий = ЗагрузитьСценарийИзСтроки(ТекстСценария());

	ПроверитьАннотацииПоляСценария(Сценарий);

КонецПроцедуры

Функция ТекстСценария()

	Возврат
	"&АннотацияБезПараметра
	|&АннотацияСПараметром(""Значение"")
	|&АннотацияСИменованнымПараметром(ИмяПараметра = ""Значение"")
	|Перем Поле Экспорт;";

КонецФункции

Процедура ПроверитьАннотацииПоляСценария(Сценарий)
	
	Рефлектор = Новый Рефлектор();

	ТаблицаСвойств = Рефлектор.ПолучитьТаблицуСвойств(Сценарий);

	Если ТаблицаСвойств.Количество() <> 1 Тогда
		ВызватьИсключение "Ожидали, что в таблице свойств будет одно свойство а это не так";
	КонецЕсли;

	КоличествоАннотаций = ТаблицаСвойств[0].Аннотации.Количество();

	Если КоличествоАннотаций <> 3 Тогда
		ВызватьИсключение "Ожидали, что в таблице аннотаций свойства будет 3 аннотации а их там " + КоличествоАннотаций;
	КонецЕсли;
	
	ИмяАннотации = ТаблицаСвойств[0].Аннотации[0].Имя;

	Если ИмяАннотации <> "АннотацияБезПараметра" Тогда
		ВызватьИсключение "Ожидали, что первой аннотацией свойства будет АннотацияБезПараметра а там " + ИмяАннотации;
	КонецЕсли;

	ИмяАннотации = ТаблицаСвойств[0].Аннотации[1].Имя;

	Если ИмяАннотации <> "АннотацияСПараметром" Тогда
		ВызватьИсключение "Ожидали, что второй аннотацией свойства будет АннотацияСПараметром а там " + ИмяАннотации;
	КонецЕсли;

	КоличествоПараметров = ТаблицаСвойств[0].Аннотации[1].Параметры.Количество();

	Если КоличествоПараметров <> 1 Тогда
		ВызватьИсключение "Ожидали, что количество параметров второй аннотации будет равно 1 а их там " + КоличествоПараметров;
	КонецЕсли;

	ПараметрАннотации = ТаблицаСвойств[0].Аннотации[1].Параметры[0];

	Если ПараметрАннотации.Имя <> Неопределено Или ПараметрАннотации.Значение <> "Значение" Тогда

		ВызватьИсключение 
			"Ожидали, что у параметра второй аннотации будет имя Неопределено и строка Значение в поле значение, а там:
			| Имя = " + ПараметрАннотации.Имя + " Значение = " + ПараметрАннотации.Значение;

	КонецЕсли;

	ИмяАннотации = ТаблицаСвойств[0].Аннотации[2].Имя;

	Если ИмяАннотации <> "АннотацияСИменованнымПараметром" Тогда
		ВызватьИсключение "Ожидали, что третьей аннотацией свойства будет АннотацияСИменованнымПараметром а там " + ИмяАннотации;
	КонецЕсли;

	КоличествоПараметров = ТаблицаСвойств[0].Аннотации[2].Параметры.Количество();

	Если КоличествоПараметров <> 1 Тогда
		ВызватьИсключение "Ожидали, что количество параметров третьей аннотации будет равно 1 а их там " + КоличествоПараметров;
	КонецЕсли;

	ПараметрАннотации = ТаблицаСвойств[0].Аннотации[2].Параметры[0];

	Если ПараметрАннотации.Имя <> "ИмяПараметра" Или ПараметрАннотации.Значение <> "Значение" Тогда

		ВызватьИсключение 
			"Ожидали, что у параметра третьей аннотации будет имя ИмяПараметра и строка Значение в поле значение, а там:
			| Имя = " + ПараметрАннотации.Имя + " Значение = " + ПараметрАннотации.Значение;

	КонецЕсли;
	
КонецПроцедуры
