//private InstanceConstructor CreateConstructor(string typeName, IValue[] arguments)
ПутьБиблиотеки = "C:\C#\VanessaExtWin64.dll";
ПодключитьВнешнююКомпоненту(ПутьБиблиотеки, "VanessaExt", ТипВнешнейКомпоненты.Native);

ВнешняяКомпонента = Новый("AddIn.VanessaExt.WindowsControl");
Сообщить(ВнешняяКомпонента.Версия);
Сообщить(ВнешняяКомпонента.СписокОкон);

ВнешняяКомпонента = Новый("AddIn.VanessaExt.ClipboardControl");
ВнешняяКомпонента.Текст = "Пример текста";
Сообщить(ВнешняяКомпонента.Текст);
