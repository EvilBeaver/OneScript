﻿Перем юТест;
Перем ТекущийКаталогСохр;
Перем Чтение;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	
	ВсеТесты.Добавить("ТестДолжен_СоздатьАрхивЧерезКонструкторИмениФайла");
	ВсеТесты.Добавить("ТестДолжен_СоздатьАрхивЧерезКонструкторСНеобязательнымиПараметрами");
	ВсеТесты.Добавить("ТестДолжен_СоздатьАрхивЧерезМетодОткрыть");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивОдиночныйФайлБезПутей");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивОдиночныйСПолнымПутем");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивКаталогТестов");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивКаталогТестовПоМаске");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивКаталогСОтносительнымиПутями");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивВложенныйКаталогСОтносительнымиПутями");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивНесколькоВложенныхКаталоговСОтносительнымиПутями");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивФайлСОтносительнымиПутями");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиКаталогСПолнымИменем");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиФайлСПолнымИменем");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиКаталогСПолнымИменемНеИзТекущегоКаталога");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиВложенныйФайлСПолнымИменемНеИзТекущегоКаталога");
	ВсеТесты.Добавить("ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиФайлСПолнымИменемНеИзТекущегоКаталога");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьИзвлечениеБезПутей");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьИзвлечениеБезПутейДляОдиночногоЭлемента");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьРазмерИзвлеченногоОдиночногоЭлемента");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПарольАрхива");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьПарольРусскиеБуквыФайловАрхива");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьШифрованиеЗипАрхива");
	
	Возврат ВсеТесты;
	
КонецФункции

Функция СоздатьВременныйФайл(Знач Расширение = "tmp")
	Возврат юТест.ИмяВременногоФайла(Расширение);
КонецФункции

Процедура ПередЗапускомТеста() Экспорт
	ТекущийКаталогСохр = ТекущийКаталог();
КонецПроцедуры

Процедура ПослеЗапускаТеста() Экспорт
	УстановитьТекущийКаталог(ТекущийКаталогСохр);

	Если ЗначениеЗаполнено(Чтение) Тогда
		Чтение.Закрыть();		
	КонецЕсли;

	юТест.УдалитьВременныеФайлы();
КонецПроцедуры

Процедура ТестДолжен_СоздатьАрхивЧерезКонструкторИмениФайла() Экспорт
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла(ИмяАрхива);
	Архив.Записать();
	
	Файл = Новый Файл(ИмяАрхива);
	юТест.ПроверитьИстину(Файл.Существует());
	
КонецПроцедуры

Процедура ТестДолжен_СоздатьАрхивЧерезКонструкторСНеобязательнымиПараметрами() Экспорт
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла(ИмяАрхива, "123", 456);
	Архив.Записать();
	
	Файл = Новый Файл(ИмяАрхива);
	юТест.ПроверитьИстину(Файл.Существует());
	
КонецПроцедуры

