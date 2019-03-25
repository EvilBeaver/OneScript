Перем ЮнитТест;

#Область ОбработчикиСобытийМодуля

Функция Версия() Экспорт
	Возврат "1.0";
КонецФункции

Функция ПолучитьСписокТестов(МенеджерТестирования) Экспорт
	
	ЮнитТест = МенеджерТестирования;

	СписокТестов = Новый Массив;

	СписокТестов.Добавить("ТестКонструктор");
	СписокТестов.Добавить("TestConstructor");
	СписокТестов.Добавить("ТестДобавитьДокументацию");
	СписокТестов.Добавить("ТестДобавитьИнформациюДляПриложения");

	Возврат СписокТестов;

КонецФункции

#КонецОбласти

#Область ОбработчикиТестирования

Процедура ТестКонструктор() Экспорт

	Аннотация = Новый АннотацияXS;
	
	ЮнитТест.ПроверитьРавенство(Аннотация.ТипКомпоненты, ТипКомпонентыXS.Аннотация);

КонецПроцедуры

Procedure TestConstructor() Export

	Annotation = new XSAnnotation;

	ЮнитТест.ПроверитьРавенство(Annotation.ComponentType, XSComponentType.Annotation);
	
EndProcedure

Процедура ТестДобавитьДокументацию() Экспорт

	Аннотация = Новый АннотацияXS;
	
	Документация = Новый ДокументацияXS;
	Аннотация.Состав.Добавить(Документация);
	
	ЮнитТест.ПроверитьРавенство(Аннотация.Состав.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Документация.Контейнер, Аннотация);
	ЮнитТест.ПроверитьРавенство(Аннотация.Содержит(Документация), Истина);

КонецПроцедуры

Процедура ТестДобавитьИнформациюДляПриложения() Экспорт

	Аннотация = Новый АннотацияXS;
	
	ИнформацияДляПриложения = Новый ИнформацияДляПриложенияXS;
	ИнформацияДляПриложения.Источник = "https://oscript.io";

	Аннотация.Состав.Добавить(ИнформацияДляПриложения);
	
	ЮнитТест.ПроверитьРавенство(Аннотация.Состав.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(ИнформацияДляПриложения.Контейнер, Аннотация);
	ЮнитТест.ПроверитьРавенство(Аннотация.Содержит(ИнформацияДляПриложения), Истина);

КонецПроцедуры

#КонецОбласти