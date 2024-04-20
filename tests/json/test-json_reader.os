#Использовать asserts

Перем юТест;
Перем ТекстПроверки;

Функция ПолучитьСписокТестов(Тестирование) Экспорт

	юТест = Тестирование;

	СписокТестов = Новый Массив;
	
	СписокТестов.Добавить("Тест_Должен_ПроверитьПолучениеВсехЭлементовПеречисленияТипЗначенияJSON");
	СписокТестов.Добавить("Тест_Должен_СверитьСвойствоСДвойнымиКавычками");
	СписокТестов.Добавить("Тест_Должен_ПроверитьПреобразованиеNullВНеопределено");

	СписокТестов.Добавить("Тест_Должен_СверитьХешСуммуРезультатаПарсингаJSON");
	СписокТестов.Добавить("Тест_Должен_СверитьХешСуммуРезультатаПарсингаJSONВСтруктуру");
	СписокТестов.Добавить("Тест_Должен_СверитьХешСуммуРезультатаПарсингаJSONМассива");
	
	СписокТестов.Добавить("Тест_ДолженВызватьОшибку_ДляUndefined");
	Возврат СписокТестов;

КонецФункции

Процедура Тест_Должен_ПроверитьПолучениеВсехЭлементовПеречисленияТипЗначенияJSON() Экспорт

юТест.ПроверитьРавенство("Null", Строка(ТипЗначенияJSON.Null));
юТест.ПроверитьРавенство("Комментарий", Строка(ТипЗначенияJSON.Комментарий));
юТест.ПроверитьРавенство("ИмяСвойства", Строка(ТипЗначенияJSON.ИмяСвойства));
юТест.ПроверитьРавенство("КонецМассива", Строка(ТипЗначенияJSON.КонецМассива));
юТест.ПроверитьРавенство("КонецОбъекта", Строка(ТипЗначенияJSON.КонецОбъекта));
юТест.ПроверитьРавенство("НачалоМассива", Строка(ТипЗначенияJSON.НачалоМассива));
юТест.ПроверитьРавенство("НачалоОбъекта", Строка(ТипЗначенияJSON.НачалоОбъекта));
юТест.ПроверитьРавенство("Ничего", Строка(ТипЗначенияJSON.Ничего));
юТест.ПроверитьРавенство("Строка", Строка(ТипЗначенияJSON.Строка));
юТест.ПроверитьРавенство("Число", Строка(ТипЗначенияJSON.Число));

КонецПроцедуры

Процедура Тест_Должен_СверитьСвойствоСДвойнымиКавычками() Экспорт
	
	СтруктураДанных = ПолучитьСтруктуруДанных("json/json-mock_struct.json");
		
	юТест.ПроверитьРавенство(СтруктураДанных.lastName, """Иванов""");
	
КонецПроцедуры

Процедура Тест_Должен_ПроверитьПреобразованиеNullВНеопределено() Экспорт

	Чтение = Новый ЧтениеJSON();
	Чтение.УстановитьСтроку("{""Null"": null}");
	СтруктураДанных = ПрочитатьJSON(Чтение,Ложь);

	юТест.ПроверитьРавенство(ТипЗнч(СтруктураДанных.Null), Тип("Неопределено"));
	
КонецПроцедуры

Процедура Тест_Должен_СверитьХешСуммуРезультатаПарсингаJSON() Экспорт
	
	СтруктураДанных = ПолучитьСтруктуруДанных("json/json-mock.json", Истина);
		
	юТест.ПроверитьРавенство(РассчитатьХешСумму(СтруктураДанных), 960829385);
	
КонецПроцедуры

Процедура Тест_Должен_СверитьХешСуммуРезультатаПарсингаJSONВСтруктуру() Экспорт
	
	СтруктураДанных = ПолучитьСтруктуруДанных("json/json-mock_struct.json", Ложь);
		
	юТест.ПроверитьРавенство(РассчитатьХешСумму(СтруктураДанных), 2800700943);
	
КонецПроцедуры

Процедура Тест_Должен_СверитьХешСуммуРезультатаПарсингаJSONМассива() Экспорт
	
	СтруктураДанных = ПолучитьСтруктуруДанных("json/json-mock_array.json", Истина);
		
	юТест.ПроверитьРавенство(РассчитатьХешСумму(СтруктураДанных), 3633637665);
	
КонецПроцедуры

Функция ЗначениеВСтроку(Значение, Уровень = 0)
	
	Текст = "";
	Отступы = "";
	Для Счетчик = 1 По Уровень Цикл
		Отступы = Отступы + Символы.Таб;
	КонецЦикла;
	
	Если ТипЗнч(Значение) = Тип("Массив") Тогда
		Для Каждого Элемент Из Значение Цикл
			Если ТипЗнч(Элемент) = Тип("Структура") ИЛИ ТипЗнч(Элемент) = Тип("Соответствие") ИЛИ ТипЗнч(Элемент) = Тип("Массив") Тогда
				Текст = Текст + ЗначениеВСтроку(Элемент, Уровень + 1);
			Иначе
				Текст = Текст + Отступы + ?(Элемент = Undefined, "Undefined", Элемент) + Символы.ПС;
			КонецЕсли;			
		КонецЦикла;
	Иначе
		Для Каждого ТекСтрока Из Значение Цикл
			Если ТипЗнч(ТекСтрока.Значение) = Тип("Структура") ИЛИ ТипЗнч(ТекСтрока.Значение) = Тип("Соответствие") ИЛИ ТипЗнч(ТекСтрока.Значение) = Тип("Массив") Тогда				
				Текст = Текст + Отступы + ТекСтрока.Ключ + ":" + Символы.ПС + ЗначениеВСтроку(ТекСтрока.Значение, Уровень + 1);
			Иначе
				Текст = Текст + Отступы + ТекСтрока.Ключ + ":" + ?(ТекСтрока.Значение = Undefined, "Undefined", ТекСтрока.Значение) + Символы.ПС;
			КонецЕсли;
		КонецЦикла; 
	КонецЕсли; 
	Возврат Текст;
	
КонецФункции

Функция РассчитатьХешСумму(СтруктураДанных)

	Текст = ЗначениеВСтроку(СтруктураДанных);
	 
	Хеширование = Новый ХешированиеДанных(ХешФункция.CRC32);
	Хеширование.Добавить(Текст);
	Возврат Хеширование.ХешСумма;

КонецФункции


Функция ПолучитьСтруктуруДанных(ПутьКФайлу, КакСоответствие = Ложь)

	Текст = Новый ТекстовыйДокумент();
	Текст.Прочитать(ПутьКФайлу, КодировкаТекста.UTF8);
	
	Чтение = Новый ЧтениеJSON;
	Чтение.УстановитьСтроку(Текст.ПолучитьТекст());
	
	Результат = ПрочитатьJSON(Чтение, КакСоответствие);
	
Возврат Результат;
	
КонецФункции

Процедура Тест_ДолженВызватьОшибку_ДляUndefined() Экспорт
	Текст = "undefined";
	Чтение = Новый ЧтениеJSON;
	Чтение.УстановитьСтроку(Текст);
	Попытка
		Результат = Чтение.Прочитать();
	Исключение
		Возврат;
	КонецПопытки;
	ВызватьИсключение "Должно было быть выдано исключение, но его не было";
КонецПроцедуры