Процедура ТестДолжен_СоздатьАрхивЧерезМетодОткрыть() Экспорт
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла();
	Архив.Открыть(ИмяАрхива,,"Это комментарий",,УровеньСжатияZip.Максимальный);
	Архив.Записать();
	
	Файл = Новый Файл(ИмяАрхива);
	юТест.ПроверитьИстину(Файл.Существует());
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивОдиночныйФайлБезПутей() Экспорт
	
	ФайлСкрипта = ТекущийСценарий().Источник;
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла();
	Архив.Открыть(ИмяАрхива,,"Это комментарий",,УровеньСжатияZip.Максимальный);
	Архив.Добавить(ФайлСкрипта, РежимСохраненияПутейZip.НеСохранятьПути);
	Архив.Записать();
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	
	Попытка
		юТест.ПроверитьРавенство("", Чтение.Элементы[0].Путь);
		юТест.ПроверитьРавенство("zip.os", Чтение.Элементы[0].Имя);
		юТест.ПроверитьРавенство("zip", Чтение.Элементы[0].ИмяБезРасширения);
	Исключение	
		Чтение.Закрыть();
		ВызватьИсключение;
	КонецПопытки;
	
	Чтение.Закрыть();
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивОдиночныйСПолнымПутем() Экспорт
	
	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла();
	Архив.Открыть(ИмяАрхива);
	Архив.Добавить(ФайлСкрипта.ПолноеИмя, РежимСохраненияПутейZip.СохранятьПолныеПути);
	Архив.Записать();
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	
	СИ = Новый СистемнаяИнформация;
	Если Найти(СИ.ВерсияОС, "Windows") > 0 Тогда
		ИмяБезДиска = Сред(ФайлСкрипта.Путь, Найти(ФайлСкрипта.Путь, "\")+1);
	Иначе
		ИмяБезДиска = Сред(ФайлСкрипта.Путь,2);
	КонецЕсли;
	
	Попытка
		юТест.ПроверитьРавенство(ИмяБезДиска, Чтение.Элементы[0].Путь);
	Исключение	
		Чтение.Закрыть();
		УдалитьФайлы(ИмяАрхива);
		ВызватьИсключение;
	КонецПопытки;
	
	Чтение.Закрыть();
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивКаталогТестов() Экспорт

	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);
	КаталогСкрипта = Новый Файл(ФайлСкрипта.Путь);
	
	ВременныйКаталог = СоздатьВременныйФайл();
	КаталогКопииТестов = ОбъединитьПути(ВременныйКаталог, КаталогСкрипта.Имя);
	СоздатьКаталог(КаталогКопииТестов);
	
	ВсеФайлы = НайтиФайлы(КаталогСкрипта.ПолноеИмя, "*.os");
	Для Каждого Файл Из ВсеФайлы Цикл
		Если Файл.ЭтоФайл() Тогда
			КопироватьФайл(Файл.ПолноеИмя, ОбъединитьПути(КаталогКопииТестов, Файл.Имя));
		КонецЕсли;
	КонецЦикла;
		
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла();
	Архив.Открыть(ИмяАрхива);
	
	Архив.Добавить(КаталогКопииТестов + ПолучитьРазделительПути(), РежимСохраненияПутейZip.СохранятьОтносительныеПути, РежимОбработкиПодкаталоговZIP.ОбрабатыватьРекурсивно);
	Архив.Записать();
	
	ОжидаемоеИмя = КаталогСкрипта.Имя + ПолучитьРазделительПути();
	Попытка
		Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
		Для Каждого Элемент Из Чтение.Элементы Цикл
			юТест.ПроверитьРавенство(ОжидаемоеИмя, Элемент.Путь, "Проверка элемента zip: " + Элемент.ПолноеИмя);
		КонецЦикла;
	Исключение
		Чтение.Закрыть();
		ВызватьИсключение;
	КонецПопытки;
	юТест.ПроверитьРавенство(ВсеФайлы.Количество(), Чтение.Элементы.Количество());
	Чтение.Закрыть();
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивКаталогТестовПоМаске() Экспорт

	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);
	КаталогСкрипта = Новый Файл(ФайлСкрипта.Путь);
	
	ВременныйКаталог = СоздатьВременныйФайл();
	КаталогКопииТестов = ОбъединитьПути(ВременныйКаталог, КаталогСкрипта.Имя);
	СоздатьКаталог(КаталогКопииТестов);
	ВсеФайлы = НайтиФайлы(КаталогСкрипта.ПолноеИмя, "*.*");
	РасширениеТестов = ".os";
	КоличествоТестов = 0;
	
	ДопК = ОбъединитьПути(КаталогКопииТестов, "add");
	СоздатьКаталог(ДопК);
	
	Для Каждого Файл Из ВсеФайлы Цикл
		Если Файл.ЭтоКаталог() Тогда
			Продолжить;
		КонецЕсли;
		
		Если Файл.Расширение = РасширениеТестов Тогда
			КоличествоТестов = КоличествоТестов + 2;
		КонецЕсли;
		КопироватьФайл(Файл.ПолноеИмя, ОбъединитьПути(КаталогКопииТестов, Файл.Имя));
		КопироватьФайл(Файл.ПолноеИмя, ОбъединитьПути(ДопК, Файл.Имя));
	КонецЦикла;
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла(ИмяАрхива);
	Архив.Добавить(ВременныйКаталог + ПолучитьРазделительПути() + "*.os", РежимСохраненияПутейZip.СохранятьОтносительныеПути, РежимОбработкиПодкаталоговZIP.ОбрабатыватьРекурсивно);
	Архив.Записать();
	
	ОжидаемоеИмяКорень = КаталогСкрипта.Имя + ПолучитьРазделительПути();
	ОжидаемоеИмяДоп = ОбъединитьПути(КаталогСкрипта.Имя, "add") + ПолучитьРазделительПути();
	Попытка
		Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
		Для Каждого Элемент Из Чтение.Элементы Цикл
			юТест.Проверить(Элемент.Путь = ОжидаемоеИмяКорень или Элемент.Путь = ОжидаемоеИмяДоп, "Проверка для пути: " + Элемент.Путь);
			юТест.ПроверитьРавенство(РасширениеТестов, Элемент.Расширение);
		КонецЦикла;
	Исключение
		Чтение.Закрыть();
		ВызватьИсключение;
	КонецПопытки;
	
	юТест.ПроверитьИстину(КоличествоТестов > 0);
	юТест.ПроверитьРавенство(КоличествоТестов, Чтение.Элементы.Количество());
	
	Чтение.Закрыть();
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьИзвлечениеБезПутей() Экспорт
	
	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);
	КаталогСкрипта = Новый Файл(ФайлСкрипта.Путь);
	
	ВременныйКаталог = СоздатьВременныйФайл();
	КаталогКопииТестов = ОбъединитьПути(ВременныйКаталог, КаталогСкрипта.Имя);
	СоздатьКаталог(КаталогКопииТестов);
	ВсеФайлы = НайтиФайлы(КаталогСкрипта.ПолноеИмя, "*.os");
	Для Каждого Файл Из ВсеФайлы Цикл
		Если Файл.ЭтоФайл() Тогда
			КопироватьФайл(Файл.ПолноеИмя, ОбъединитьПути(КаталогКопииТестов, Файл.Имя));
		КонецЕсли;
	КонецЦикла;
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла();
	Архив.Открыть(ИмяАрхива);
	Архив.Добавить(ВременныйКаталог,РежимСохраненияПутейZip.СохранятьОтносительныеПути,РежимОбработкиПодкаталоговZIP.ОбрабатыватьРекурсивно);
	Архив.Записать();
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	КаталогИзвлечения = СоздатьВременныйФайл();
	СоздатьКаталог(КаталогИзвлечения);
	Чтение.ИзвлечьВсе(КаталогИзвлечения, РежимВосстановленияПутейФайловZIP.НеВосстанавливать);
	Чтение.Закрыть();
	ИзвлеченныеФайлы = НайтиФайлы(КаталогИзвлечения, "*.*");
	
	юТест.ПроверитьНеравенство(0, ИзвлеченныеФайлы.Количество());
	юТест.ПроверитьРавенство(ВсеФайлы.Количество(), ИзвлеченныеФайлы.Количество());
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьИзвлечениеБезПутейДляОдиночногоЭлемента() Экспорт
	
	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);
	КаталогСкрипта = Новый Файл(ФайлСкрипта.Путь);
	
	ВременныйКаталог = СоздатьВременныйФайл();
	КаталогКопииТестов = ОбъединитьПути(ВременныйКаталог, КаталогСкрипта.Имя);
	СоздатьКаталог(КаталогКопииТестов);
	ВсеФайлы = НайтиФайлы(КаталогСкрипта.ПолноеИмя, "*.os");
	Для Каждого Файл Из ВсеФайлы Цикл
		Если Файл.ЭтоФайл() Тогда
			КопироватьФайл(Файл.ПолноеИмя, ОбъединитьПути(КаталогКопииТестов, Файл.Имя));
		КонецЕсли;
	КонецЦикла;
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла();
	Архив.Открыть(ИмяАрхива);
	Архив.Добавить(ВременныйКаталог,РежимСохраненияПутейZip.СохранятьОтносительныеПути,РежимОбработкиПодкаталоговZIP.ОбрабатыватьРекурсивно);
	Архив.Записать();
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	КаталогИзвлечения = СоздатьВременныйФайл();
	СоздатьКаталог(КаталогИзвлечения);
	Элемент = Чтение.Элементы.Найти(ФайлСкрипта.Имя);
	
	Чтение.Извлечь(Элемент, КаталогИзвлечения, РежимВосстановленияПутейФайловZIP.НеВосстанавливать);
	Чтение.Закрыть();
	ИзвлеченныеФайлы = НайтиФайлы(КаталогИзвлечения, "*.os");
	
	юТест.ПроверитьНеравенство(0, ИзвлеченныеФайлы.Количество());
	юТест.ПроверитьРавенство(ФайлСкрипта.Имя, ИзвлеченныеФайлы[0].Имя);
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьРазмерИзвлеченногоОдиночногоЭлемента() Экспорт
	
	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла();
	Архив.Открыть(ИмяАрхива);
	Архив.Добавить(ФайлСкрипта.ПолноеИмя);
	Архив.Записать();
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	КаталогИзвлечения = СоздатьВременныйФайл();
	СоздатьКаталог(КаталогИзвлечения);
	Элемент = Чтение.Элементы.Найти(ФайлСкрипта.Имя);
	Чтение.Извлечь(Элемент, КаталогИзвлечения, РежимВосстановленияПутейФайловZIP.НеВосстанавливать);
	Чтение.Закрыть();

	ИзвлеченныйФайл = Новый Файл(ОбъединитьПути(КаталогИзвлечения, ФайлСкрипта.Имя));
	
	юТест.ПроверитьРавенство(ФайлСкрипта.Размер(), ИзвлеченныйФайл.Размер());
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьПарольАрхива() Экспорт
	
	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);

	Пароль = "password";
	
	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла(ИмяАрхива, Пароль);
	Архив.Добавить(ФайлСкрипта.ПолноеИмя);
	Архив.Записать();
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива, Пароль);
	КаталогИзвлечения = СоздатьВременныйФайл();
	СоздатьКаталог(КаталогИзвлечения);
	Элемент = Чтение.Элементы.Найти(ФайлСкрипта.Имя);
	Чтение.Извлечь(Элемент, КаталогИзвлечения, РежимВосстановленияПутейФайловZIP.НеВосстанавливать);
	Чтение.Закрыть();

	ИзвлеченныйФайл = Новый Файл(ОбъединитьПути(КаталогИзвлечения, ФайлСкрипта.Имя));
	
	юТест.ПроверитьРавенство(ФайлСкрипта.Размер(), ИзвлеченныйФайл.Размер());
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьПарольРусскиеБуквыФайловАрхива() Экспорт
	
	ФайлСкрипта = Новый Файл(ТекущийСценарий().Источник);

	ИмяРусскогоФайла = "ОбщиеФункции.os";

	ПутьРусскогоФайла = ОбъединитьПути(ФайлСкрипта.Путь, "testlib", ИмяРусскогоФайла);

	ИмяАрхива = СоздатьВременныйФайл("zip");
	Архив = Новый ЗаписьZipФайла(ИмяАрхива);
	Архив.Добавить(ПутьРусскогоФайла);
	Архив.Записать();
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	КаталогИзвлечения = СоздатьВременныйФайл();
	СоздатьКаталог(КаталогИзвлечения);
	Элемент = Чтение.Элементы.Найти(ИмяРусскогоФайла);
	Чтение.Извлечь(Элемент, КаталогИзвлечения, РежимВосстановленияПутейФайловZIP.НеВосстанавливать);
	Чтение.Закрыть();

	ИзвлеченныйФайл = Новый Файл(ОбъединитьПути(КаталогИзвлечения, ИмяРусскогоФайла));
	
	юТест.ПроверитьИстину(ИзвлеченныйФайл.Существует());
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивКаталогСОтносительнымиПутями() Экспорт
	ДобавитьВАрхивСОтносительнымиПутями_КаталогИлиФайл(Истина, Истина);	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивФайлСОтносительнымиПутями() Экспорт
	ДобавитьВАрхивСОтносительнымиПутями_КаталогИлиФайл(Ложь, Истина);
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиКаталогСПолнымИменем() Экспорт
	ДобавитьВАрхивСОтносительнымиПутями_КаталогИлиФайл(Истина, Ложь);
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиФайлСПолнымИменем() Экспорт
	ДобавитьВАрхивСОтносительнымиПутями_КаталогИлиФайл(Ложь, Ложь);
