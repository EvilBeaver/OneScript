﻿//private InstanceConstructor CreateConstructor(string typeName, IValue[] arguments)
ПутьБиблиотеки = "C:\C#\VanessaExtWin64.dll";
ПутьБиблиотеки = "H:\Cpp\VanessaExt\AddIn.zip"; 
ПодключитьВнешнююКомпоненту(ПутьБиблиотеки, "VanessaExt", ТипВнешнейКомпоненты.Native);

ВнешняяКомпонента = Новый("AddIn.VanessaExt.WindowsControl");
ВнешняяКомпонента.УстановитьПозициюКурсора(200, 200);
ДвоичныеДанные = ВнешняяКомпонента.ПолучитьСнимокЭкрана(0);
ДвоичныеДанные.Записать("H:/screenshot.png");
Сообщить(ВнешняяКомпонента.ПолучитьСписокПроцессов(Истина));
Сообщить(ВнешняяКомпонента.Версия);
Сообщить(ВнешняяКомпонента.СписокОкон);

ВнешняяКомпонента = Новый("AddIn.VanessaExt.ClipboardControl");
ВнешняяКомпонента.Текст = "Пример текста";
Сообщить(ВнешняяКомпонента.Текст);
Сообщить(ТипЗнч(ВнешняяКомпонента));
Сообщить(ТипЗнч(ВнешняяКомпонента) = Тип("AddIn.VanessaExt.ClipboardControl"));

ПутьБиблиотеки = "C:\C#\AddInNativeWin64.dll";
ПодключитьВнешнююКомпоненту(ПутьБиблиотеки, "AddInNative", ТипВнешнейКомпоненты.Native);
ВнешняяКомпонента = Новый("AddIn.AddInNative.CAddInNative");
ВнешняяКомпонента.Включен = Истина;
Сообщить("Есть таймер: " + ВнешняяКомпонента.ЕстьТаймер);
Сообщить("Включен: " + ВнешняяКомпонента.Включен);
ДвоичныеДанные2 = ВнешняяКомпонента.ЗагрузитьКартинку("H:/screenshot.png");
ДвоичныеДанные2.Записать("H:/screenshot2.png");
Сообщить(ТипЗнч(ДвоичныеДанные2));
