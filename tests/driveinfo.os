#Использовать asserts

Функция ПолучитьСписокТестов(Тестирование) Экспорт
	
	СписокТестов = Новый Массив;
	СписокТестов.Добавить("Тест_Должен_ВернутьДоступноеМесто");
	СписокТестов.Добавить("Тест_Должен_ВернутьИмяФС");
	СписокТестов.Добавить("Тест_Должен_ВернутьТипДиска");
	СписокТестов.Добавить("Тест_Должен_ВернутьГотов");
	СписокТестов.Добавить("Тест_Должен_ВернутьИмя");
	СписокТестов.Добавить("Тест_Должен_ВернутьКорневойКаталог");
	СписокТестов.Добавить("Тест_Должен_ВернутьОбщийОбъемСвободногоМеста");
	СписокТестов.Добавить("Тест_Должен_ВернутьРазмерДиска");
	СписокТестов.Добавить("Тест_Должен_ВернутьМеткаТома");
	
	СписокТестов.Добавить("Тест_Должен_ВывестиЗначения");
	
	Возврат СписокТестов;
	
КонецФункции

Функция ПодключенныйДиск()
	Диски = Новый СистемнаяИнформация().ИменаЛогическихДисков;
	Для Каждого Диск из Диски Цикл
		ИнформацияОДиске = Новый ИнформацияОДиске(Диск);
		Если ИнформацияОДиске.Готов Тогда
			Возврат ИнформацияОДиске;
		КонецЕсли;
	КонецЦикла;
КонецФункции

Процедура Тест_Должен_ВернутьДоступноеМесто() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.Доступно).БольшеИлиРавно(0);
КонецПроцедуры

Процедура Тест_Должен_ВернутьИмяФС() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.ИмяФС).ЭтоНе().Равно("");
КонецПроцедуры

Процедура Тест_Должен_ВернутьТипДиска() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.ТипДиска).ЭтоНе().Равно("");
КонецПроцедуры

Процедура Тест_Должен_ВернутьГотов() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.Готов).ЭтоИстина();
КонецПроцедуры

Процедура Тест_Должен_ВернутьИмя() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.Имя).ЭтоНе().Равно("");
КонецПроцедуры

Процедура Тест_Должен_ВернутьКорневойКаталог() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.КорневойКаталог.Путь).ЭтоНе().Равно("");
КонецПроцедуры

Процедура Тест_Должен_ВернутьОбщийОбъемСвободногоМеста() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.ОбщийОбъемСвободногоМеста).БольшеИлиРавно(0);
КонецПроцедуры

Процедура Тест_Должен_ВернутьРазмерДиска() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.РазмерДиска).БольшеИлиРавно(0);
КонецПроцедуры

Процедура Тест_Должен_ВернутьМеткаТома() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Ожидаем.Что(ИнформацияОДиске.МеткаТома).Существует();
КонецПроцедуры

Процедура Тест_Должен_ВывестиЗначения() Экспорт
	ИнформацияОДиске = ПодключенныйДиск();
	Сообщить("Доступно: " + ИнформацияОДиске.Доступно);
	Сообщить("ИмяФС: " + ИнформацияОДиске.ИмяФС);
	Сообщить("ТипДиска: " + ИнформацияОДиске.ТипДиска);
	Сообщить("Готов: " + ИнформацияОДиске.Готов);
	Сообщить("Имя: " + ИнформацияОДиске.Имя);
	Сообщить("КорневойКаталог.Путь: " + ИнформацияОДиске.КорневойКаталог.Путь);
	Сообщить("ОбщийОбъемСвободногоМеста: " + ИнформацияОДиске.ОбщийОбъемСвободногоМеста);
	Сообщить("РазмерДиска: " + ИнформацияОДиске.РазмерДиска);
	Сообщить("МеткаТома: " + ИнформацияОДиске.МеткаТома);
КонецПроцедуры