КонецПроцедуры

Процедура ДобавитьВАрхивСОтносительнымиПутями_КаталогИлиФайл(Знач ПередатьКаталог, Знач ПередатьОтносительныйПуть) Экспорт
	УстановитьВременныйКаталогКакТекущий();
	
	ИМЯ_КАТАЛОГА = "РодительскийКаталог";
	ИМЯ_ФАЙЛА = "ВложенныйФайл.txt";

	ОписаниеКаталога = ПодготовитьФайлВоВложенномКаталоге(ИМЯ_КАТАЛОГА, ИМЯ_ФАЙЛА);
	ОтносительныйПутьКаталога = ОписаниеКаталога.ПутьКаталога;
	ОтносительныйПутьФайла = ОписаниеКаталога.ПутьФайла;
	ПолныйПутьКаталога = ОбъединитьПути(ТекущийКаталог(), ОписаниеКаталога.ПутьКаталога);
	ПолныйПутьФайла = ОбъединитьПути(ТекущийКаталог(), ОписаниеКаталога.ПутьФайла);

	Если ПередатьКаталог Тогда
		Если ПередатьОтносительныйПуть Тогда
			Путь = ОтносительныйПутьКаталога;
		Иначе
			Путь = ПолныйПутьКаталога;
		КонецЕсли;
	Иначе
		Если ПередатьОтносительныйПуть Тогда
			Путь = ОтносительныйПутьФайла;
		Иначе
			Путь = ПолныйПутьФайла;
		КонецЕсли;
	КонецЕсли;

	ИмяАрхива = ДобавитьФайлИлиКаталогВАрхивСОтносительнымиПутями(Путь);
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	
	юТест.ПроверитьРавенство(Чтение.Элементы.Количество(), 1, "Количество элементов zip: ");
	Элемент = Чтение.Элементы[0];
	юТест.ПроверитьРавенство(СтрЗаменить(ОтносительныйПутьФайла, "\",  "/"), 
		Элемент.ПолноеИмя, "Проверка элемента zip: " + Элемент.ПолноеИмя);

	Распаковка = ИзвлечьВсеИзАрхиваВоВременныйКаталог(ИмяАрхива);
	
	НеверныйПутьФайлаВКорне = ОбъединитьПути(Распаковка, ИМЯ_ФАЙЛА);
	ПроверитьОтсутствиеФайла(НеверныйПутьФайлаВКорне, "Файл не должен был существовать в корне распаковки, а он есть.");
	
	ИскомыйПутьКаталога = ОбъединитьПути(Распаковка, ОтносительныйПутьКаталога);
	ПроверитьНаличиеФайла(ИскомыйПутьКаталога, "Каталог должен был существовать в корне, а его нет.");
	
	ИскомыйПутьФайла = ОбъединитьПути(Распаковка, ОтносительныйПутьФайла);
	ПроверитьНаличиеФайла(ИскомыйПутьФайла, "Файл должен был существовать во вложенном каталоге, а его нет.");
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивВложенныйКаталогСОтносительнымиПутями() Экспорт
	УстановитьВременныйКаталогКакТекущий();
	
	ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА = "РодительскийКаталог";
	ИМЯ_КАТАЛОГА = "ВложенныйКаталог";
	ИМЯ_ФАЙЛА = "ВложенныйФайл.txt";

	СоздатьКаталог(ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА);
	
	ОписаниеКаталога = ПодготовитьФайлВоВложенномКаталоге(ОбъединитьПути(ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА, ИМЯ_КАТАЛОГА), ИМЯ_ФАЙЛА);
	ОтносительныйПутьКаталога = ОписаниеКаталога.ПутьКаталога;
	ОтносительныйПутьФайла = ОписаниеКаталога.ПутьФайла;
	
	ИмяАрхива = ДобавитьФайлИлиКаталогВАрхивСОтносительнымиПутями(ОтносительныйПутьКаталога);
	Распаковка = ИзвлечьВсеИзАрхиваВоВременныйКаталог(ИмяАрхива);

	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);

	НеверныйПутьКаталогаВКорне = ОбъединитьПути(Распаковка, ИМЯ_КАТАЛОГА);
	ПроверитьОтсутствиеФайла(НеверныйПутьКаталогаВКорне, "Вложенный каталог не должен был существовать в корне, а он есть.");
	
	НеверныйПутьФайлаВКорне = ОбъединитьПути(Распаковка, ИМЯ_ФАЙЛА);
	ПроверитьОтсутствиеФайла(НеверныйПутьФайлаВКорне, "Файл не должен был существовать в корне распаковки, а он есть.");
	
	ИскомыйПутьРодительскогоКаталога = ОбъединитьПути(Распаковка, ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА);
	ПроверитьНаличиеФайла(ИскомыйПутьРодительскогоКаталога, "Родительский каталог должен был существовать в корне, а его нет.");
	
	ИскомыйПутьКаталога = ОбъединитьПути(Распаковка, ОтносительныйПутьКаталога);
	ПроверитьНаличиеФайла(ИскомыйПутьКаталога, "Вложенный каталог должен был существовать в каталоге родителя, а его нет.");
	
	ИскомыйПутьФайла = ОбъединитьПути(Распаковка, ОтносительныйПутьФайла);
	ПроверитьНаличиеФайла(ИскомыйПутьФайла, "Файл должен был существовать во вложенном каталоге, а его нет.");
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивНесколькоВложенныхКаталоговСОтносительнымиПутями() Экспорт
	УстановитьВременныйКаталогКакТекущий();
	
	ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА = "РодительскийКаталог";
	ИМЯ_КАТАЛОГА = "ВложенныйКаталог";
	ИМЯ_КАТАЛОГА2 = "ВложенныйКаталог2";
	ИМЯ_ФАЙЛА = "ВложенныйФайл.txt";
	ИМЯ_ФАЙЛА2 = "ВложенныйФайл2.txt";

	СоздатьКаталог(ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА);
	
	ОписаниеКаталога = ПодготовитьФайлВоВложенномКаталоге(ОбъединитьПути(ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА, ИМЯ_КАТАЛОГА), ИМЯ_ФАЙЛА);
	ОтносительныйПутьКаталога = ОписаниеКаталога.ПутьКаталога;
	ОтносительныйПутьФайла = ОписаниеКаталога.ПутьФайла;
	
	ОписаниеКаталога2 = ПодготовитьФайлВоВложенномКаталоге(ОбъединитьПути(ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА, ИМЯ_КАТАЛОГА2), ИМЯ_ФАЙЛА2);
	ОтносительныйПутьКаталога2 = ОписаниеКаталога2.ПутьКаталога;
	ОтносительныйПутьФайла2 = ОписаниеКаталога2.ПутьФайла;
	
	МассивПутей = Новый Массив;
	МассивПутей.Добавить(ОтносительныйПутьКаталога);
	МассивПутей.Добавить(ОтносительныйПутьКаталога2);

	ИмяАрхива = ДобавитьКоллекциюФайловИлиКаталоговВАрхивСОтносительнымиПутями(МассивПутей);
	Распаковка = ИзвлечьВсеИзАрхиваВоВременныйКаталог(ИмяАрхива);

	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);
	
	НеверныйПутьКаталогаВКорне = ОбъединитьПути(Распаковка, ИМЯ_КАТАЛОГА);
	ПроверитьОтсутствиеФайла(НеверныйПутьКаталогаВКорне, "Вложенный каталог не должен был существовать в корне, а он есть.");
	
	НеверныйПутьФайлаВКорне = ОбъединитьПути(Распаковка, ИМЯ_ФАЙЛА);
	ПроверитьОтсутствиеФайла(НеверныйПутьФайлаВКорне, "Файл не должен был существовать в корне распаковки, а он есть.");

	НеверныйПутьКаталогаВКорне2 = ОбъединитьПути(Распаковка, ИМЯ_КАТАЛОГА2);
	ПроверитьОтсутствиеФайла(НеверныйПутьКаталогаВКорне2, "Второй вложенный каталог не должен был существовать в корне, а он есть.");
	
	НеверныйПутьФайлаВКорне2 = ОбъединитьПути(Распаковка, ИМЯ_ФАЙЛА2);
	ПроверитьОтсутствиеФайла(НеверныйПутьФайлаВКорне2, "Второй файл не должен был существовать в корне распаковки, а он есть.");
	
	ИскомыйПутьРодительскогоКаталога = ОбъединитьПути(Распаковка, ИМЯ_РОДИТЕЛЬСКОГО_КАТАЛОГА);
	ПроверитьНаличиеФайла(ИскомыйПутьРодительскогоКаталога, "Родительский каталог должен был существовать в корне, а его нет.");
	
	ИскомыйПутьКаталога = ОбъединитьПути(Распаковка, ОтносительныйПутьКаталога);
	ПроверитьНаличиеФайла(ИскомыйПутьКаталога, "Вложенный каталог должен был существовать в каталоге родителя, а его нет.");
	
	ИскомыйПутьФайла = ОбъединитьПути(Распаковка, ОтносительныйПутьФайла);
	ПроверитьНаличиеФайла(ИскомыйПутьФайла, "Файл должен был существовать во вложенном каталоге, а его нет.");

	ИскомыйПутьКаталога2 = ОбъединитьПути(Распаковка, ОтносительныйПутьКаталога2);
	ПроверитьНаличиеФайла(ИскомыйПутьКаталога2, "Второй вложенный каталог должен был существовать в каталоге родителя, а его нет.");
	
	ИскомыйПутьФайла2 = ОбъединитьПути(Распаковка, ОтносительныйПутьФайла2);
	ПроверитьНаличиеФайла(ИскомыйПутьФайла2, "Второй файл должен был существовать во втором вложенном каталоге, а его нет.");
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиКаталогСПолнымИменемНеИзТекущегоКаталога() Экспорт
	ДобавитьВАрхивСОтносительнымиПутямиФайлИлиКаталогСПолнымИменемНеИзТекущегоКаталога(Истина);
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиВложенныйФайлСПолнымИменемНеИзТекущегоКаталога() Экспорт
	ДобавитьВАрхивСОтносительнымиПутямиФайлИлиКаталогСПолнымИменемНеИзТекущегоКаталога(Ложь);
