#native

Перем юТест;

Функция ПолучитьСписокТестов(Тестирование) Экспорт
    
    юТест = Тестирование;

    Тесты = Новый Массив;
    Тесты.Добавить("ТестДолжен_ПроверитьПоискВМассиве");
    
    Возврат Тесты;
    
КонецФункции

Процедура ТестДолжен_ПроверитьПоискВМассиве() Экспорт
    
    Массив = Новый Массив();
    Массив.Добавить("1");
    Массив.Добавить("2");
    Массив.Добавить("3");

    Если Массив.Найти("1") <> Неопределено Тогда
        НашлиЗначение1 = Истина;
    Иначе
        НашлиЗначение1 = Ложь;
    КонецЕсли;

    Если Массив.Найти("9") <> Неопределено Тогда
        НашлиЗначение9 = Истина;
    Иначе
        НашлиЗначение9 = Ложь;
    КонецЕсли;
    
    юТест.ПроверитьРавенство(НашлиЗначение1, Истина, "Нашли 1");
    юТест.ПроверитьРавенство(НашлиЗначение9, Ложь, "Не нашли 9");
    юТест.ПроверитьРавенство(Массив.Найти("2"), 1, "Индекс элемента по значению");

КонецПроцедуры
