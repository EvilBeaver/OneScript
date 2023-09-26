#Использовать "."

Перем юТест;

Функция ПолучитьСписокТестов(Тестирование) Экспорт
    
    юТест = Тестирование;
    
    Тесты = Новый Массив;
    Тесты.Добавить("ТестДолжен_ПроверитьОберткуНадРегуляркой");
    
    Возврат Тесты;
    
КонецФункции

Процедура ТестДолжен_ПроверитьОберткуНадРегуляркой() Экспорт
    ИсходнаяСтрока = "{20221110000049,N,
    |{2444a6a24da10,3d},1,1,1,1803214,81,I,""Первое событие"",599,
    |{""U""},""Представление данных"",1,1,9,3,0,
    |{0}
    |},
    |{20221110000049,U,
    |{2444a6a24da10,3d},1,1,1,1803214,81,E,""Второе событие"",599,
    |{""U""},""Представление данных2"",1,1,9,3,0,
    |{2,1,31,2,31}
    |}";
    Регулярка =  "\,*\r*\n*\{(\d{14}),(\w),\r*\n\{([0-9a-f]+),([0-9a-f]+)\},(\d+),(\d+),(\d+),(\d+),(\d+),(\w),""([^ꡏ]*?)(?="",\d+,\r*\n)"",(\d+),\r*\n\{([^ꡏ]*?)(?=\},"")\},""([^ꡏ]*?)(?="",\d+)"",(\d+),(\d+),(\d+),(\d+),\d+[,\d+]*,\r*\n\{((\d+)|\d+,(\d+),(\d+),(\d+),(\d+))\}\r*\n\},*\r*\n*";	
    
    ОберткаНадРегуляркой = Новый ОберткаНадРегуляркой(Регулярка);
    
    Совпадения = ОберткаНадРегуляркой.НайтиСовпадения(ИсходнаяСтрока);
    Совпадение = Совпадения[0];
    
    юТест.ПроверитьРавенство(2, Совпадения.Количество());
КонецПроцедуры