КонецПроцедуры

Процедура ДобавитьВАрхивСОтносительнымиПутямиФайлИлиКаталогСПолнымИменемНеИзТекущегоКаталога(Знач ПередатьКаталог) Экспорт
	УстановитьВременныйКаталогКакТекущий();
	
	ИМЯ_КАТАЛОГА = "ДругойРодительскийКаталог";
	ИМЯ_ФАЙЛА = "ДругойВложенныйФайл.txt";

	ОписаниеКаталога = ПодготовитьФайлВоВложенномКаталоге(ИМЯ_КАТАЛОГА, ИМЯ_ФАЙЛА);
	ОтносительныйПутьКаталога = ОписаниеКаталога.ПутьКаталога;
	ОтносительныйПутьФайла = ОписаниеКаталога.ПутьФайла;
	ПолныйПутьКаталога = ОбъединитьПути(ТекущийКаталог(), ОписаниеКаталога.ПутьКаталога);
	ПолныйПутьФайла = ОбъединитьПути(ТекущийКаталог(), ОписаниеКаталога.ПутьФайла);

	УстановитьТекущийКаталог(ТекущийКаталогСохр);
	
	Если ПередатьКаталог Тогда
		Путь = ПолныйПутьКаталога;
	Иначе
		Путь = ПолныйПутьФайла;
	КонецЕсли;
	
	ИмяАрхива = ДобавитьФайлИлиКаталогВАрхивСОтносительнымиПутями(Путь);
	
	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);

	юТест.ПроверитьРавенство(Чтение.Элементы.Количество(), 1, "Количество элементов zip: ");
	Элемент = Чтение.Элементы[0];
	Если ПередатьКаталог Тогда
		юТест.ПроверитьРавенство(ОтносительныйПутьКаталога + ПолучитьРазделительПути(), Элемент.Путь, "Проверка элемента zip: " + Элемент.ПолноеИмя);
	Иначе
		юТест.ПроверитьРавенство(ИМЯ_ФАЙЛА, Элемент.ПолноеИмя, "Проверка элемента zip: " + Элемент.ПолноеИмя);
		юТест.ПроверитьРавенство("", Элемент.Путь, "Проверка пути элемента zip: " + Элемент.ПолноеИмя);
	КонецЕсли;
	
	Распаковка = ИзвлечьВсеИзАрхиваВоВременныйКаталог(ИмяАрхива);
	
	Если ПередатьКаталог Тогда
		НеверныйПутьФайлаВКорне = ОбъединитьПути(Распаковка, ИМЯ_ФАЙЛА);
		ПроверитьОтсутствиеФайла(НеверныйПутьФайлаВКорне, "Файл не должен был существовать в корне распаковки, а он есть.");
	
		ИскомыйПутьКаталога = ОбъединитьПути(Распаковка, ОтносительныйПутьКаталога);
		ПроверитьНаличиеФайла(ИскомыйПутьКаталога, "Каталог должен был существовать в корне, а его нет.");
		
		ИскомыйПутьФайла = ОбъединитьПути(Распаковка, ОтносительныйПутьФайла);
		ПроверитьНаличиеФайла(ИскомыйПутьФайла, "Файл должен был существовать во вложенном каталоге, а его нет.");
	Иначе
		ИскомыйПутьФайла = ОбъединитьПути(Распаковка, ИМЯ_ФАЙЛА);
		ПроверитьНаличиеФайла(ИскомыйПутьФайла, "Файл должен был существовать в корне распаковки, а его нет.");
	КонецЕсли;
	
