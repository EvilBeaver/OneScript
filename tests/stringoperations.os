﻿Перем юТест;

Функция ПолучитьСписокТестов(ЮнитТестирование) Экспорт
	
	юТест = ЮнитТестирование;
	
	ВсеТесты = Новый Массив;
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНачинаетсяС_НаКириллице");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНачинаетсяС_НаЛатинице");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНачинаетсяС_СПустойСтрокойПоиска");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНачинаетсяС_ПодстрокаНеНайдена");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНачинаетсяС_ИсходнаяСтрокаНеЗадана");
	
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_НаКириллице");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_НаЛатинице");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_СПустойСтрокойПоиска");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_ПодстрокаНеНайдена");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_ИсходнаяСтрокаНеЗадана");
	
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_НаКириллице");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_НаЛатинице");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_СПустойСтрокой");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_СПустойСтрокойРазделителя");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_РазделениеБезПустых");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_РазделениеСПустыми");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_СПустойСтрокойБезПустых");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_Авторазделение");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрРазделить_ПараметрыНеЗаданы");
	
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_БезПараметровДанных");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриПустомЗнакеПроцента");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриНомере0");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриНомереБольше10");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_КорректнаяЗамена");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_СЭкранированием");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_СПропущеннымиПараметрами");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриЛишнихПараметрах");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_СНомеромВСкобках");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрШаблон_СПустымШаблоном");
	
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрСоединить_СРазделителем");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрСоединить_БезРазделителя");
	
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНайти_КакФункциюНайти");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьВхождениеСтрокиСНачалаПоОшибке171");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНайти_СТретьейПозиции");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНайти_ВтороеВхождениеСНачала");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНайти_ВтороеВхождениеСКонца");
	ВсеТесты.Добавить("ТестДолжен_ВызватьМетод_СтрНайти_ВтороеВхождениеСКонцаНачинаяСПредпоследнегоСимвола");
	ВсеТесты.Добавить("Тест_ДолженПроверитьНСтрВозвращаетПервуюСтроку");
	ВсеТесты.Добавить("ТестДолжен_Проверить_Что_НСТР_С_СуществующимПараметром_ВозвращаетНужнуюСтроку");
	ВсеТесты.Добавить("ТестДолжен_Проверить_Что_НСТР_С_НесуществующимПараметром_ВозвращаетПустуюСтроку");
	ВсеТесты.Добавить("ТестДолжен_ПроверитьГраничныеУсловияСтр");
	
	Возврат ВсеТесты;
	
КонецФункции

Процедура ТестДолжен_ПроверитьГраничныеУсловияСтр() Экспорт

	Текст = "Привет";
	юТест.ПроверитьРавенство("Привет",Сред(Текст,1,6));
	юТест.ПроверитьРавенство("Приве",Сред(Текст,1,5));
	юТест.ПроверитьРавенство("ет",Сред(Текст,5,4));
	юТест.ПроверитьРавенство("т",Сред(Текст,6,1));

КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрНачинаетсяС_НаКириллице() Экспорт
	//arrange	
	//act
	Рез = СтрНачинаетсяС("Проверка", "Пров");
	//assert
	юТест.ПроверитьРавенство(Истина, Рез);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрНачинаетсяС_НаЛатинице() Экспорт
	//arrange
	//act
	Рез = StrStartsWith("Проверка", "Пров");
	//assert
	юТест.ПроверитьРавенство(Истина, Рез);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрНачинаетсяС_СПустойСтрокойПоиска() Экспорт
	ТекстОшибки = "";
	//arrange	
	//act
	Попытка
		Рез = СтрНачинаетсяС("Проверка", "");
	Исключение
		ТекстОшибки = ОписаниеОшибки();
	КонецПопытки;	
	//assert
	//Сообщить("[" + ТекстОшибки + "]");
	юТест.ПроверитьРавенство(Истина, 	Найти(ТекстОшибки, "Недопустимое значение параметра") > 0);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрНачинаетсяС_ПодстрокаНеНайдена() Экспорт
	//arrange	
	//act
	Рез = СтрНачинаетсяС("Проверка", "Тест");
	//assert
	юТест.ПроверитьРавенство(Ложь, Рез);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрНачинаетсяС_ИсходнаяСтрокаНеЗадана() Экспорт
	ТекстОшибки = "";
	//arrange	
	//act
	Рез = СтрНачинаетсяС(Неопределено, "1");
	//assert
	юТест.ПроверитьРавенство(Ложь, Рез);
