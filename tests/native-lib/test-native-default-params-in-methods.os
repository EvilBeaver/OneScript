#native

Перем юТест;

Функция ПолучитьСписокТестов(Тестирование) Экспорт
    
    юТест = Тестирование;

    Тесты = Новый Массив;
    Тесты.Добавить("ТестДолжен_ЗначенияВМетодахПоУмолчаниюСтрока");
    Тесты.Добавить("ТестДолжен_ЗначенияВМетодахПоУмолчаниюБулево");
    Тесты.Добавить("ТестДолжен_ЗначенияВМетодахПоУмолчаниюНеопределено");
    
    Возврат Тесты;
    
КонецФункции

Процедура ТестДолжен_ЗначенияВМетодахПоУмолчаниюСтрока() Экспорт
    
    Результат = МетодСтроки(,"Текст");

    юТест.ПроверитьРавенство(Результат, "Т1 Текст", "Текст сформирован корректно");

КонецПроцедуры

Процедура ТестДолжен_ЗначенияВМетодахПоУмолчаниюБулево() Экспорт
    
    Результат1 = МетодБулево();
    Результат2 = МетодБулево(Истина);
    Результат3 = МетодБулево(Ложь);

    юТест.ПроверитьРавенство(Результат1, 1, "параметр передан корректно");
    юТест.ПроверитьРавенство(Результат2, 1, "параметр передан корректно");
    юТест.ПроверитьРавенство(Результат3, 0, "параметр передан корректно");

КонецПроцедуры

Процедура ТестДолжен_ЗначенияВМетодахПоУмолчаниюНеопределено() Экспорт
    
    Результат1 = МетодНеопределено();
    Результат2 = МетодНеопределено(Неопределено);
    Результат3 = МетодНеопределено(Ложь);

    юТест.ПроверитьРавенство(Результат1, 1, "параметр передан корректно");
    юТест.ПроверитьРавенство(Результат2, 1, "параметр передан корректно");
    юТест.ПроверитьРавенство(Результат3, 0, "параметр передан корректно");

КонецПроцедуры

Функция МетодСтроки(Текст1 = "Т1 ", Текст2 = "Т2 ")
	Сообщить(Текст1);
	Сообщить(Текст2);
	Возврат Текст1 + Текст2;
КонецФункции

Функция МетодБулево(Булево = Истина)
	
	Если Булево = Истина Или Булево Тогда
		Возврат 1;
	Иначе 
		Возврат 0;
	КонецЕсли;

КонецФункции

Функция МетодНеопределено(Параметр = Неопределено)
	
	Если Параметр = Неопределено Тогда
		Возврат 1;
	Иначе 
		Возврат 0;
	КонецЕсли;

КонецФункции