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

	ЮнитТест.ПроверитьРавенство(ТипЗнч(ФормаПредставленияXS),                     Тип("ПеречислениеФормаПредставленияXS"));
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ФормаПредставленияXS.Квалифицированная),   Тип("ФормаПредставленияXS"));
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ФормаПредставленияXS.Неквалифицированная), Тип("ФормаПредставленияXS"));

КонецПроцедуры

Процедура TestConstructor() Экспорт

	ЮнитТест.ПроверитьРавенство(TypeOf(XSForm),             Type("EnumerationXSForm"));
	ЮнитТест.ПроверитьРавенство(TypeOf(XSForm.Qualified),   Type("XSForm"));
	ЮнитТест.ПроверитьРавенство(TypeOf(XSForm.Unqualified), Type("XSForm"));

КонецПроцедуры

#КонецОбласти