КонецПроцедуры	


//------------------------------------------------------------

Процедура ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_НаКириллице() Экспорт
	//arrange	
	//act
	Рез = СтрЗаканчиваетсяНа("Проверка", "верка");
	//assert
	юТест.ПроверитьРавенство(Истина, Рез);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_НаЛатинице() Экспорт
	//arrange	
	//act
	Рез = StrEndsWith("Проверка", "верка");
	//assert
	юТест.ПроверитьРавенство(Истина, Рез);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_СПустойСтрокойПоиска() Экспорт
	ТекстОшибки = "";
	//arrange	
	//act
	Попытка
		Рез = СтрЗаканчиваетсяНа("Проверка", "");
	Исключение
		ТекстОшибки = ОписаниеОшибки();
	КонецПопытки;	
	//assert
	юТест.ПроверитьРавенство(Истина, 	Найти(ТекстОшибки, "Недопустимое значение параметра") > 0);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_ПодстрокаНеНайдена() Экспорт
	//arrange	
	//act
	Рез = СтрЗаканчиваетсяНа("Проверка", "Тест");
	//assert
	юТест.ПроверитьРавенство(Ложь, Рез);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрЗаканчиваетсяНа_ИсходнаяСтрокаНеЗадана() Экспорт
	//arrange	
	//act
	Рез = СтрЗаканчиваетсяНа(Неопределено, "1");
	//assert
	юТест.ПроверитьРавенство(Ложь, 	Рез);
КонецПроцедуры	

//------------------------------------------------------------

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_НаКириллице() Экспорт
	//arrange	
	//act
	мРезультат = СтрРазделить("0,1,2,3,4,5", ",");
	//assert
	юТест.ПроверитьРавенство(6, мРезультат.Количество());
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_НаЛатинице() Экспорт
	//arrange	
	//act
	мРезультат = StrSplit("0,1,2,3,4,5", ",");
	//assert
	юТест.ПроверитьРавенство(6, мРезультат.Количество());
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_СПустойСтрокой() Экспорт
	ТекстОшибки = "";
	//arrange	
	//act
		мРезультат = СтрРазделить("", ",");
	//assert
	юТест.ПроверитьРавенство(1, мРезультат.Количество());
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_СПустойСтрокойБезПустых() Экспорт
	ТекстОшибки = "";
	//arrange	
	//act
		мРезультат = СтрРазделить("", ",", ложь);
	//assert
	юТест.ПроверитьРавенство(0, мРезультат.Количество());
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_СПустойСтрокойРазделителя() Экспорт
	ТекстОшибки = "";
	//arrange	
	//act
	Попытка
		мРезультат = СтрРазделить("0,1,2,3,4,5", "");
	Исключение
		ТекстОшибки = ОписаниеОшибки();
	КонецПопытки;	
	//assert
	юТест.ПроверитьРавенство("0,1,2,3,4,5", мРезультат[0]);
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_РазделениеБезПустых() Экспорт
	//arrange	
	//act
		мРезультат = СтрРазделить("0,1,2,,4,5", ",", Ложь);
	//assert
	юТест.ПроверитьРавенство(5, мРезультат.Количество());
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_РазделениеСПустыми() Экспорт
	//arrange	
	//act
	мРезультат = СтрРазделить("0,1,2,,4,5", ",", Истина);
	//assert
	юТест.ПроверитьРавенство(6, мРезультат.Количество());
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_Авторазделение() Экспорт
	//arrange	
	//act
	мРезультат = СтрРазделить("0,1,2,,4,5", ",");
	//assert
	юТест.ПроверитьРавенство(6, мРезультат.Количество());
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрРазделить_ПараметрыНеЗаданы() Экспорт
	ТекстОшибки = "";
	//arrange	
	//act
	Попытка
		мРезультат = СтрРазделить(Неопределено, Неопределено);
	Исключение
		ТекстОшибки = ОписаниеОшибки();
	КонецПопытки;	
	//assert
	юТест.ПроверитьРавенство(1, мРезультат.Количество());
