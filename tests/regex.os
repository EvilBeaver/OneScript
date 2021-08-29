﻿
Перем юТест;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	
	ВсеТесты.Добавить("ТестДолжен_СоздатьОбъектРегулярноеВыражение");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьМетодСовпадает");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьМетодНайтиСовпадения");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПолучениеВложенныхГрупп");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПолучениеИменованныхГруппБезИменования");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПолучениеИменованныхГруппСИменованиемИСмешаннымПорядком");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьСвойство_ИгнорироватьРегистр");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьСвойство_Многострочный");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьМетодРазделить");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьМетодЗаменить");
	
	Возврат ВсеТесты;
КонецФункции

Процедура ТестДолжен_СоздатьОбъектРегулярноеВыражение() Экспорт
	РегулярноеВыражение = Новый РегулярноеВыражение("a");

	юТест.ПроверитьРавенство(ТипЗнч(РегулярноеВыражение), Тип("РегулярноеВыражение"), "РегулярноеВыражение");
КонецПроцедуры

Процедура ТестДолжен_ПроверитьМетодСовпадает() Экспорт
	РегулярноеВыражение = Новый РегулярноеВыражение("a");
	Совпало = РегулярноеВыражение.Совпадает("a");
	юТест.Проверить(Совпало, "Совпало");

	РегулярноеВыражение = Новый РегулярноеВыражение("\d\d");
	Совпало = РегулярноеВыражение.Совпадает("15");
	юТест.Проверить(Совпало, "Совпало 15");

	Совпало = РегулярноеВыражение.Совпадает("q");
	юТест.ПроверитьЛожь(Совпало, "Не Совпало q");
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьСвойство_ИгнорироватьРегистр() Экспорт
	ИсходнаяСтрока = "s";
	
	РегулярноеВыражение = Новый РегулярноеВыражение(ВРег(ИсходнаяСтрока));
	юТест.Проверить(РегулярноеВыражение.ИгнорироватьРегистр, "РегулярноеВыражение.ИгнорироватьРегистр");
	Совпало = РегулярноеВыражение.Совпадает(ИсходнаяСтрока);
	юТест.Проверить(Совпало, "Совпало");

	РегулярноеВыражение.ИгнорироватьРегистр = Ложь;
	юТест.ПроверитьЛожь(РегулярноеВыражение.ИгнорироватьРегистр, "РегулярноеВыражение.ИгнорироватьРегистр ложь");

	Совпало = РегулярноеВыражение.Совпадает(ИсходнаяСтрока);
	юТест.ПроверитьЛожь(Совпало, "Совпало");

КонецПроцедуры

Процедура ТестДолжен_ПроверитьСвойство_Многострочный() Экспорт
	ИсходнаяСтрока = "S"+Символы.ПС+"s";

	РегулярноеВыражение = Новый РегулярноеВыражение("^S");
	юТест.Проверить(РегулярноеВыражение.Многострочный, "РегулярноеВыражение.Многострочный");

	КоллекцияСовпадений = РегулярноеВыражение.НайтиСовпадения(ИсходнаяСтрока);
	юТест.ПроверитьРавенство(2, КоллекцияСовпадений.Количество(), "КоллекцияСовпадений.Количество()");

	РегулярноеВыражение.Многострочный = Ложь;
	юТест.ПроверитьЛожь(РегулярноеВыражение.Многострочный, "РегулярноеВыражение.Многострочный ложь");

	КоллекцияСовпадений = РегулярноеВыражение.НайтиСовпадения(ИсходнаяСтрока);
	юТест.ПроверитьРавенство(1, КоллекцияСовпадений.Количество(), "КоллекцияСовпадений.Количество() Многострочный ложь");

КонецПроцедуры

Процедура ТестДолжен_ПроверитьМетодНайтиСовпадения() Экспорт
	ИсходнаяСтрока = "456";
	РегулярноеВыражение = Новый РегулярноеВыражение("\d\d\d");
	КоллекцияСовпадений = РегулярноеВыражение.НайтиСовпадения("s" + ИсходнаяСтрока);
	юТест.ПроверитьРавенство(ТипЗнч(КоллекцияСовпадений), Тип("КоллекцияСовпаденийРегулярногоВыражения"), "КоллекцияСовпадений");
	юТест.ПроверитьРавенство(1, КоллекцияСовпадений.Количество(), "КоллекцияСовпадений.Количество()");

	Совпадение0 = КоллекцияСовпадений[0];
	
	юТест.ПроверитьРавенство(ТипЗнч(Совпадение0), Тип("СовпадениеРегулярногоВыражения"), "Совпадение0");
	УбедитьсяЧтоНашлиНужнуюСтроку(ИсходнаяСтрока, Совпадение0, "Совпадение0");

	УбедитьсяЧтоОбходКоллекцииРаботаетВерно(КоллекцияСовпадений, Совпадение0, "Совпадение");
