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

	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЗавершенностьСхемыXS),             Тип("ПеречислениеЗавершенностьСхемыXS"));
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЗавершенностьСхемыXS.Все),         Тип("ЗавершенностьСхемыXS"));
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЗавершенностьСхемыXS.Объединение), Тип("ЗавершенностьСхемыXS"));
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЗавершенностьСхемыXS.Ограничение), Тип("ЗавершенностьСхемыXS"));
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЗавершенностьСхемыXS.Расширение),  Тип("ЗавершенностьСхемыXS"));
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЗавершенностьСхемыXS.Список),      Тип("ЗавершенностьСхемыXS"));

КонецПроцедуры

Процедура TestConstructor() Экспорт

	ЮнитТест.ПроверитьРавенство(TypeOf(XSSchemaFinal),             Type("EnumXSSchemaFinal"));
	ЮнитТест.ПроверитьРавенство(TypeOf(XSSchemaFinal.All),         Type("XSSchemaFinal"));
	ЮнитТест.ПроверитьРавенство(TypeOf(XSSchemaFinal.Union),       Type("XSSchemaFinal"));
	ЮнитТест.ПроверитьРавенство(TypeOf(XSSchemaFinal.Restriction), Type("XSSchemaFinal"));
	ЮнитТест.ПроверитьРавенство(TypeOf(XSSchemaFinal.Extension),   Type("XSSchemaFinal"));
	ЮнитТест.ПроверитьРавенство(TypeOf(XSSchemaFinal.List),        Type("XSSchemaFinal"));

КонецПроцедуры

#КонецОбласти