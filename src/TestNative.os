//private InstanceConstructor CreateConstructor(string typeName, IValue[] arguments)
ПутьБиблиотеки = "C:\C#\VanessaExtWin64.dll";
ПодключитьВнешнююКомпоненту(ПутьБиблиотеки, "VanessaExt", ТипВнешнейКомпоненты.Native);

ВнешняяКомпонента = Новый("AddIn.VanessaExt.GitFor1C");
Сообщить(ВнешняяКомпонента.Версия);