КонецПроцедуры

Процедура ТестДолжен_ПроверитьПолучениеВложенныхГрупп() Экспорт
	ИсходнаяСтрока = "456";
	РегулярноеВыражение = Новый РегулярноеВыражение("(\d)(\d\d)");
	КоллекцияСовпадений = РегулярноеВыражение.НайтиСовпадения("s" + ИсходнаяСтрока);

	Совпадение0 = КоллекцияСовпадений[0];
	КоллекцияГрупп = Совпадение0.Группы;

	юТест.ПроверитьРавенство(ТипЗнч(КоллекцияГрупп), Тип("КоллекцияГруппРегулярногоВыражения"), "КоллекцияГрупп");
	юТест.ПроверитьРавенство(3, КоллекцияГрупп.Количество(), "КоллекцияГрупп.Количество()");
	
	Группа0 = КоллекцияГрупп[0];
	юТест.ПроверитьРавенство(ТипЗнч(Группа0), Тип("ГруппаРегулярногоВыражения"), "Группа0");
	УбедитьсяЧтоНашлиНужнуюСтроку(ИсходнаяСтрока, Группа0, "Группа0");
	
	Группа1 = КоллекцияГрупп[1];
	юТест.ПроверитьРавенство("4", Группа1.Значение, "Группа1.Значение");
	юТест.ПроверитьРавенство(1, Группа1.Длина, "Группа1.Длина");
	юТест.ПроверитьРавенство(1, Группа1.Индекс, "Группа1.Индекс");
	
	Группа2 = КоллекцияГрупп[2];
	юТест.ПроверитьРавенство("56", Группа2.Значение, "Группа2.Значение");
	юТест.ПроверитьРавенство(2, Группа2.Длина, "Группа2.Длина");
	юТест.ПроверитьРавенство(2, Группа2.Индекс, "Группа2.Индекс");

	УбедитьсяЧтоОбходКоллекцииРаботаетВерно(КоллекцияГрупп, Группа0, "Группа");
КонецПроцедуры

Процедура ТестДолжен_ПроверитьПолучениеИменованныхГруппБезИменования() Экспорт
	
	ИсходнаяСтрока = "123456";
	РегулярноеВыражение = Новый РегулярноеВыражение("(\d)(\d)(\d)");
	КоллекцияСовпадений = РегулярноеВыражение.НайтиСовпадения("s" + ИсходнаяСтрока);
	
	// получение имени через итератор
	Для Каждого Совпадение Из КоллекцияСовпадений Цикл
		Имя = 0;
		Для Каждого Группа Из Совпадение.Группы Цикл
			юТест.ПроверитьРавенство(Строка(Имя), Группа.Имя, "Группа.Имя");
			Имя = Имя + 1;
		КонецЦикла;
	КонецЦикла;
	
	// получение имени через группу по индексу
	Группы = КоллекцияСовпадений[1].Группы;
	юТест.ПроверитьРавенство(Строка(0), Группы[0].Имя, "Группа0.Имя");
	юТест.ПроверитьРавенство(Строка(1), Группы[1].Имя, "Группа1.Имя");
	юТест.ПроверитьРавенство(Строка(2), Группы[2].Имя, "Группа2.Имя");
	юТест.ПроверитьРавенство(Строка(3), Группы[3].Имя, "Группа3.Имя");

КонецПроцедуры

