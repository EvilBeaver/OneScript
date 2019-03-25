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
	СписокТестов.Добавить("ТестУстановитьПространствоИмен");

	Возврат СписокТестов;

КонецФункции

#КонецОбласти

#Область ОбработчикиТестирования

Процедура ТестКонструктор() Экспорт

	СхемаXML = Новый СхемаXML;
	
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СхемаXML), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(СхемаXML.ТипКомпоненты, ТипКомпонентыXS.Схема);
	ЮнитТест.ПроверитьРавенство(СхемаXML.URIПространстваИменСхемыДляСхемыXML, "http://www.w3.org/2001/XMLSchema");
	ЮнитТест.ПроверитьРавенство(СхемаXML.Содержимое.Количество(), 0);
	ЮнитТест.ПроверитьРавенство(СхемаXML.Директивы.Количество(), 0);

КонецПроцедуры

Procedure TestConstructor() Export

	Schema = New XMLSchema;

	ЮнитТест.ПроверитьРавенство(ТипЗнч(Schema), Тип("XMLSchema"));
	ЮнитТест.ПроверитьРавенство(Schema.ComponentType, XSComponentType.Schema);

EndProcedure

Процедура ТестУстановитьПространствоИмен() Экспорт

	СхемаXML = Новый СхемаXML;
	СхемаXML.ПространствоИмен = "https://oscript.io";

	ЮнитТест.ПроверитьРавенство(СхемаXML.ПространствоИмен, "https://oscript.io");

КонецПроцедуры

#КонецОбласти