КонецПроцедуры	

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_БезПараметровДанных() Экспорт
	Шаблон = "%1 %2 %3 %4 %5 %6 %7 %8 %9 %10";
	результат = СтрШаблон(Шаблон);
	юТест.ПроверитьРавенство("         ", результат);
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриПустомЗнакеПроцента() Экспорт
	
	Попытка
		а = СтрШаблон("тест % тест");
	Исключение
		юТест.ТестПройден();
		Возврат;
	КонецПопытки;
	
	юТест.ТестПровален("Ожидаемое исключение не возникло.");
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриНомере0() Экспорт
	
	Попытка
		а = СтрШаблон("тест %0 тест");
	Исключение
		юТест.ТестПройден();
		Возврат;
	КонецПопытки;
	
	юТест.ТестПровален("Ожидаемое исключение не возникло.");
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриНомереБольше10() Экспорт
	
	Попытка
		а = СтрШаблон("тест %11 тест");
	Исключение
		юТест.ТестПройден();
		Возврат;
	КонецПопытки;
	
	юТест.ТестПровален("Ожидаемое исключение не возникло.");
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_КорректнаяЗамена() Экспорт
	
	Шаблон = "Привет, %2, я %1!";
	
	Результат = СтрШаблон(Шаблон, "OneScript", "%username%");
	
	юТест.ПроверитьРавенство("Привет, %username%, я OneScript!", Результат);
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_СЭкранированием() Экспорт
	
	Шаблон = "тест %%1 тест";
	
	Результат = СтрШаблон(Шаблон);
	
	юТест.ПроверитьРавенство("тест %1 тест", Результат);
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_СПропущеннымиПараметрами() Экспорт
	
	Результат = СтрШаблон("тест %1 тест %2 тест %3", "1", ,"3");
	юТест.ПроверитьРавенство("тест 1 тест  тест 3", Результат);

	Результат = СтрШаблон("тест %1 тест %2", ,"2", ,);
	юТест.ПроверитьРавенство("тест  тест 2", Результат);

	Результат = СтрШаблон("тест %1 тест %2 тест %3 тест %4", Неопределено, ,"3", , , );
	юТест.ПроверитьРавенство("тест  тест  тест 3 тест ", Результат);

КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_ИсключениеПриЛишнихПараметрах() Экспорт
	
	Попытка
		а = СтрШаблон("тест %1 тест", 1, 2);
	Исключение
		юТест.ТестПройден();
		Возврат;
	КонецПопытки;
	
	юТест.ТестПровален("Ожидаемое исключение не возникло.");
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_СНомеромВСкобках() Экспорт
	
	Результат = СтрШаблон("тест %(1)0", "=");
	юТест.ПроверитьРавенство("тест =0", Результат);

	Попытка
		а = СтрШаблон("тест %(11)0", 1);
	Исключение
		юТест.ТестПройден();
		Возврат;
	КонецПопытки;
	
	юТест.ТестПровален("Ожидаемое исключение не возникло.");

КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрШаблон_СПустымШаблоном() Экспорт

	Результат = СтрШаблон(,);
	юТест.ПроверитьРавенство("", Результат);

	Попытка
		а = СтрШаблон(,1);
	Исключение
		юТест.ТестПройден();
		Возврат;
	КонецПопытки;
	
	юТест.ТестПровален("Ожидаемое исключение не возникло.");