КонецПроцедуры

Процедура ТестДолжен_ДобавитьВАрхивСОтносительнымиПутямиФайлСПолнымИменемНеИзТекущегоКаталога() Экспорт
	УстановитьВременныйКаталогКакТекущий();
	
	ИМЯ_ФАЙЛА = "ВложенныйФайл.txt";

	ОписаниеКаталога = ПодготовитьФайлВоВложенномКаталоге("", ИМЯ_ФАЙЛА);
	ОтносительныйПутьФайла = ОписаниеКаталога.ПутьФайла;
	ПолныйПутьФайла = ОбъединитьПути(ТекущийКаталог(), ОписаниеКаталога.ПутьФайла);
	УстановитьТекущийКаталог(ТекущийКаталогСохр);

	ИмяАрхива = ДобавитьФайлИлиКаталогВАрхивСОтносительнымиПутями(ПолныйПутьФайла);

	Чтение = Новый ЧтениеZipФайла(ИмяАрхива);

	юТест.ПроверитьРавенство(Чтение.Элементы.Количество(), 1, "Количество элементов zip: ");
	Элемент = Чтение.Элементы[0];
	юТест.ПроверитьРавенство(ИМЯ_ФАЙЛА, Элемент.ПолноеИмя, "Проверка элемента zip: " + Элемент.ПолноеИмя);
		
	Распаковка = ИзвлечьВсеИзАрхиваВоВременныйКаталог(ИмяАрхива);
	
	ИскомыйПутьФайла = ОбъединитьПути(Распаковка, ИМЯ_ФАЙЛА);
	ПроверитьНаличиеФайла(ИскомыйПутьФайла, "Файл должен был существовать в корне распаковки, а его нет.");
