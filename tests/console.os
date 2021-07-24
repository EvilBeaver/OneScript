Перем юТест;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт

	юТест = ЮнитТестирование;

	ВсеТесты = Новый Массив;
	ВсеТесты.Добавить("ТестДолжен_ПроверитьЧтоСтандартныйПотокВводаЭтоПоток");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПередачуДанныхВСкриптЧерезСтандартныйПотокВвода");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПеренаправлениеВывода");

	Возврат ВсеТесты;

КонецФункции

Процедура ТестДолжен_ПроверитьЧтоСтандартныйПотокВводаЭтоПоток() Экспорт
	
	ПотокВвода = Консоль.ОткрытьСтандартныйПотокВвода();
	юТест.ПроверитьРавенство(Тип("Поток"), ТипЗнч(ПотокВвода), "Стандартный поток ввода - Поток");

КонецПроцедуры

Процедура ТестДолжен_ПроверитьПередачуДанныхВСкриптЧерезСтандартныйПотокВвода() Экспорт

	СистемнаяИнформация = Новый СистемнаяИнформация;
	ЭтоWindows = Найти(НРег(СистемнаяИнформация.ВерсияОС), "windows") > 0;

	ПутьКОскрипт = ОбъединитьПути(КаталогПрограммы(), "oscript.exe");

	КодСкрипта = "Чтение = Новый ЧтениеТекста();
	             |Чтение.Открыть(Консоль.ОткрытьСтандартныйПотокВвода());
	             |Сообщить(СокрЛП(Чтение.Прочитать()));
	             |";

	ТекстСкрипта = Новый ТекстовыйДокумент();
	ТекстСкрипта.УстановитьТекст(КодСкрипта);

	ВремФайл = ПолучитьИмяВременногоФайла("os");

	ТекстСкрипта.Записать(ВремФайл);

	ТестовыеДанные = "12346";

	ИсполняемаяКоманда = СтрШаблон("echo %1 | %2 %3", ТестовыеДанные, ПутьКОскрипт, ВремФайл);

	Если ЭтоWindows Тогда
		ШаблонЗапуска = "cmd /c ""%1""";
	Иначе
		ШаблонЗапуска = "sh -c '%1'";
	КонецЕсли;

	ИсполняемаяКоманда = СтрШаблон(ШаблонЗапуска, ИсполняемаяКоманда);

	Процесс = СоздатьПроцесс(ИсполняемаяКоманда, , Истина, Истина);
	Процесс.Запустить();

	Пока НЕ Процесс.Завершен Цикл
		Приостановить(100);
		Если Процесс.ПотокВывода.ЕстьДанные Тогда
			Прервать;
		КонецЕсли;
	КонецЦикла;

	ВыводКоманды = СокрЛП(Процесс.ПотокВывода.Прочитать());
	УдалитьФайлы(ВремФайл);

	юТест.ПроверитьРавенство(ВыводКоманды, ТестовыеДанные, "Вывод команды - тестовые данные");

КонецПроцедуры

Процедура ТестДолжен_ПроверитьПеренаправлениеВывода() Экспорт
	
	ДобавитьОбработчик Консоль.CancelKeyPress, ЭтотОбъект.Обработчик;

	Сч = 0;
	Пока Сч < 20 Цикл
		Приостановить(1000);
		Сообщить(Сч);
		Сч = Сч + 1;
	КонецЦикла;

	ВФ = ПолучитьИмяВременногоФайла();
	Поток = ФайловыеПотоки.ОткрытьДляЗаписи(ВФ);
	Консоль.УстановитьПотокВывода(Поток);
	Попытка
		Сообщить("Привет мир!");
	Исключение
		// что-то пошло не так
		Консоль.УстановитьПотокВывода(Консоль.ОткрытьСтандартныйПотокВывода());
		ВызватьИсключение;
	КонецПопытки;
	
	Поток.Закрыть();
	Консоль.УстановитьПотокВывода(Консоль.ОткрытьСтандартныйПотокВывода());

	Чтение = Новый ЧтениеТекста(ВФ, Консоль.КодировкаВыходногоПотока);
	Текст = Чтение.Прочитать();
	Чтение.Закрыть();

	УдалитьФайлы(ВФ);

	юТест.ПроверитьРавенство("Привет мир!", СокрЛП(Текст));

КонецПроцедуры

Процедура Обработчик(Отказ) Экспорт
	Сообщить("Обработчик вызван");
	Отказ = Истина;
КонецПроцедуры