КонецПроцедуры


Процедура ТестДолжен_ВызватьМетод_СтрСоединить_БезРазделителя() Экспорт
	
	Массив = Новый Массив;
	Массив.Добавить("Один");
	Массив.Добавить("Два");
	Массив.Добавить("Три");
	
	юТест.ПроверитьРавенство("ОдинДваТри", СтрСоединить(Массив));
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрСоединить_СРазделителем() Экспорт
	
	Массив = Новый Массив;
	Массив.Добавить("Один");
	Массив.Добавить("Два");
	Массив.Добавить("Три");
	
	юТест.ПроверитьРавенство("Один, Два, Три", СтрСоединить(Массив, ", "));
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрНайти_КакФункциюНайти() Экспорт
	
	ГдеИщем = "Один,Два,Три";
	ЧтоИщем = ",";
	
	юТест.ПроверитьРавенство(5, СтрНайти(ГдеИщем, ЧтоИщем));
	юТест.ПроверитьРавенство(0, СтрНайти(ГдеИщем, "%"));
	
КонецПроцедуры

Процедура ТестДолжен_ПроверитьВхождениеСтрокиСНачалаПоОшибке171() Экспорт

	СтрокаТест = "abc"; 
	юТест.ПроверитьРавенство(1, СтрНайти(СтрокаТест, "a"));

КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрНайти_СТретьейПозиции() Экспорт
	
	ГдеИщем = "1,2,3";
	ЧтоИщем = ",";
	
	юТест.ПроверитьРавенство(4, СтрНайти(ГдеИщем, ЧтоИщем,,3));
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрНайти_ВтороеВхождениеСНачала() Экспорт
	
	ГдеИщем = "Один,Два,Три";
	ЧтоИщем = ",";
	
	юТест.ПроверитьРавенство(9, СтрНайти(ГдеИщем, ЧтоИщем,,,2));
	юТест.ПроверитьРавенство(9, СтрНайти(ГдеИщем, ЧтоИщем,НаправлениеПоиска.СНачала,,2));
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрНайти_ВтороеВхождениеСКонца() Экспорт
	
	ГдеИщем = "Один,Два,Три";
	ЧтоИщем = ",";
	
	юТест.ПроверитьРавенство(5, СтрНайти(ГдеИщем, ЧтоИщем, НаправлениеПоиска.СКонца,,2));
	
КонецПроцедуры

Процедура ТестДолжен_ВызватьМетод_СтрНайти_ВтороеВхождениеСКонцаНачинаяСПредпоследнегоСимвола() Экспорт
	
	ГдеИщем = "Один,Два,Три,";
	ЧтоИщем = ",";
	
	юТест.ПроверитьРавенство(5, СтрНайти(ГдеИщем, ЧтоИщем, НаправлениеПоиска.СКонца,12,2));
	
КонецПроцедуры

Процедура Тест_ДолженПроверитьНСтрВозвращаетПервуюСтроку() Экспорт
	
	Стр = НСтр("ru = 'Строка1'; en = 'Строка2'", "ru");
	юТест.ПроверитьРавенство("Строка1", Стр);
	
КонецПроцедуры

Процедура ТестДолжен_Проверить_Что_НСТР_С_СуществующимПараметром_ВозвращаетНужнуюСтроку() Экспорт
	Шаблон = "ru = 'Строка1'; en = 'Строка2'";
	Стр = НСтр(Шаблон, "en");
	юТест.ПроверитьРавенство("Строка2", Стр);
	
	Стр = НСтр(Шаблон, "ru");
	юТест.ПроверитьРавенство("Строка1", Стр);
КонецПроцедуры

Процедура ТестДолжен_Проверить_Что_НСТР_С_НесуществующимПараметром_ВозвращаетПустуюСтроку() Экспорт
	Стр = НСтр("ru = 'Строка1'; en = 'Строка2'", "unkn");
	юТест.ПроверитьРавенство("", Стр);
КонецПроцедуры