КонецПроцедуры

Процедура ТестДолжен_ПроверитьШифрованиеЗипАрхива() Экспорт

	Архивируемый = СоздатьФайл();
	Архив = ПолучитьИмяВременногоФайла("zip");
	ЗаписьZipФайла = Новый ЗаписьZipФайла(Архив, "123", , , , МетодШифрованияZIP.AES256);
	ЗаписьZipФайла.Добавить(Архивируемый);
	ЗаписьZipФайла.Записать();

	УдалитьФайлы(Архивируемый);
	
	ЧтениеZipФайла = Новый ЧтениеZipФайла(Архив, "123");
	ЭлементЗашифрован = ЧтениеZipФайла.Элементы[0].Зашифрован;
	ЧтениеZipФайла.Закрыть();
	УдалитьФайлы(Архив);

	юТест.ПроверитьИстину(ЭлементЗашифрован);

КонецПроцедуры

Функция УстановитьВременныйКаталогКакТекущий()
	ИмяКаталогаКорня = юТест.ИмяВременногоФайла();
	СоздатьКаталог(ИмяКаталогаКорня);
	УстановитьТекущийКаталог(ИмяКаталогаКорня);
	Возврат ТекущийКаталог();
КонецФункции

Функция ПодготовитьФайлВоВложенномКаталоге(Знач ИмяКаталога, Знач ИмяФайла)
	ПутьКаталога = ИмяКаталога;
	Если Не ПустаяСтрока(ИмяКаталога) Тогда
		СоздатьКаталог(ПутьКаталога);
		ПутьФайла = ОбъединитьПути(ПутьКаталога, ИмяФайла);
	Иначе
		ПутьФайла = ИмяФайла;
	КонецЕсли;

	СоздатьФайл(ПутьФайла);

	Возврат Новый Структура(
		"ПутьКаталога, ПутьФайла", ПутьКаталога, ПутьФайла);
