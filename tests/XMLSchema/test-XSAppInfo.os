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

	Возврат СписокТестов;

КонецФункции

#КонецОбласти

#Область ОбработчикиТестирования

Процедура ТестКонструктор() Экспорт

	ИнформацияДляПриложения = Новый ИнформацияДляПриложенияXS;
	
	ЮнитТест.ПроверитьРавенство(ИнформацияДляПриложения.ТипКомпоненты, ТипКомпонентыXS.ИнформацияПриложения);
	
КонецПроцедуры

Procedure TestConstructor() Export

	AppInfo = New XSAppInfo;

	ЮнитТест.ПроверитьРавенство(AppInfo.ComponentType, XSComponentType.AppInfo);

EndProcedure

#КонецОбласти