Процедура ТестДолжен_ПроверитьПолучениеИменованныхГруппСИменованиемИСмешаннымПорядком() Экспорт
	
	ИсходнаяСтрока = "123456";
	РегулярноеВыражение = Новый РегулярноеВыражение("(\d)(?<myname>\d)(\d)");
	КоллекцияСовпадений = РегулярноеВыражение.НайтиСовпадения("s" + ИсходнаяСтрока);
	
	Значения =	
	"123,1,3,0;1,1,1,1;3,3,1,2;2,2,1,myname;
	|456,4,3,0;4,4,1,1;6,6,1,2;5,5,1,myname";
	
	КонтрольныеЗначенияСовпадений = СтрРазделить(Значения, Символы.ПС);
	КонтрольныеЗначенияСовпаденийИндекс = 0;
	Для Каждого Совпадение Из КоллекцияСовпадений Цикл
		КонтрольныеЗначенияГрупп = СтрРазделить(КонтрольныеЗначенияСовпадений[КонтрольныеЗначенияСовпаденийИндекс], ";");
		КонтрольныеЗначенияГруппИндекс = 0;
		Для Каждого Группа Из Совпадение.Группы Цикл
			КонтрольныеЗначения = СтрРазделить(КонтрольныеЗначенияГрупп[КонтрольныеЗначенияГруппИндекс], ",");
			юТест.ПроверитьРавенство(Строка(КонтрольныеЗначения[0]), Группа.Значение, "Группа.Значение");
			юТест.ПроверитьРавенство(Число(КонтрольныеЗначения[1]),  Группа.Индекс,   "Группа.Индекс");
			юТест.ПроверитьРавенство(Число(КонтрольныеЗначения[2]),  Группа.Длина,    "Группа.Длина");
			юТест.ПроверитьРавенство(Строка(КонтрольныеЗначения[3]), Группа.Имя,      "Группа.Имя");
			КонтрольныеЗначенияГруппИндекс = КонтрольныеЗначенияГруппИндекс + 1;
		КонецЦикла;
		КонтрольныеЗначенияСовпаденийИндекс = КонтрольныеЗначенияСовпаденийИндекс + 1;
	КонецЦикла;
	
	// получение имени через группу по индексу
	Группы = КоллекцияСовпадений[1].Группы;
	юТест.ПроверитьРавенство(Строка(0), Группы[0].Имя, "Группа0.Имя");
	юТест.ПроверитьРавенство(Строка(1), Группы[1].Имя, "Группа1.Имя");
	юТест.ПроверитьРавенство(Строка(2), Группы[2].Имя, "Группа2.Имя");
	юТест.ПроверитьРавенство("myname",  Группы[3].Имя, "Группа3.Имя");
	
	// получение группы по имени
	// примечание: получается всегда новый объект
	ЛевЗнач = Группы[3];
	ПравЗнач = Группы.ПоИмени("myname");
	юТест.ПроверитьРавенство(ЛевЗнач.Значение, ПравЗнач.Значение, "Группы.ПоИмени()");
	юТест.ПроверитьРавенство(ЛевЗнач.Индекс,   ПравЗнач.Индекс,   "Группы.ПоИмени()");
	юТест.ПроверитьРавенство(ЛевЗнач.Длина,    ПравЗнач.Длина,    "Группы.ПоИмени()");
	юТест.ПроверитьРавенство(ЛевЗнач.Имя,      ПравЗнач.Имя,      "Группы.ПоИмени()");
	
КонецПроцедуры

Процедура УбедитьсяЧтоНашлиНужнуюСтроку(ИсходнаяСтрока, Совпадение, Представление)
	юТест.ПроверитьРавенство(ИсходнаяСтрока, Совпадение.Значение, Представление+".Значение");
	юТест.ПроверитьРавенство(СтрДлина(ИсходнаяСтрока), Совпадение.Длина, Представление+".Длина");
	юТест.ПроверитьРавенство(1, Совпадение.Индекс, Представление+".Индекс");
КонецПроцедуры

Процедура ТестДолжен_ПроверитьМетодРазделить() Экспорт
	ИсходнаяСтрока = "4 5";
	РегулярноеВыражение = Новый РегулярноеВыражение("\s");
	
	МассивСтрок = РегулярноеВыражение.Разделить(ИсходнаяСтрока);
	юТест.ПроверитьРавенство(ТипЗнч(МассивСтрок), Тип("Массив"), "МассивСтрок");
	юТест.ПроверитьРавенство(2, МассивСтрок.Количество(), "МассивСтрок.Количество()");

	юТест.ПроверитьРавенство("4", МассивСтрок[0], "МассивСтрок[0]");
	юТест.ПроверитьРавенство("5", МассивСтрок[1], "МассивСтрок[1]");
КонецПроцедуры

Процедура ТестДолжен_ПроверитьМетодЗаменить() Экспорт
	ИсходнаяСтрока = "456";
	СтрокаЗамены = "$3$2$1";
	РегулярноеВыражение = Новый РегулярноеВыражение("(\d)(\d)(\d)");
	
	Результат = РегулярноеВыражение.Заменить(ИсходнаяСтрока, СтрокаЗамены);
	юТест.ПроверитьРавенство(ТипЗнч(Результат), Тип("Строка"), "Результат");
	юТест.ПроверитьРавенство("654", Результат, "Результат");
КонецПроцедуры

Процедура УбедитьсяЧтоОбходКоллекцииРаботаетВерно(КоллекцияСовпадений, ЭлементДляСравнения, Представление)
	Для Каждого Элемент Из КоллекцияСовпадений Цикл
		Прервать;
	КонецЦикла;
	юТест.ПроверитьРавенство(ЭлементДляСравнения.Значение, Элемент.Значение, Представление+".Значение, "+ Представление + "_1.Значение");
	юТест.ПроверитьРавенство(ЭлементДляСравнения.Длина, Элемент.Длина, Представление+".Длина, " + Представление+"_1.Длина");
	юТест.ПроверитьРавенство(ЭлементДляСравнения.Индекс, Элемент.Индекс, Представление+".Индекс, " + Представление+"_1.Индекс");
КонецПроцедуры