КонецФункции

Функция СоздатьФайл(Знач ПутьФайла = "")
	Если ПутьФайла = "" Тогда
		ПутьФайла = СоздатьВременныйФайл();
	КонецЕсли;

	ЗТ = Новый ЗаписьТекста(ПутьФайла);
	ЗТ.Записать("Привет");
	ЗТ.Закрыть();
	Возврат ПутьФайла;
КонецФункции

Функция ДобавитьКоллекциюФайловИлиКаталоговВАрхивСОтносительнымиПутями(Знач МассивПутей)
	ИмяАрхива = юТест.ИмяВременногоФайла("zip");
	Архив = Новый ЗаписьZIPФайла(ИмяАрхива);
	
	Для каждого ОтносительныйПуть Из МассивПутей Цикл
		Архив.Добавить(ОтносительныйПуть, РежимСохраненияПутейZIP.СохранятьОтносительныеПути, РежимОбработкиПодкаталоговZIP.ОбрабатыватьРекурсивно);
	КонецЦикла;

	Архив.Записать();
	Возврат ИмяАрхива;
КонецФункции

Функция ДобавитьФайлИлиКаталогВАрхивСОтносительнымиПутями(Знач ОтносительныйПуть)
	МассивПутей = Новый Массив;
	МассивПутей.Добавить(ОтносительныйПуть);
	Возврат ДобавитьКоллекциюФайловИлиКаталоговВАрхивСОтносительнымиПутями(МассивПутей);
КонецФункции

Функция ИзвлечьВсеИзАрхиваВоВременныйКаталог(Знач ИмяАрхива)
	Архив = Новый ЧтениеZipФайла(ИмяАрхива);
	Распаковка = юТест.ИмяВременногоФайла();
	СоздатьКаталог(Распаковка);
	
	Архив.ИзвлечьВсе(Распаковка);
	Архив.Закрыть();

	Возврат Распаковка;
КонецФункции

Процедура ПроверитьНаличиеФайла(Знач ИскомыйПутьФайла, Знач СообщениеОшибки)
	Файл = Новый Файл(ИскомыйПутьФайла);
	юТест.ПроверитьИстину(Файл.Существует(), СообщениеОшибки + ИскомыйПутьФайла);
КонецПроцедуры

Процедура ПроверитьОтсутствиеФайла(Знач НеверныйПутьФайла, Знач СообщениеОшибки)
	Файл = Новый Файл(НеверныйПутьФайла);
	юТест.ПроверитьЛожь(Файл.Существует(), СообщениеОшибки + НеверныйПутьФайла);
КонецПроцедуры
