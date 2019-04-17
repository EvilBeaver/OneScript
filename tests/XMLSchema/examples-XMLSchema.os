Перем ЮнитТест;
Перем ЗаписьXML;
Перем ЧтениеXML;
Перем СериализаторXDTO;

#Область ОбработчикиСобытийМодуля

Функция ПолучитьСписокТестов(МенеджерТестирования) Экспорт
	
	ЮнитТест = МенеджерТестирования;

	СписокТестов = Новый Массив;
	СписокТестов.Добавить("ТестСхемаXML");
	СписокТестов.Добавить("ТестВключениеXS");
	СписокТестов.Добавить("ТестДокументацияXS");
	СписокТестов.Добавить("ТестИнформацияДляПриложенияXS");
	СписокТестов.Добавить("ТестОпределениеПростогоТипаXS");
	СписокТестов.Добавить("ТестОпределениеПростогоТипаXS_Объединение");
	СписокТестов.Добавить("ТестФасетДлиныXS");
	СписокТестов.Добавить("ТестФасетМинимальнойДлиныXS");
	СписокТестов.Добавить("ТестФасетМаксимальнойДлиныXS");
	СписокТестов.Добавить("ТестФасетКоличестваРазрядовДробнойЧастиXS");
	СписокТестов.Добавить("ТестФасетМинимальногоИсключающегоЗначенияXS");
	СписокТестов.Добавить("ТестМаскаXS");
	СписокТестов.Добавить("ТестОпределениеГруппыАтрибутовXS");
	СписокТестов.Добавить("ТестОбъявлениеНотацииXS");
	СписокТестов.Добавить("ТестОпределениеГруппыМоделиXS");
	
	Возврат СписокТестов;

КонецФункции

#КонецОбласти

#Область ОбработчикиТестирования

Функция ТекстСхемыXML(СхемаXML)

	ЗаписьXML.УстановитьСтроку();
	СериализаторXDTO.ЗаписатьXML(ЗаписьXML, СхемаXML);
	ТекстСхемыXML = ЗаписьXML.Закрыть(); 
	
	Возврат ТекстСхемыXML;

КонецФункции

Функция СхемаXMLИзТекста(ТекстСхемыXML)

	ЧтениеXML.УстановитьСтроку(ТекстСхемыXML);
	ЧтениеXML.ПерейтиКСодержимому();
	СхемаXML = СериализаторXDTO.ПрочитатьXML(ЧтениеXML, Тип("СхемаXML"));
	ЧтениеXML.Закрыть();
	
	Возврат СхемаXML;

КонецФункции

Функция СериализоватьДесериализоватьСхемуXML(СхемаXML)

	ТекстСхемыXML = ТекстСхемыXML(СхемаXML);
	НоваяСхемаXML = СхемаXMLИзТекста(ТекстСхемыXML);

	Возврат НоваяСхемаXML;

КонецФункции

Процедура ТестДокументацияXS() Экспорт

	Схема = ПримерДокументацияXS();
	Schema = ExampleXSDocumentation();

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьЗаполненность(Schema);

КонецПроцедуры

Процедура ТестИнформацияДляПриложенияXS() Экспорт

	Схема = ПримерИнформацияДляПриложенияXS();
	Schema = ExampleXSAppInfo();

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьЗаполненность(Schema);

КонецПроцедуры

Процедура ТестОпределениеПростогоТипаXS() Экспорт

	Схема = ПримерОпределениеПростогоТипаXS();
	Schema = ExampleXSSimpleTypeDefinition();

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьЗаполненность(Schema);

КонецПроцедуры

Процедура ТестОпределениеПростогоТипаXS_Объединение() Экспорт

	Схема = ПримерОпределениеПростогоТипаXS_Объединение();
	Schema = ExampleXSSimpleTypeDefinition_Union();

	ПроверитьОпределениеПростогоТипаXS_Объединение(Схема);
	ПроверитьОпределениеПростогоТипаXS_Объединение(Schema);

КонецПроцедуры

Процедура ТестФасетДлиныXS() Экспорт

	Схема = ПримерФасетДлиныXS();
	Schema = ExampleXSLengthFacet();

	ПроверитьФасетДлиныXS(Схема);
	ПроверитьФасетДлиныXS(Schema)

КонецПроцедуры

Процедура ТестФасетМинимальнойДлиныXS() Экспорт

	Схема = ПримерФасетМинимальнойДлиныXS();
	Schema = ExampleXSMinLengthFacet();

	ПроверитьФасетМинимальнойДлиныXS(Схема);
	ПроверитьФасетМинимальнойДлиныXS(Schema)

КонецПроцедуры

Процедура ТестФасетМаксимальнойДлиныXS() Экспорт

	Схема = ПримерФасетМаксимальнойДлиныXS();
	Schema = ExampleXSMaxLengthFacet();

	ПроверитьФасетМаксимальнойДлиныXS(Схема);
	ПроверитьФасетМаксимальнойДлиныXS(Schema)

КонецПроцедуры

Процедура ТестФасетКоличестваРазрядовДробнойЧастиXS() Экспорт

	Схема = ПримерФасетКоличестваРазрядовДробнойЧастиXS();
	Schema = ExampleXSFractionDigitsFacet();

	ПроверитьФасетКоличестваРазрядовДробнойЧастиXS(Схема);
	ПроверитьФасетКоличестваРазрядовДробнойЧастиXS(Schema)

КонецПроцедуры

Процедура ТестФасетМинимальногоИсключающегоЗначенияXS() Экспорт

	Схема = ПримерФасетМинимальногоИсключающегоЗначенияXS();
	Schema = ExampleXSMinExclusiveFacet();

	ПроверитьФасетМинимальногоИсключающегоЗначенияXS(Схема);
	ПроверитьФасетМинимальногоИсключающегоЗначенияXS(Schema)

КонецПроцедуры

#КонецОбласти

#Область ВыборПримера

Функция ПримерФормированияСхемыXML()

	СхемаXML = Новый СхемаXML;
	
	//////////////////////////

	//СхемаXML = ПримерСхемаXML();
	//СхемаXML = ExampleXMLSchema();

	//СхемаXML = ПримерВключениеXS();
	//СхемаXML = ExampleXSInclude();
	
	//СхемаXML = ПримерДокументацияXS();
	//СхемаXML = ExampleXSDocumentation();

	//СхемаXML = ПримерИнформацияДляПриложенияXS();
	//СхемаXML = ExampleXSAppInfo();

	//СхемаXML = ПримерОпределениеПростогоТипаXS();
	//СхемаXML = ExampleXSSimpleTypeDefinition();

	//СхемаXML = ПримерОпределениеПростогоТипаXS_Объединение();
	//СхемаXML = ExampleXSSimpleTypeDefinition_Union();

	//СхемаXML = ПримерФасетДлиныXS();
	//СхемаXML = ExampleXSLengthFacet();

	//СхемаXML = ПримерФасетМинимальнойДлиныXS();
	//СхемаXML = ExampleXSMinLengthFacet();

	//СхемаXML = ПримерФасетМаксимальнойДлиныXS();
	//СхемаXML = ExampleXSMaxLengthFacet();

	//СхемаXML = ПримерФасетКоличестваРазрядовДробнойЧастиXS();
	//СхемаXML = ExampleXSFractionDigitsFacet();

	//СхемаXML = ПримерФасетМинимальногоИсключающегоЗначенияXS();
	//СхемаXML = ExampleXSMinExclusiveFacet();

	//СхемаXML = ПримерМаскаXS();
	//СхемаXML = ExampleXSWildcard();

	//СхемаXML = ПримерОпределениеГруппыАтрибутовXS();
	//СхемаXML = ExampleXSAttributeGroupDefinition();
	
	//СхемаXML = ПримерОбъявлениеНотацииXS();
	//СхемаXML = ExampleXSNotationDeclaration();

	СхемаXML = ПримерОпределениеГруппыМоделиXS();
	//СхемаXML = ExampleXSModelGroupDefinition();

	//////////////////////////
	
	Возврат СхемаXML;

КонецФункции

#КонецОбласти

#Область Примеры

#Область СхемаXML

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschema
//
// Результат:
//	см. РезультатСхемаXML

Функция ПримерСхемаXML()

	Схема = Новый СхемаXML;

	// <xs:element name="cat" type="xs:string"/>
	Элемент = Новый ОбъявлениеЭлементаXS; 
	Элемент.Имя = "cat";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	Схема.Содержимое.Добавить(Элемент);

	// <xs:element name="dog" type="xs:string"/>
	Элемент = Новый ОбъявлениеЭлементаXS; 
	Элемент.Имя = "dog";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	Схема.Содержимое.Добавить(Элемент);
	
	 // <xs:element name="redDog" substitutionGroup="dog" />
	Элемент = Новый ОбъявлениеЭлементаXS; 
	Элемент.Имя = "redDog";
	Элемент.ПрисоединениеКГруппеПодстановки = Новый РасширенноеИмяXML("", "dog");
	Схема.Содержимое.Добавить(Элемент);

	// <xs:element name="brownDog" substitutionGroup ="dog" />
	Элемент = Новый ОбъявлениеЭлементаXS; 
	Элемент.Имя = "brownDog";
	Элемент.ПрисоединениеКГруппеПодстановки = Новый РасширенноеИмяXML("", "dog");
	Схема.Содержимое.Добавить(Элемент);

	// <xs:element name="pets">
	Элемент = Новый ОбъявлениеЭлементаXS; 
	Элемент.Имя = "pets";
	Схема.Содержимое.Добавить(Элемент);
	
	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	
	// <xs:choice minOccurs="0" maxOccurs="unbounded">
	ГруппаВыбор = Новый ГруппаМоделиXS;
	ГруппаВыбор.ВидГруппы = ВидГруппыМоделиXS.Выбор;
	Фрагмент = Новый ФрагментXS;
	Фрагмент.МинимальноВходит = 0;
	Фрагмент.МаксимальноВходит = -1;
	Фрагмент.Часть = ГруппаВыбор;
	СоставнойТип.Содержимое = Фрагмент;

	// <xs:element ref="cat"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Ссылка = Новый РасширенноеИмяXML("", "cat");
	ГруппаВыбор.Фрагменты.Добавить(Элемент);

	// <xs:element ref="dog"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Ссылка = Новый РасширенноеИмяXML("", "dog");
	ГруппаВыбор.Фрагменты.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXMLSchema()
	
	Schema = New XMLSchema;
	
	// <xs:element name="cat" type="xs:string"/>
	elementCat = New XSElementDeclaration;
	elementCat.Name = "cat";
	elementCat.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	schema.Content.Add(elementCat);
	
	// <xs:element name="dog" type="xs:string"/>
	elementDog = New XSElementDeclaration;
	elementDog.Name = "dog";
	elementDog.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	schema.Content.Add(elementDog);	
	
	// <xs:element name="redDog" substitutionGroup="dog" />
	elementRedDog = New XSElementDeclaration;
	elementRedDog.Name = "redDog";
	elementRedDog.SubstitutionGroupAffiliation = New XMLExpandedName("", "dog");
	schema.Content.Add(elementRedDog);
	
	// <xs:element name="brownDog" substitutionGroup ="dog" />
	elementBrownDog = New XSElementDeclaration;
	elementBrownDog.Name = "brownDog";
	elementBrownDog.SubstitutionGroupAffiliation = New XMLExpandedName("", "dog");
	schema.Content.Add(elementBrownDog);
	
	// <xs:element name="pets">
	elementPets = New XSElementDeclaration;
	elementPets.Name = "pets";
	schema.Content.Add(elementPets);
	
	// <xs:complexType>
	complexType = new XSComplexTypeDefinition;
	elementPets.AnonymousTypeDefinition = complexType;
	
	// <xs:choice minOccurs="0" maxOccurs="unbounded">
	choice = New XSModelGroup;
	choice.Compositor = XSCompositor.Choice;
	particle = New XSParticle;
	particle.MinOccurs = 0;
	particle.MaxOccurs = -1;
	particle.Term = choice;
	complexType.Content = particle;
	
	// <xs:element ref="cat"/>
	catRef = New XSElementDeclaration;
	catRef.Reference = New XMLExpandedName("", "cat");
	choice.Particles.Add(catRef);
	
	// <xs:element ref="dog"/>
	dogRef = New XSElementDeclaration;
	dogRef.Reference = New XMLExpandedName("", "dog");
	choice.Particles.Add(dogRef);
	
	Return Schema;

EndFunction

Функция РезультатСхемаXML()
	
	Возврат
	"<xs:schema xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
	|    <xs:element name=""cat"" type=""xs:string""/>
	|    <xs:element name=""dog"" type=""xs:string""/>
	|    <xs:element name=""redDog"" type=""xs:string"" substitutionGroup=""dog""/>
	|    <xs:element name=""brownDog"" type=""xs:string"" substitutionGroup =""dog"" />
	|    <xs:element name=""pets"">
	|      <xs:complexType>
	|        <xs:choice minOccurs=""0"" maxOccurs=""unbounded"">
	|          <xs:element ref=""cat""/>
	|          <xs:element ref=""dog""/>
	|        </xs:choice>
	|      </xs:complexType>
	|    </xs:element>
	|</xs:schema>";
	
КонецФункции

Процедура ПроверитьСхемаXML(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 5);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 5);

	Элемент = Схема.ОбъявленияЭлементов.Получить("cat");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "cat");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	Элемент = Схема.ОбъявленияЭлементов.Получить("dog");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "dog");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	Элемент = Схема.ОбъявленияЭлементов.Получить("redDog");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "redDog");
	ЮнитТест.ПроверитьРавенство(Элемент.ПрисоединениеКГруппеПодстановки, Новый РасширенноеИмяXML("", "dog"));

	Элемент = Схема.ОбъявленияЭлементов.Получить("brownDog");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "brownDog");
	ЮнитТест.ПроверитьРавенство(Элемент.ПрисоединениеКГруппеПодстановки, Новый РасширенноеИмяXML("", "dog"));

	Элемент = Схема.ОбъявленияЭлементов.Получить("pets");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "pets");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));

	Фрагмент = СоставнойТип.Содержимое;
	ЮнитТест.ПроверитьЗаполненность(Фрагмент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Фрагмент), Тип("ФрагментXS"));
	ЮнитТест.ПроверитьРавенство(Фрагмент.МинимальноВходит, 0);
	ЮнитТест.ПроверитьРавенство(Фрагмент.МаксимальноВходит, -1);

	ГруппаВыбор = Фрагмент.Часть;
	ЮнитТест.ПроверитьЗаполненность(ГруппаВыбор);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ГруппаВыбор), Тип("ГруппаМоделиXS"));
	ЮнитТест.ПроверитьРавенство(ГруппаВыбор.ВидГруппы, ВидГруппыМоделиXS.Выбор);
	ЮнитТест.ПроверитьРавенство(ГруппаВыбор.Фрагменты.Количество(), 2);

	Элемент = ГруппаВыбор.Фрагменты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Ссылка, Новый РасширенноеИмяXML("", "cat"));
	ЮнитТест.ПроверитьИстину(Элемент.ЭтоСсылка);

	Элемент = ГруппаВыбор.Фрагменты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Ссылка, Новый РасширенноеИмяXML("", "dog"));
	ЮнитТест.ПроверитьИстину(Элемент.ЭтоСсылка);

КонецПроцедуры

Процедура ТестСхемаXML() Экспорт

	Схема = ПримерСхемаXML();
	Schema = ExampleXMLSchema();
	СхемаТекст = СхемаXMLИзТекста(РезультатСхемаXML());
	//TODO: СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);

	ПроверитьСхемаXML(Схема);
	ПроверитьСхемаXML(Schema);
	//TODO: ПроверитьСхемаXML(СхемаТекст);
	//TODO: ПроверитьСхемаXML(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ВключениеXS

// Источник:
// 	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemainclude
//
// Результат:
//	см. РезультатВключениеXS

Функция ПримерВключениеXS()

	Схема = Новый СхемаXML;
	Схема.ФормаЭлементовПоУмолчанию = ФормаПредставленияXS.Квалифицированная;
	Схема.ПространствоИмен = "http://www.w3.org/2001/05/XMLInfoset";
	
	// <xs:import namespace="http://www.example.com/IPO" />
	Импорт = Новый ИмпортXS;
	Импорт.ПространствоИмен = "http://www.example.com/IPO";
	Схема.Директивы.Добавить(Импорт);
	
	// <xs:include schemaLocation="example.xsd" />
	Включение = Новый ВключениеXS;
	Включение.РасположениеСхемы = "example.xsd";
	Схема.Директивы.Добавить(Включение);
	
	Возврат Схема;
	
КонецФункции	

Function ExampleXSInclude()

	Schema = new XMLSchema;
	Schema.ElementFormDefault = XSForm.Qualified;
	Schema.TargetNamespace = "http://www.w3.org/2001/05/XMLInfoset";

	// <xs:import namespace="http://www.example.com/IPO" />
	Import = new XSImport;
	Import.Namespace = "http://www.example.com/IPO";
	Schema.Directives.Add(Import);

	// <xs:include schemaLocation="example.xsd" />
	Include = new XSInclude;
	Include.SchemaLocation = "example.xsd";
	Schema.Directives.Add(Include);
	
	Return Schema;

EndFunction

Функция РезультатВключениеXS()
	Возврат
	"<schema elementFormDefault=""qualified"" targetNamespace=""http://www.w3.org/2001/05/XMLInfoset"" xmlns=""http://www.w3.org/2001/XMLSchema"">
	| 	<import namespace=""http://www.example.com/IPO"" />
	| 	<include schemaLocation=""example.xsd"" />
	|</schema>";
КонецФункции

Процедура ПроверитьВключениеXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Компоненты.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.Директивы.Количество(), 2);

	Импорт = Схема.Директивы.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(Импорт);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Импорт), Тип("ИмпортXS"));
	ЮнитТест.ПроверитьРавенство(Импорт.ПространствоИмен, "http://www.example.com/IPO");

	Включение = Схема.Директивы.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(Включение);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Включение), Тип("ВключениеXS"));
	ЮнитТест.ПроверитьРавенство(Включение.РасположениеСхемы, "example.xsd");

КонецПроцедуры	

Процедура ТестВключениеXS() Экспорт

	Схема = ПримерВключениеXS();
	Schema = ExampleXSInclude();
	СхемаТекст = СхемаXMLИзТекста(РезультатВключениеXS());
	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);

	ПроверитьВключениеXS(Схема);
	ПроверитьВключениеXS(Schema);
	ПроверитьВключениеXS(СхемаТекст);
	ПроверитьВключениеXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ДокументацияXS

// Источник:
// 	https://docs.microsoft.com/en-us/dotnet/api/system.xml.schema.xmlschemadocumentation
//
// Результат:
//	см. РезультатДокументацияXS

Функция ПримерДокументацияXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="northwestStates">
	ПростойТип = Новый ОпределениеПростогоТипаXS;
	ПростойТип.Имя = "northwestStates";
	Схема.Содержимое.Добавить(ПростойТип);
	
	// <xs:annotation>
	АннотацияNorthwestStates = Новый АннотацияXS;
	ПростойТип.Аннотация = АннотацияNorthwestStates;

	// <xs:documentation>States in the Pacific Northwest of US</xs:documentation>
	ДокументацияNorthwestStates = Новый ДокументацияXS;
	АннотацияNorthwestStates.Состав.Добавить(ДокументацияNorthwestStates);
	ДокументацияNorthwestStates.Источник = "States in the Pacific Northwest of US";
	//ДокументацияNorthwestStates.Markup = ТекстВСписокУзлов("States in the Pacific Northwest of US");

	// <xs:restriction base="xs:string">
	ПростойТип.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");

	// <xs:enumeration value="WA">
	ПеречислениеWA = Новый ФасетПеречисленияXS;
	ПростойТип.Фасеты.Добавить(ПеречислениеWA);
	ПеречислениеWA.Значение = "WA";

	// <xs:annotation>
	АннотацияWA =  Новый АннотацияXS;
	ПеречислениеWA.Аннотация = АннотацияWA;

	// <xs:documentation>Washington</documentation>
	ДокументацияWA = Новый ДокументацияXS;
	АннотацияWA.Состав.Добавить(ДокументацияWA);
	ДокументацияWA.Источник = "Washington";
	//ДокументацияWA.Markup = ТекстВСписокУзлов("Washington");

	// <xs:enumeration value="OR">
	ПеречислениеOR = Новый ФасетПеречисленияXS;
	ПростойТип.Фасеты.Добавить(ПеречислениеOR);
	ПеречислениеOR.Значение = "OR";

	// <xs:annotation>
	АннотацияOR = Новый АннотацияXS;
	ПеречислениеOR.Аннотация = АннотацияOR;
	
	// <xs:documentation>Oregon</xs:documentation>
	ДокументацияOR = Новый ДокументацияXS;
	АннотацияOR.Состав.Добавить(ДокументацияOR);
	ДокументацияOR.Источник = "Oregon";
	//ДокументацияOR.Markup = ТекстВСписокУзлов("Oregon");
	
	// <xs:enumeration value="ID">
	ПеречислениеID = Новый ФасетПеречисленияXS;
	ПростойТип.Фасеты.Добавить(ПеречислениеID);
	ПеречислениеID.Значение = "ID";

	// <xs:annotation>
	АннотацияID = Новый АннотацияXS;
	ПеречислениеID.Аннотация = АннотацияID;

	// <xs:documentation>Idaho</xs:documentation>
	ДокументацияID = Новый ДокументацияXS;
	АннотацияID.Состав.Добавить(ДокументацияID);
	ДокументацияID.Источник = "Idaho";
	//ДокументацияID.Markup = ТекстВСписокУзлов("Idaho");
	
	Возврат Схема;

КонецФункции

Function ExampleXSDocumentation()

	Schema = New XMLSchema;

	// <xs:simpleType name="northwestStates">
	SimpleType = New XSSimpleTypeDefinition;
	SimpleType.Name = "northwestStates";
	Schema.Content.Add(SimpleType);

	// <xs:annotation>
	AnnotationNorthwestStates = New XSAnnotation;
	SimpleType.Annotation = AnnotationNorthwestStates;

	// <xs:documentation>States in the Pacific Northwest of US</xs:documentation>
	DocumentationNorthwestStates = New XSDocumentation;
	AnnotationNorthwestStates.Content.Add(DocumentationNorthwestStates);
	DocumentationNorthwestStates.Source = "States in the Pacific Northwest of US";
	//DocumentationNorthwestStates.Markup = TextToNodeArray("States in the Pacific Northwest of US");

	// <xs:restriction base="xs:string">
	SimpleType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");

	// <xs:enumeration value="WA">
	EnumerationWA = New XSEnumerationFacet;
	EnumerationWA.Value = "WA";
	SimpleType.Facets.Add(EnumerationWA);

	// <xs:annotation>
	AnnotationWA = New XSAnnotation;
	EnumerationWA.Annotation = AnnotationWA;

	// <xs:documentation>Washington</documentation>
	DocumentationWA = New XSDocumentation;
	AnnotationWA.Content.Add(DocumentationWA);
	DocumentationWA.Source = "Washington";
	//DocumentationWA.Markup = TextToNodeArray("Washington");

	// <xs:enumeration value="OR">
	EnumerationOR = New XSEnumerationFacet;
	SimpleType.Facets.Add(EnumerationOR);
	EnumerationOR.Value = "OR";

	// <xs:annotation>
	AnnotationOR = New XSAnnotation;
	EnumerationOR.Annotation = AnnotationOR;

	// <xs:documentation>Oregon</xs:documentation>
	DocumentationOR = New XSDocumentation;
	AnnotationOR.Content.Add(DocumentationOR);
	DocumentationOR.Source = "Oregon";
	//DocumentationOR.Markup = TextToNodeArray("Oregon");

	// <xs:enumeration value="ID">
	EnumerationID = New XSEnumerationFacet;
	SimpleType.Facets.Add(EnumerationID);
	EnumerationID.Value = "ID";

	// <xs:annotation>
	AnnotationID = New XSAnnotation;
	EnumerationID.Annotation = AnnotationID;

	// <xs:documentation>Idaho</xs:documentation>
	DocumentationID = New XSDocumentation;
	AnnotationID.Content.Add(DocumentationID);
	DocumentationID.Source = "Idaho";
	//DocumentationID.Markup = TextToNodeArray("Idaho");
	
	Return Schema;

EndFunction

Процедура РезультатДокументацияXS()
	// <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	// 	<xs:simpleType name="northwestStates">
	// 	<xs:annotation>
	// 		<xs:documentation>States in the Pacific Northwest of US</xs:documentation>
	// 	</xs:annotation>
	// 	<xs:restriction base="xs:string">
	// 	  <xs:enumeration value="WA">
	// 		<xs:annotation>
	// 		  <xs:documentation>Washington</xs:documentation>
	// 		</xs:annotation>
	// 	  </xs:enumeration>
	// 	  <xs:enumeration value="OR">
	// 		<xs:annotation>
	// 		  <xs:documentation>Oregon</xs:documentation>
	// 		</xs:annotation>
	// 	  </xs:enumeration>
	// 	  <xs:enumeration value="ID">
	// 		<xs:annotation>
	// 		  <xs:documentation>Idaho</xs:documentation>
	// 		</xs:annotation>
	// 	  </xs:enumeration>
	// 	</xs:restriction>
	// </xs:simpleType>
	// </xs:schema>
КонецПроцедуры

#КонецОбласти

#Область ИнформацияДляПриложенияXS

// Источник:
// 	https://docs.microsoft.com/en-us/dotnet/api/system.xml.schema.xmlschemaappinfo
//
// Результат:
//	см. РезультатИнформацияДляПриложенияXS

Функция ПримерИнформацияДляПриложенияXS()
	
	Схема = Новый СхемаXML;
	
	// <xs:element name="State">
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "State";
	Схема.Содержимое.Добавить(Элемент);
	
	// <xs:annotation>
	АннотацияNorthwestStates = Новый АннотацияXS;
	Элемент.Аннотация = АннотацияNorthwestStates;
	
	// <xs:documentation>State Name</xs:documentation>
	ДокументацияNorthwestStates = Новый ДокументацияXS;
	АннотацияNorthwestStates.Состав.Добавить(ДокументацияNorthwestStates);
	ДокументацияNorthwestStates.Источник = "State Name";
	//ДокументацияNorthwestStates.Markup = TextToNodeArray("State Name");
	
	// <xs:appInfo>Application Information</xs:appInfo>
	ИнформацияДляПриложения = Новый ИнформацияДляПриложенияXS;
	АннотацияNorthwestStates.Состав.Добавить(ИнформацияДляПриложения);
	ИнформацияДляПриложения.Источник = "Application Information";
	// ИнформацияДляПриложения.Markup = TextToNodeArray("Application Information");
	
	Возврат Схема;
	
КонецФункции

Function ExampleXSAppInfo()

	Schema = New XMLSchema;
	
	// <xs:element name="State">
	Element = New XSElementDeclaration;
	Element.Name = "State";
	Schema.Content.Add(Element);
	
	// <xs:annotation>
	AnnotationNorthwestStates = New XSAnnotation;
	Element.Annotation = AnnotationNorthwestStates;
	
	// <xs:documentation>State Name</xs:documentation>
	DocumentationNorthwestStates = New XSDocumentation;
	AnnotationNorthwestStates.Content.Add(DocumentationNorthwestStates);
	DocumentationNorthwestStates.Source = "State Name";
	//DocumentationNorthwestStates.Markup = TextToNodeArray("State Name");
	
	// <xs:appInfo>Application Information</xs:appInfo>
	AppInfo = New XSAppInfo;
	AnnotationNorthwestStates.Content.Add(AppInfo);
	AppInfo.Source = "Application Information";
	//AppInfo.Markup = TextToNodeArray("Application Information");
	
	Return Schema;

EndFunction

Процедура РезультатИнформацияДляПриложенияXS()
	// <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	// 	<xs:element name="State">
	// 		<xs:annotation>
	// 			<xs:documentation source="State Name"/>
	// 			<xs:appinfo source="Application Information"/>
	// 		</xs:annotation>
	// 	</xs:element>
	// </xs:schema>
КонецПроцедуры

#КонецОбласти

#Область ОпределениеПростогоТипаXS 

// Источник:
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemasimpletype
//
// Результат:
//	см. РезультатОпределениеПростогоТипаXS

Функция ПримерОпределениеПростогоТипаXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="LotteryNumber">
	ТипLotteryNumber = Новый ОпределениеПростогоТипаXS;
	ТипLotteryNumber.Имя = "LotteryNumber";

	// <xs:restriction base="xs:int">
	ТипLotteryNumber.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "int");
	
	// <xs:minInclusive value="1"/>
	МинимальноеВключающееЗначения = Новый ФасетМинимальногоВключающегоЗначенияXS;
	МинимальноеВключающееЗначения.Значение = 1;
	ТипLotteryNumber.Фасеты.Добавить(МинимальноеВключающееЗначения);
	
	// <xs:maxInclusive value="99"/>
	МаксимальноеВключающееЗначения = Новый ФасетМаксимальногоВключающегоЗначенияXS;
	МаксимальноеВключающееЗначения.Значение = 99;
	ТипLotteryNumber.Фасеты.Добавить(МаксимальноеВключающееЗначения);
	
	Схема.Содержимое.Добавить(ТипLotteryNumber);
	
	// <xs:simpleType name="LotteryNumberList">
	ТипСписокLotteryNumber = Новый ОпределениеПростогоТипаXS;
	ТипСписокLotteryNumber.Имя = "LotteryNumberList";
	
	//// <xs:list itemType="LotteryNumber"/>
	ТипСписокLotteryNumber.Вариант = ВариантПростогоТипаXS.Список;
	ТипСписокLotteryNumber.ИмяТипаЭлемента = Новый РасширенноеИмяXML("", "LotteryNumber");
	
	Схема.Содержимое.Добавить(ТипСписокLotteryNumber);
	
	// <xs:simpleType name="LotteryNumbers">
	ТипLotteryNumbers = Новый ОпределениеПростогоТипаXS;
	ТипLotteryNumbers.Имя = "LotteryNumbers";
	
	// // <xs:restriction base="LotteryNumberList">
	ТипLotteryNumbers.ИмяБазовогоТипа = Новый РасширенноеИмяXML("", "LotteryNumberList");
	
	// <xs:length value="5"/>
	Длина = Новый ФасетДлиныXS;
	Длина.Значение = 5;
	ТипLotteryNumbers.Фасеты.Добавить(Длина);
	
	Схема.Содержимое.Добавить(ТипLotteryNumbers);
	
	// <xs:element name="TodaysLottery" type="LotteryNumbers">
	ЭлементTodaysLottery = Новый ОбъявлениеЭлементаXS;
	ЭлементTodaysLottery.Имя = "TodaysLottery";
	ЭлементTodaysLottery.ИмяТипа = Новый РасширенноеИмяXML("", "LotteryNumbers");
	
	Схема.Содержимое.Добавить(ЭлементTodaysLottery);
	
	Возврат Схема;

КонецФункции

Function ExampleXSSimpleTypeDefinition()

	schema = New XMLSchema;

	// <xs:simpleType name="LotteryNumber">
	LotteryNumberType = New XSSimpleTypeDefinition;
	LotteryNumberType.Name = "LotteryNumber";

	// <xs:restriction base="xs:int">
	LotteryNumberType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "int");
	
	// <xs:minInclusive value="1"/>
	minInclusive = New XSMinInclusiveFacet;
	minInclusive.Value = 1;
	LotteryNumberType.Facets.Add(minInclusive);
	
	// <xs:maxInclusive value="99"/>
	maxInclusive = New XSMaxInclusiveFacet;
	maxInclusive.Value = 99;
	LotteryNumberType.Facets.Add(maxInclusive);
	
	schema.Content.Add(LotteryNumberType);
	
	// // <xs:simpleType name="LotteryNumberList">
	LotteryNumberListType = New XSSimpleTypeDefinition;
	LotteryNumberListType.Name = "LotteryNumberList";
	
	//// <xs:list itemType="LotteryNumber"/>
	LotteryNumberListType.Variety = XSSimpleTypeVariety.List;
	LotteryNumberListType.ItemTypeName = New XMLExpandedName("", "LotteryNumber");
	
	schema.Content.Add(LotteryNumberListType);
	
	// <xs:simpleType name="LotteryNumbers">
	LotteryNumbersType = New XSSimpleTypeDefinition;
	LotteryNumbersType.Name = "LotteryNumbers";
	
	// // <xs:restriction base="LotteryNumberList">
	LotteryNumbersType.BaseTypeName = New XMLExpandedName("", "LotteryNumberList");
	
	// <xs:length value="5"/>
	length = New XSLengthFacet;
	length.Value = 5;
	LotteryNumbersType.Facets.Add(length);
	
	schema.Content.Add(LotteryNumbersType);
	
	// <xs:element name="TodaysLottery" type="LotteryNumbers">
	TodaysLottery = New XSElementDeclaration;
	TodaysLottery.Name = "TodaysLottery";
	TodaysLottery.TypeName = New XMLExpandedName("", "LotteryNumbers");
	
	schema.Content.Add(TodaysLottery);
	
	return schema;
	
EndFunction

Процедура РезультатОпределениеПростогоТипаXS()
	// <xs:schema  xmlns:xs="http://www.w3.org/2001/XMLSchema">
	// 	<xs:simpleType name="LotteryNumber">
	// 		<xs:restriction base="xs:int">
	// 			<xs:minInclusive value="1"/>
	// 			<xs:maxInclusive value="99"/>
	// 		</xs:restriction>
	// 	</xs:simpleType>
	//
	// 	<xs:simpleType name="LotteryNumberList">
	// 		<xs:list itemType="LotteryNumber"/>
	// 	</xs:simpleType>
	//	
	// 	<xs:simpleType name="LotteryNumbers">
	// 		<xs:restriction base="LotteryNumberList">
	// 			<xs:length value="5"/>
	// 		</xs:restriction>
	// 	</xs:simpleType>
	//	
	// 	<xs:element name="TodaysLottery" type="LotteryNumbers">
	// 	</xs:element>
	//	
	// </xs:schema>
КонецПроцедуры

#КонецОбласти

#Область ОпределениеПростогоТипаXS_Объединение

// Источник:
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemasimpletypeunion
//
// Результат:
//	см. РезультатОпределениеПростогоТипаXS_Объединение

Функция ПримерОпределениеПростогоТипаXS_Объединение()

	Схема = Новый СхемаXML;

	//<xs:simpleType name="StringOrIntType">
	ТипСтрокаИлиЧисло = Новый ОпределениеПростогоТипаXS;
	ТипСтрокаИлиЧисло.Имя = "StringOrIntType";
	
	// <xs:union>
	ТипСтрокаИлиЧисло.Вариант = ВариантПростогоТипаXS.Объединение;
	Схема.Содержимое.Добавить(ТипСтрокаИлиЧисло);
	
	// <xs:simpleType>
	ТипСтрока = Новый ОпределениеПростогоТипаXS;
	
	// <xs:restriction base="xs:string"/>
	ТипСтрока.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	ТипСтрокаИлиЧисло.ОпределенияТиповОбъединения.Добавить(ТипСтрока);
		
	// <xs:simpleType>
	ТипЧисло = Новый ОпределениеПростогоТипаXS;
	
	// <xs:restriction base="xs:int"/>
	ТипЧисло.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "int");
	ТипСтрокаИлиЧисло.ОпределенияТиповОбъединения.Добавить(ТипЧисло);
	
	// <xs:element name="size" type="StringOrIntType"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "size";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("", "StringOrIntType");
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSSimpleTypeDefinition_Union()

	schema = New XMLSchema;

	//<xs:simpleType name="StringOrIntType">
	StringOrIntType = New XSSimpleTypeDefinition;
	StringOrIntType.Name = "StringOrIntType";
	
	// <xs:union>
	StringOrIntType.Variety = XSSimpleTypeVariety.Union;
	schema.Content.Add(StringOrIntType);
	
	// <xs:simpleType>
	simpleType1 = New XSSimpleTypeDefinition;
	
	// <xs:restriction base="xs:string"/>
	simpleType1.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	StringOrIntType.MemberTypeDefinitions.Add(simpleType1);
		
	// <xs:simpleType>
	simpleType2 = New XSSimpleTypeDefinition;
	
	// <xs:restriction base="xs:int"/>
	simpleType2.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "int");
	StringOrIntType.MemberTypeDefinitions.Add(simpleType2);
	
	// <xs:element name="size" type="StringOrIntType"/>
	elementSize = New XSElementDeclaration;
	elementSize.Name = "size";
	elementSize.TypeName = New XMLExpandedName("", "StringOrIntType");
	schema.Content.Add(elementSize);
	
	return schema;
	
EndFunction

Процедура РезультатОпределениеПростогоТипаXS_Объединение()
	//<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">

	//    <xs:simpleType name="StringOrIntType">
	//        <xs:union>
	//            <xs:simpleType>
	//                <xs:restriction base="xs:string"/>
	//            </xs:simpleType>
	//    
	//            <xs:simpleType>
	//                <xs:restriction base="xs:int"/>
	//            </xs:simpleType>
	//        </xs:union>
	//    </xs:simpleType>
	//    
	//    <xs:element name="size" type="StringOrIntType"/>
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьОпределениеПростогоТипаXS_Объединение(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ТипСтрокаИлиЧисло = Схема.ОпределенияТипов.Получить("StringOrIntType");
	ЮнитТест.ПроверитьЗаполненность(ТипСтрокаИлиЧисло);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипСтрокаИлиЧисло), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипСтрокаИлиЧисло.Имя, "StringOrIntType");
	ЮнитТест.ПроверитьРавенство(ТипСтрокаИлиЧисло.Вариант, ВариантПростогоТипаXS.Объединение);
	ЮнитТест.ПроверитьРавенство(ТипСтрокаИлиЧисло.ОпределенияТиповОбъединения.Количество(), 2);

	ТипСтрока = ТипСтрокаИлиЧисло.ОпределенияТиповОбъединения.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ТипСтрока);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипСтрока), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипСтрока.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипСтрока.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	ТипЧисло = ТипСтрокаИлиЧисло.ОпределенияТиповОбъединения.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(ТипЧисло);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипЧисло), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипЧисло.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипЧисло.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "int"));

	Элемент = Схема.ОбъявленияЭлементов.Получить("size"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "size");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("", "StringOrIntType"));

КонецПроцедуры	

#КонецОбласти

#Область ФасетДлиныXS 

// Источник:
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemalengthfacet
//
// Результат:
//	см. РезультатФасетДлиныXS

Функция ПримерФасетДлиныXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="ZipCodeType">
	ТипПочтовыйИндекс = Новый ОпределениеПростогоТипаXS;
	ТипПочтовыйИндекс.Имя = "ZipCodeType";
	
	// <xs:restriction base="xs:string">
	ТипПочтовыйИндекс.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:length value="5"/>
	Длина = Новый ФасетДлиныXS;
	Длина.Значение = 5;
	ТипПочтовыйИндекс.Фасеты.Добавить(Длина);
	
	Схема.Содержимое.Добавить(ТипПочтовыйИндекс);
	
	// <xs:element name="Address">
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "Address";
	
	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	
	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	АтрибутПочтовыйИндекс = Новый ОбъявлениеАтрибутаXS;
	АтрибутПочтовыйИндекс.Имя = "ZipCode";
	АтрибутПочтовыйИндекс.ИмяТипа = Новый РасширенноеИмяXML("", "ZipCodeType");
	СоставнойТип.Атрибуты.Добавить(АтрибутПочтовыйИндекс);
	
	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSLengthFacet()

	schema = New XMLSchema;

	// <xs:simpleType name="ZipCodeType">
	ZipCodeType = New XSSimpleTypeDefinition;
	ZipCodeType.Name = "ZipCodeType";
	
	// <xs:restriction base="xs:string">
	ZipCodeType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:length value="5"/>
	length = New XSLengthFacet;
	length.Value = 5;
	ZipCodeType.Facets.Add(length);
	
	schema.Content.Add(ZipCodeType);
	
	// <xs:element name="Address">
	element = New XSElementDeclaration;
	element.Name = "Address";
	
	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;
	
	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	ZipCodeAttribute = New XSAttributeDeclaration;
	ZipCodeAttribute.Name = "ZipCode";
	ZipCodeAttribute.TypeName = New XMLExpandedName("", "ZipCodeType");
	complexType.Attributes.Add(ZipCodeAttribute);
	
	element.AnonymousTypeDefinition = complexType;
	schema.Content.Add(element);
	
	return schema;
	
EndFunction

Процедура РезультатФасетДлиныXS()
	//<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	//
	//	<xs:simpleType name="ZipCodeType">
	//		<xs:restriction base="xs:string">
	//			<xs:length value="5"/>
	//		</xs:restriction>
	//	</xs:simpleType>
	//
	//	<xs:element name="Address">
	//		<xs:complexType>
	//			<xs:attribute name="ZipCode" type="ZipCodeType"/>
	//		</xs:complexType>
	//	</xs:element>
	//
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьФасетДлиныXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ТипПочтовыйИндекс = Схема.ОпределенияТипов.Получить("ZipCodeType");
	ЮнитТест.ПроверитьЗаполненность(ТипПочтовыйИндекс);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипПочтовыйИндекс), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Имя, "ZipCodeType");
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Фасеты.Количество(), 1);

	Длина = ТипПочтовыйИндекс.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(Длина);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Длина), Тип("ФасетДлиныXS"));
	ЮнитТест.ПроверитьРавенство(Длина.Значение, 5);

	Элемент = Схема.ОбъявленияЭлементов.Получить("Address"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "Address");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутПочтовыйИндекс = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутПочтовыйИндекс);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутПочтовыйИндекс), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутПочтовыйИндекс.Имя, "ZipCode");
	ЮнитТест.ПроверитьРавенство(АтрибутПочтовыйИндекс.ИмяТипа, Новый РасширенноеИмяXML("", "ZipCodeType"));

КонецПроцедуры

#КонецОбласти

#Область ФасетМинимальнойДлиныXS 

// Источник:
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemaminlengthfacet
//
// Результат:
//	см. РезультатФасетМинимальнойДлиныXS

Функция ПримерФасетМинимальнойДлиныXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="ZipCodeType">
	ТипПочтовыйИндекс = Новый ОпределениеПростогоТипаXS;
	ТипПочтовыйИндекс.Имя = "ZipCodeType";
	
	// <xs:restriction base="xs:string">
	ТипПочтовыйИндекс.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:minLength value="5"/>
	МинимальнаяДлина = Новый ФасетМинимальнойДлиныXS;
	МинимальнаяДлина.Значение = 5;
	ТипПочтовыйИндекс.Фасеты.Добавить(МинимальнаяДлина);
	
	Схема.Содержимое.Добавить(ТипПочтовыйИндекс);
	
	// <xs:element name="Address">
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "Address";
	
	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	
	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	АтрибутПочтовыйИндекс = Новый ОбъявлениеАтрибутаXS;
	АтрибутПочтовыйИндекс.Имя = "ZipCode";
	АтрибутПочтовыйИндекс.ИмяТипа = Новый РасширенноеИмяXML("", "ZipCodeType");
	СоставнойТип.Атрибуты.Добавить(АтрибутПочтовыйИндекс);
	
	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSMinLengthFacet()

	schema = New XMLSchema;

	// <xs:simpleType name="ZipCodeType">
	ZipCodeType = New XSSimpleTypeDefinition;
	ZipCodeType.Name = "ZipCodeType";
	
	// <xs:restriction base="xs:string">
	ZipCodeType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:minLength value="5"/>
	minLength = New XSMinLengthFacet;
	minLength.Value = 5;
	ZipCodeType.Facets.Add(minLength);
	
	schema.Content.Add(ZipCodeType);	
	
	// <xs:element name="Address">
	element = New XSElementDeclaration;
	element.Name = "Address";
	
	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;
	
	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	ZipCodeAttribute = New XSAttributeDeclaration;
	ZipCodeAttribute.Name = "ZipCode";
	ZipCodeAttribute.TypeName = New XMLExpandedName("", "ZipCodeType");
	complexType.Attributes.Add(ZipCodeAttribute);
	
	element.AnonymousTypeDefinition = complexType;
	schema.Content.Add(element);
	
	return schema;
	
EndFunction

Процедура РезультатФасетМинимальнойДлиныXS()
	//<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	//
	//	<xs:simpleType name="ZipCodeType">
	//		<xs:restriction base="xs:string">
	//			<xs:minLength value="5"/>
	//		</xs:restriction>
	//	</xs:simpleType>
	//
	//	<xs:element name="Address">
	//		<xs:complexType>
	//			<xs:attribute name="ZipCode" type="ZipCodeType"/>
	//		</xs:complexType>
	//	</xs:element>
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьФасетМинимальнойДлиныXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ТипПочтовыйИндекс = Схема.ОпределенияТипов.Получить("ZipCodeType");
	ЮнитТест.ПроверитьЗаполненность(ТипПочтовыйИндекс);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипПочтовыйИндекс), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Имя, "ZipCodeType");
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Фасеты.Количество(), 1);

	МинимальнаяДлина = ТипПочтовыйИндекс.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(МинимальнаяДлина);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(МинимальнаяДлина), Тип("ФасетМинимальнойДлиныXS"));
	ЮнитТест.ПроверитьРавенство(МинимальнаяДлина.Значение, 5);

	Элемент = Схема.ОбъявленияЭлементов.Получить("Address"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "Address");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутПочтовыйИндекс = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутПочтовыйИндекс);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутПочтовыйИндекс), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутПочтовыйИндекс.Имя, "ZipCode");
	ЮнитТест.ПроверитьРавенство(АтрибутПочтовыйИндекс.ИмяТипа, Новый РасширенноеИмяXML("", "ZipCodeType"));

КонецПроцедуры

#КонецОбласти

#Область ФасетМаксимальнойДлиныXS 

// Источник:
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemamaxlengthfacet
//
// Результат:
//	см. РезультатФасетМаксимальнойДлиныXS

Функция ПримерФасетМаксимальнойДлиныXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="ZipCodeType">
	ТипПочтовыйИндекс = Новый ОпределениеПростогоТипаXS;
	ТипПочтовыйИндекс.Имя = "ZipCodeType";
	
	// <xs:restriction base="xs:string">
	ТипПочтовыйИндекс.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:maxLength value="10"/>
	МаксимальнаяДлина = Новый ФасетМаксимальнойДлиныXS;
	МаксимальнаяДлина.Значение = 10;
	ТипПочтовыйИндекс.Фасеты.Добавить(МаксимальнаяДлина);
	
	Схема.Содержимое.Добавить(ТипПочтовыйИндекс);
	
	// <xs:element name="Address">
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "Address";
	
	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	
	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	АтрибутПочтовыйИндекс = Новый ОбъявлениеАтрибутаXS;
	АтрибутПочтовыйИндекс.Имя = "ZipCode";
	АтрибутПочтовыйИндекс.ИмяТипа = Новый РасширенноеИмяXML("", "ZipCodeType");
	СоставнойТип.Атрибуты.Добавить(АтрибутПочтовыйИндекс);
	
	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSMaxLengthFacet()

	schema = New XMLSchema;

	// <xs:simpleType name="ZipCodeType">
	ZipCodeType = New XSSimpleTypeDefinition;
	ZipCodeType.Name = "ZipCodeType";
	
	// <xs:restriction base="xs:string">
	ZipCodeType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:maxLength value="10"/>
	minLength = New XSMaxLengthFacet;
	minLength.Value = 10;
	ZipCodeType.Facets.Add(minLength);
	
	schema.Content.Add(ZipCodeType);	
	
	// <xs:element name="Address">
	element = New XSElementDeclaration;
	element.Name = "Address";
	
	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;
	
	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	ZipCodeAttribute = New XSAttributeDeclaration;
	ZipCodeAttribute.Name = "ZipCode";
	ZipCodeAttribute.TypeName = New XMLExpandedName("", "ZipCodeType");
	complexType.Attributes.Add(ZipCodeAttribute);
	
	element.AnonymousTypeDefinition = complexType;
	schema.Content.Add(element);
	
	return schema;
	
EndFunction

Процедура РезультатФасетМаксимальнойДлиныXS()
	//<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	//
	//	<xs:simpleType name="ZipCodeType">
	//		<xs:restriction base="xs:string">
	//			<xs:maxLength value="10"/>
	//		</xs:restriction>
	//	</xs:simpleType>
	//
	//	<xs:element name="Address">
	//		<xs:complexType>
	//			<xs:attribute name="ZipCode" type="ZipCodeType"/>
	//		</xs:complexType>
	//	</xs:element>
	//
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьФасетМаксимальнойДлиныXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ТипПочтовыйИндекс = Схема.ОпределенияТипов.Получить("ZipCodeType");
	ЮнитТест.ПроверитьЗаполненность(ТипПочтовыйИндекс);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипПочтовыйИндекс), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Имя, "ZipCodeType");
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));
	ЮнитТест.ПроверитьРавенство(ТипПочтовыйИндекс.Фасеты.Количество(), 1);

	МаксимальнаяДлина = ТипПочтовыйИндекс.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(МаксимальнаяДлина);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(МаксимальнаяДлина), Тип("ФасетМаксимальнойДлиныXS"));
	ЮнитТест.ПроверитьРавенство(МаксимальнаяДлина.Значение, 10);

	Элемент = Схема.ОбъявленияЭлементов.Получить("Address"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "Address");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутПочтовыйИндекс = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутПочтовыйИндекс);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутПочтовыйИндекс), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутПочтовыйИндекс.Имя, "ZipCode");
	ЮнитТест.ПроверитьРавенство(АтрибутПочтовыйИндекс.ИмяТипа, Новый РасширенноеИмяXML("", "ZipCodeType"));

КонецПроцедуры

#КонецОбласти

#Область ФасетКоличестваРазрядовДробнойЧастиXS 

// Источник:
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemafractiondigitsfacet
//
// Результат:
//	см. РезультатФасетКоличестваРазрядовДробнойЧастиXS

Функция ПримерФасетКоличестваРазрядовДробнойЧастиXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="RatingType">
	ТипРейтинг = Новый ОпределениеПростогоТипаXS;;
	ТипРейтинг.Имя = "RatingType";
	
	// <xs:restriction base="xs:number">
	ТипРейтинг.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "decimal");
	
	// <xs:totalDigits value="2"/>
	ВсегоРазрядов = Новый ФасетОбщегоКоличестваРазрядовXS;
	ВсегоРазрядов.Значение = 2;
	ТипРейтинг.Фасеты.Добавить(ВсегоРазрядов);
	
	// <xs:fractionDigits value="1"/>
	ДробныхРазрядов = Новый ФасетКоличестваРазрядовДробнойЧастиXS;
	ДробныхРазрядов.Значение = 1;
	ТипРейтинг.Фасеты.Добавить(ДробныхРазрядов);
	
	Схема.Содержимое.Добавить(ТипРейтинг);
	
	// <xs:element name="movie">
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "movie";
	
	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	
	// <xs:attribute name="rating" type="RatingType"/>
	АтрибутРейтинг = Новый ОбъявлениеАтрибутаXS;
	АтрибутРейтинг.Имя = "rating";
	АтрибутРейтинг.ИмяТипа = Новый РасширенноеИмяXML("", "RatingType");
	СоставнойТип.Атрибуты.Добавить(АтрибутРейтинг);
	
	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSFractionDigitsFacet()

	schema = New XMLSchema;

	// <xs:simpleType name="RatingType">
	RatingType = New XSSimpleTypeDefinition;
	RatingType.Name = "RatingType";
	
	// <xs:restriction base="xs:number">
	RatingType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "decimal");
	
	// <xs:totalDigits value="2"/>
	totalDigits = New XSTotalDigitsFacet;
	totalDigits.Value = 2;
	RatingType.Facets.Add(totalDigits);
	
	// <xs:fractionDigits value="1"/>
	fractionDigits = New XSFractionDigitsFacet;
	fractionDigits.Value = 1;
	RatingType.Facets.Add(fractionDigits);
	
	schema.Content.Add(RatingType);
	
	// <xs:element name="movie">
	element = New XSElementDeclaration;
	element.Name = "movie";
	
	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;
	
	// <xs:attribute name="rating" type="RatingType"/>
	ratingAttribute = New XSAttributeDeclaration;
	ratingAttribute.Name = "rating";
	ratingAttribute.TypeName = New XMLExpandedName("", "RatingType");
	complexType.Attributes.Add(ratingAttribute);
	
	element.AnonymousTypeDefinition = complexType;
	schema.Content.Add(element);
	
	return schema;
	
EndFunction

Процедура РезультатФасетКоличестваРазрядовДробнойЧастиXS()
	//<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	//
	//	<xs:simpleType name="RatingType">
	//		<xs:restriction base="xs:decimal">
	//		    <xs:totalDigits value="2"/>
	//			<xs:fractionDigits value="1"/>
	//		</xs:restriction>
	//	</xs:simpleType>
	//
	//	<xs:element name="movie">
	//		<xs:complexType>
	//			<xs:attribute name="rating" type="RatingType"/>
	//		</xs:complexType>
	//	</xs:element>
	//
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьФасетКоличестваРазрядовДробнойЧастиXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ТипРейтинг = Схема.ОпределенияТипов.Получить("RatingType");
	ЮнитТест.ПроверитьЗаполненность(ТипРейтинг);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипРейтинг), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипРейтинг.Имя, "RatingType");
	ЮнитТест.ПроверитьРавенство(ТипРейтинг.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипРейтинг.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "decimal"));
	ЮнитТест.ПроверитьРавенство(ТипРейтинг.Фасеты.Количество(), 2);

	ВсегоРазрядов = ТипРейтинг.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ВсегоРазрядов);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ВсегоРазрядов), Тип("ФасетОбщегоКоличестваРазрядовXS"));
	ЮнитТест.ПроверитьРавенство(ВсегоРазрядов.Значение, 2);
	
	ДробныхРазрядов = ТипРейтинг.Фасеты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(ДробныхРазрядов);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ДробныхРазрядов), Тип("ФасетКоличестваРазрядовДробнойЧастиXS"));
	ЮнитТест.ПроверитьРавенство(ДробныхРазрядов.Значение, 1);
	
	Элемент = Схема.ОбъявленияЭлементов.Получить("movie"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "movie");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутРейтинг = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутРейтинг);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутРейтинг), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутРейтинг.Имя, "rating");
	ЮнитТест.ПроверитьРавенство(АтрибутРейтинг.ИмяТипа, Новый РасширенноеИмяXML("", "RatingType"));

КонецПроцедуры

#КонецОбласти

#Область ФасетМинимальногоИсключающегоЗначенияXS 

// Источник:
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemaminexclusivefacet
//	https://docs.microsoft.com/ru-ru/dotnet/api/system.xml.schema.xmlschemamaxexclusivefacet
//
// Результат:
//	см. РезультатФасетМаксимальногоИсключающегоЗначенияXS 

Функция ПримерФасетМинимальногоИсключающегоЗначенияXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="WaitQueueLengthType">
	ТипДлинаОчереди = Новый ОпределениеПростогоТипаXS;
	ТипДлинаОчереди.Имя = "WaitQueueLengthType";
	
	// <xs:restriction base="xs:int">
	ТипДлинаОчереди.BaseTypeName = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "int");
	
	// <xs:minExclusive value="5"/>
	МинимальноИсключая = Новый ФасетМинимальногоИсключающегоЗначенияXS;
	МинимальноИсключая.Значение = 5;
	ТипДлинаОчереди.Фасеты.Добавить(МинимальноИсключая);
	
	// <xs:maxExclusive value="10"/>
	МаксимальноИсключая = Новый ФасетМаксимальногоИсключающегоЗначенияXS ;
	МаксимальноИсключая.Значение = 10;
	ТипДлинаОчереди.Фасеты.Добавить(МаксимальноИсключая);
	
	Схема.Содержимое.Добавить(ТипДлинаОчереди);
	
	// <xs:element name="Lobby">
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "Lobby";
	
	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	
	// <xs:attribute name="WaitQueueLength" type="WaitQueueLengthType"/>
	АтрибутДлинаОчереди = Новый ОбъявлениеАтрибутаXS;
	АтрибутДлинаОчереди.Имя = "WaitQueueLength";
	АтрибутДлинаОчереди.ИмяТипа = Новый РасширенноеИмяXML("", "WaitQueueLengthType");
	СоставнойТип.Атрибуты.Добавить(АтрибутДлинаОчереди);
	
	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSMinExclusiveFacet()

	schema = New XMLSchema;

	// <xs:simpleType name="WaitQueueLengthType">
	WaitQueueLengthType = New XSSimpleTypeDefinition;
	WaitQueueLengthType.Name = "WaitQueueLengthType";
	
	// <xs:restriction base="xs:int">
	WaitQueueLengthType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "int");
	
	// <xs:minExclusive value="5"/>
	MinExclusive = New XSMinExclusiveFacet;
	MinExclusive.Value = 5;
	WaitQueueLengthType.Facets.Add(MinExclusive);
	
	// <xs:maxExclusive value="10"/>
	MaxExclusive = New XSMaxExclusiveFacet;
	MaxExclusive.Value = 10;
	WaitQueueLengthType.Facets.Add(MaxExclusive);
	
	schema.Content.Add(WaitQueueLengthType);
	
	// <xs:element name="Lobby">
	element = New XSElementDeclaration;
	element.Name = "Lobby";
	
	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;
	
	// <xs:attribute name="WaitQueueLength" type="WaitQueueLengthType"/>
	WaitQueueLengthAttribute = New XSAttributeDeclaration;
	WaitQueueLengthAttribute.Name = "WaitQueueLength";
	WaitQueueLengthAttribute.TypeName = New XMLExpandedName("", "WaitQueueLengthType");
	complexType.Attributes.Add(WaitQueueLengthAttribute);
	
	element.AnonymousTypeDefinition = complexType;
	schema.Content.Add(element);
	
	return schema;
	
EndFunction

Процедура РезультатФасетМинимальногоИсключающегоЗначенияXS()
	// 	<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	// 	<xs:simpleType name="WaitQueueLengthType">
	// 	  <xs:restriction base="xs:int">
	// 		<xs:minExclusive value="5" />
	// 		<xs:maxExclusive value="10" />
	// 	  </xs:restriction>
	// 	</xs:simpleType>
	// 	<xs:element name="Lobby">
	// 	  <xs:complexType>
	// 		<xs:attribute name="WaitQueueLength" type="WaitQueueLengthType" />
	// 	  </xs:complexType>
	// 	</xs:element>
	//   </xs:schema>
КонецПроцедуры

Процедура ПроверитьФасетМинимальногоИсключающегоЗначенияXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ТипДлинаОчереди = Схема.ОпределенияТипов.Получить("WaitQueueLengthType");
	ЮнитТест.ПроверитьЗаполненность(ТипДлинаОчереди);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипДлинаОчереди), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипДлинаОчереди.Имя, "WaitQueueLengthType");
	ЮнитТест.ПроверитьРавенство(ТипДлинаОчереди.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипДлинаОчереди.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "int"));
	ЮнитТест.ПроверитьРавенство(ТипДлинаОчереди.Фасеты.Количество(), 2);

	МинимальноИсключая = ТипДлинаОчереди.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(МинимальноИсключая);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(МинимальноИсключая), Тип("ФасетМинимальногоИсключающегоЗначенияXS"));
	ЮнитТест.ПроверитьРавенство(МинимальноИсключая.Значение, 5);
	
	МаксимальноИсключая = ТипДлинаОчереди.Фасеты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(МаксимальноИсключая);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(МаксимальноИсключая), Тип("ФасетМаксимальногоИсключающегоЗначенияXS"));
	ЮнитТест.ПроверитьРавенство(МаксимальноИсключая.Значение, 10);
	
	Элемент = Схема.ОбъявленияЭлементов.Получить("Lobby"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "Lobby");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутДлинаОчереди = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутДлинаОчереди);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутДлинаОчереди), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутДлинаОчереди.Имя, "WaitQueueLength");
	ЮнитТест.ПроверитьРавенство(АтрибутДлинаОчереди.ИмяТипа, Новый РасширенноеИмяXML("", "WaitQueueLengthType"));

КонецПроцедуры

#КонецОбласти

#Область МаскаXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemaany
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemaanyattribute
//
// Результат:
//	см. РезультатМаскаXS 

Функция ПримерМаскаXS()

	Схема = Новый СхемаXML;

	// <element name='htmlText'>
	ЭлементТекстHTML = Новый ОбъявлениеЭлементаXS;
	ЭлементТекстHTML.Имя = "htmlText";
	
	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	
	// <xs:sequence>
	ГруппаМодели = Новый ГруппаМоделиXS;
	ГруппаМодели.ВидГруппы = ВидГруппыМоделиXS.Последовательность;
	СоставнойТип.Содержимое = ГруппаМодели;
	
	// <any namespace='http://www.w3.org/1999/xhtml'
	//    minOccurs='1' maxOccurs='unbounded'
	//    processContents='lax'/>
	Маска = Новый МаскаXS;
	Маска.ВидОбработкиСодержимого = ОбработкаСодержимогоXS.Слабая;
	Маска.ЛексическоеЗначениеОграниченияПространствИмен	= "http://www.w3.org/1999/xhtml";

	Фрагмент = Новый ФрагментXS;
	Фрагмент.МинимальноВходит = 1;
	Фрагмент.МаксимальноВходит = -1;
	Фрагмент.Часть = Маска;
	ГруппаМодели.Фрагменты.Добавить(Фрагмент);
	
	ЭлементТекстHTML.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(ЭлементТекстHTML);
	
	// <xs:element name="stringElementWithAnyAttribute">
	ЭлементЛюбойАтрибут = Новый ОбъявлениеЭлементаXS;
	ЭлементЛюбойАтрибут.Name = "stringElementWithAnyAttribute"; 

	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	
	// <xs:simpleContent>
	СоставнойТип.МодельСодержимого = МодельСодержимогоXS.Простая;
	
	// <xs:extension base="xs:string">
	СоставнойТип.МетодНаследования = МетодНаследованияXS.Расширение;
	СоставнойТип.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:anyAttribute namespace="##targetNamespace"/>	
	Маска = Новый МаскаXS;
	Маска.ЛексическоеЗначениеОграниченияПространствИмен	= "##targetNamespace";
	СоставнойТип.МаскаАтрибутов = Маска;
	
	ЭлементЛюбойАтрибут.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(ЭлементЛюбойАтрибут);
	
	Возврат Схема;

КонецФункции

Function ExampleXSWildcard()

	schema = New XMLSchema;

	// <element name='htmlText'>
	elementHtmlText = New XSElementDeclaration;
	elementHtmlText.Name = "htmlText";
	
	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;
	
	// <xs:sequence>
	ModelGroup = New XSModelGroup;
	ModelGroup.Compositor = XSCompositor.Sequence;
	complexType.Content = ModelGroup;
	
	// <any namespace='http://www.w3.org/1999/xhtml'
	//    minOccurs='1' maxOccurs='unbounded'
	//    processContents='lax'/>
	Wildcard = New XSWildcard;
	Wildcard.ProcessContents = XSProcessContents.Lax;
	Wildcard.LexicalNamespaceConstraint	= "http://www.w3.org/1999/xhtml";

	Particle = new XSParticle;
	Particle.MinOccurs = 1;
	Particle.MaxOccurs = -1;
	Particle.Term = Wildcard;
	ModelGroup.Particles.Add(Particle);
	
	elementHtmlText.AnonymousTypeDefinition = complexType;
	schema.Content.Add(elementHtmlText);
	
	// <xs:element name="stringElementWithAnyAttribute">
	elementAnyAttribute = New XSElementDeclaration;
	elementAnyAttribute.Name = "stringElementWithAnyAttribute"; 
	
	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;
	
	// <xs:simpleContent>
	complexType.ContentModel = XSContentModel.Simple;
	
	// <xs:extension base="xs:string">
	complexType.DerivationMethod = XSDerivationMethod.Extension;
	complexType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	
	// <xs:anyAttribute namespace="##targetNamespace"/>	
	Wildcard = New XSWildcard;
	Wildcard.LexicalNamespaceConstraint	= "##targetNamespace";
	complexType.AttributeWildcard = Wildcard;
	
	elementAnyAttribute.AnonymousTypeDefinition = complexType;
	schema.Content.Add(elementAnyAttribute);
	
	return schema;
	
EndFunction

Процедура РезультатМаскаXS()
	//<xs:schema  xmlns:xs="http://www.w3.org/2001/XMLSchema">
	//
	//	<xs:element name="htmlText">
	//		<xs:complexType>
	//			<xs:sequence>
	//				<xs:any 
	//					namespace='http://www.w3.org/1999/xhtml'
	//					processContents='lax'
	//              	minOccurs='1' maxOccurs='unbounded'/>
	//			</xs:sequence>
	//		</xs:complexType>
	//	</xs:element>
	//	
	//	<xs:element name="stringElementWithAnyAttribute">
	//        <xs:complexType>
	//            <xs:simpleContent>
	//                <xs:extension base="xs:string">
	//                    <xs:anyAttribute namespace="##targetNamespace"/>
	//                </xs:extension>
	//            </xs:simpleContent>
	//        </xs:complexType>
	//    </xs:element>
	//	
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьМаскаXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 2);

	ЭлементТекстHTML = Схема.ОбъявленияЭлементов.Получить("htmlText"); 
	ЮнитТест.ПроверитьЗаполненность(ЭлементТекстHTML);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЭлементТекстHTML), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(ЭлементТекстHTML.Имя, "htmlText");

	СоставнойТип = ЭлементТекстHTML.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));

	ГруппаМодели = СоставнойТип.Содержимое;
	ЮнитТест.ПроверитьЗаполненность(ГруппаМодели);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ГруппаМодели), Тип("ГруппаМоделиXS"));
	ЮнитТест.ПроверитьРавенство(ГруппаМодели.ВидГруппы, ВидГруппыМоделиXS.Последовательность);
	ЮнитТест.ПроверитьРавенство(ГруппаМодели.Фрагменты.Количество(), 1);

	Фрагмент = ГруппаМодели.Фрагменты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(Фрагмент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Фрагмент), Тип("ФрагментXS"));
	ЮнитТест.ПроверитьРавенство(Фрагмент.МинимальноВходит, 1);
	ЮнитТест.ПроверитьРавенство(Фрагмент.МаксимальноВходит, -1);
	
	Маска = Фрагмент.Часть; 
	ЮнитТест.ПроверитьЗаполненность(Маска);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Маска), Тип("МаскаXS"));
	ЮнитТест.ПроверитьРавенство(Маска.ВидОбработкиСодержимого, ОбработкаСодержимогоXS.Слабая);
	ЮнитТест.ПроверитьРавенство(Маска.ЛексическоеЗначениеОграниченияПространствИмен, "http://www.w3.org/1999/xhtml");

	ЭлементЛюбойАтрибут = Схема.ОбъявленияЭлементов.Получить("stringElementWithAnyAttribute"); 
	ЮнитТест.ПроверитьЗаполненность(ЭлементЛюбойАтрибут);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЭлементЛюбойАтрибут), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(ЭлементЛюбойАтрибут.Имя, "stringElementWithAnyAttribute");

	СоставнойТип = ЭлементЛюбойАтрибут.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.МодельСодержимого, МодельСодержимогоXS.Простая);
	ЮнитТест.ПроверитьРавенство(СоставнойТип.МетодНаследования, МетодНаследованияXS.Расширение);
	ЮнитТест.ПроверитьРавенство(СоставнойТип.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	Маска = СоставнойТип.МаскаАтрибутов; 
	ЮнитТест.ПроверитьЗаполненность(Маска);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Маска), Тип("МаскаXS"));
	ЮнитТест.ПроверитьРавенство(Маска.ЛексическоеЗначениеОграниченияПространствИмен, "##targetNamespace");

КонецПроцедуры	

Процедура ТестМаскаXS() Экспорт

	Схема = ПримерМаскаXS();
	Schema = ExampleXSWildcard();

	ПроверитьМаскаXS(Схема);
	ПроверитьМаскаXS(Schema)

КонецПроцедуры

#КонецОбласти

#Область ОпределениеГруппыАтрибутовXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemaattributegroup
//
// Результат:
//	см. РезультатОпределениеГруппыАтрибутовXS 

Функция ПримерОпределениеГруппыАтрибутовXS()

	Схема = Новый СхемаXML;

	// <xs:attributeGroup name="myAttributeGroup">
	ГруппаАтрибутов = Новый ОпределениеГруппыАтрибутовXS;
	ГруппаАтрибутов.Имя = "myAttributeGroup";
	
	// <xs:attribute name="someattribute1" type="xs:integer"/>
	АтрибутЧисло = Новый ОбъявлениеАтрибутаXS;
	АтрибутЧисло.Имя = "someattribute1";
	АтрибутЧисло.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "integer");
	ГруппаАтрибутов.Содержимое.Добавить(АтрибутЧисло);
	
	// <xs:attribute name="someattribute2" type="xs:string"/>
	АтрибутСтрока = Новый ОбъявлениеАтрибутаXS;
	АтрибутСтрока.Имя = "someattribute2";
	АтрибутСтрока.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	ГруппаАтрибутов.Содержимое.Добавить(АтрибутСтрока);
	
	Схема.Содержимое.Добавить(ГруппаАтрибутов);
	
	// <xs:attributeGroup name="myAttributeGroupB">
	ГруппаАтрибутов = Новый ОпределениеГруппыАтрибутовXS;
	ГруппаАтрибутов.Имя = "myAttributeGroupB";
	
	// <xs:attribute name="someattribute20" type="xs:date"/>
	АтрибутДата = Новый ОбъявлениеАтрибутаXS;
	АтрибутДата.Имя = "someattribute20";
	АтрибутДата.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "date");
	ГруппаАтрибутов.Содержимое.Добавить(АтрибутДата);
	
	// <xs:attributeGroup ref="myAttributeGroupA"/>
	АтрибутСсылка = Новый ОпределениеГруппыАтрибутовXS;
	АтрибутСсылка.Ссылка = Новый РасширенноеИмяXML("", "myAttributeGroup");
	ГруппаАтрибутов.Содержимое.Добавить(АтрибутСсылка);
	
	// <xs:anyAttribute namespace="##targetNamespace"/>	
	ЛюбойАтрибут = Новый МаскаXS;
	ЛюбойАтрибут.ЛексическоеЗначениеОграниченияПространствИмен	= "##targetNamespace";
	ГруппаАтрибутов.Маска = ЛюбойАтрибут;
	
	Схема.Содержимое.Добавить(ГруппаАтрибутов);
	
	// <xs:complexType name="myElementType">
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	СоставнойТип.Имя = "myElementType";
	
	// <xs:attributeGroup ref="myAttributeGroup"/>
	АтрибутСсылка = Новый ОпределениеГруппыАтрибутовXS;
	АтрибутСсылка.Ссылка = Новый РасширенноеИмяXML("", "myAttributeGroup");
	СоставнойТип.Атрибуты.Добавить(АтрибутСсылка);
	
	Схема.Содержимое.Добавить(СоставнойТип);
	
	Возврат Схема;

КонецФункции

Function ExampleXSAttributeGroupDefinition()

	schema = New XMLSchema;

	// <xs:attributeGroup name="myAttributeGroup">
	myAttributeGroup = New XSAttributeGroupDefinition;
	myAttributeGroup.Name = "myAttributeGroup";
	
	// <xs:attribute name="someattribute1" type="xs:integer"/>
	someattribute1 = New XSAttributeDeclaration;
	someattribute1.Name = "someattribute1";
	someattribute1.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "integer");
	myAttributeGroup.Content.Add(someattribute1);
	
	// <xs:attribute name="someattribute2" type="xs:string"/>
	someattribute2 = New XSAttributeDeclaration;
	someattribute2.Name = "someattribute2";
	someattribute2.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	myAttributeGroup.Content.Add(someattribute2);
	
	schema.Content.Add(myAttributeGroup);
	
	// <xs:attributeGroup name="myAttributeGroupB">
	myAttributeGroupB = New XSAttributeGroupDefinition;
	myAttributeGroupB.Name = "myAttributeGroupB";
	
	// <xs:attribute name="someattribute20" type="xs:date"/>
	someattribute20 = New XSAttributeDeclaration;
	someattribute20.Name = "someattribute20";
	someattribute20.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "date");
	myAttributeGroupB.Content.Add(someattribute20);
	
	// <xs:attributeGroup ref="myAttributeGroupA"/>
	myAttributeGroupRefA = New XSAttributeGroupDefinition;
	myAttributeGroupRefA.Reference = New XMLExpandedName("", "myAttributeGroup");
	myAttributeGroupB.Content.Add(myAttributeGroupRefA);
	
	// <xs:anyAttribute namespace="##targetNamespace"/>	
	Wildcard = New XSWildcard;
	Wildcard.LexicalNamespaceConstraint	= "##targetNamespace";
	myAttributeGroupB.Wildcard = Wildcard;
	
	schema.Content.Add(myAttributeGroupB);
	
	// <xs:complexType name="myElementType">
	myElementType = New XSComplexTypeDefinition;
	myElementType.Name = "myElementType";
	
	// <xs:attributeGroup ref="myAttributeGroup"/>
	myAttributeGroupRef = New XSAttributeGroupDefinition;
	myAttributeGroupRef.Reference = New XMLExpandedName("", "myAttributeGroup");
	myElementType.Attributes.Add(myAttributeGroupRef);
	
	schema.Content.Add(myElementType);
	
	return schema;
	
EndFunction

Процедура РезультатОпределениеГруппыАтрибутовXS()
	//<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	//	<xs:attributeGroup name="myAttributeGroup">
	//		<xs:attribute name="someattribute1" type="xs:integer"/>
	//		<xs:attribute name="someattribute2" type="xs:string"/>
	//	</xs:attributeGroup>
	//	<xs:attributeGroup name="myAttributeGroupB">
	//		<xs:attribute name="someattribute20" type="xs:date"/>
	//		<xs:attributeGroup ref="myAttributeGroup"/>
	//		<xs:any namespace="##targetNamespace"/>
	//	</xs:attributeGroup>
	//	<xs:complexType name="myElementType">
	//		<xs:attributeGroup ref="myAttributeGroup"/>
	//	</xs:complexType>
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьОпределениеГруппыАтрибутовXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 3);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияГруппАтрибутов.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);

	ГруппаАтрибутов = Схема.ОпределенияГруппАтрибутов.Получить("myAttributeGroup");
	ЮнитТест.ПроверитьЗаполненность(ГруппаАтрибутов);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ГруппаАтрибутов), Тип("ОпределениеГруппыАтрибутовXS"));
	ЮнитТест.ПроверитьРавенство(ГруппаАтрибутов.Имя, "myAttributeGroup");
	ЮнитТест.ПроверитьРавенство(ГруппаАтрибутов.Компоненты.Количество(), 2);

	АтрибутЧисло = ГруппаАтрибутов.Компоненты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутЧисло);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутЧисло), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутЧисло.Имя, "someattribute1");
	ЮнитТест.ПроверитьРавенство(АтрибутЧисло.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "integer"));
	
	АтрибутСтрока = ГруппаАтрибутов.Компоненты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(АтрибутСтрока);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутСтрока), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутСтрока.Имя, "someattribute2");
	ЮнитТест.ПроверитьРавенство(АтрибутСтрока.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	ГруппаАтрибутов = Схема.ОпределенияГруппАтрибутов.Получить("myAttributeGroupB");
	ЮнитТест.ПроверитьЗаполненность(ГруппаАтрибутов);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ГруппаАтрибутов), Тип("ОпределениеГруппыАтрибутовXS"));
	ЮнитТест.ПроверитьРавенство(ГруппаАтрибутов.Имя, "myAttributeGroupB");
	ЮнитТест.ПроверитьРавенство(ГруппаАтрибутов.Компоненты.Количество(), 2);

	АтрибутДата = ГруппаАтрибутов.Компоненты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутДата);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутДата), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутДата.Имя, "someattribute20");
	ЮнитТест.ПроверитьРавенство(АтрибутДата.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "date"));

	АтрибутСсылка = ГруппаАтрибутов.Компоненты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(АтрибутСсылка);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутСсылка), Тип("ОпределениеГруппыАтрибутовXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутСсылка.Ссылка, Новый РасширенноеИмяXML("", "myAttributeGroup"));

	ЛюбойАтрибут = ГруппаАтрибутов.Маска;
	ЮнитТест.ПроверитьЗаполненность(ЛюбойАтрибут);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЛюбойАтрибут), Тип("МаскаXS"));
	ЮнитТест.ПроверитьРавенство(ЛюбойАтрибут.ЛексическоеЗначениеОграниченияПространствИмен, "##targetNamespace");
	
	СоставнойТип = Схема.ОпределенияТипов.Получить("myElementType");
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Имя, "myElementType");
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутСсылка = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутСсылка);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутСсылка), Тип("ОпределениеГруппыАтрибутовXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутСсылка.Ссылка, Новый РасширенноеИмяXML("", "myAttributeGroup"));

КонецПроцедуры

Процедура ТестОпределениеГруппыАтрибутовXS() Экспорт
	
	Схема = ПримерОпределениеГруппыАтрибутовXS();
	Schema = ExampleXSAttributeGroupDefinition();

	ПроверитьОпределениеГруппыАтрибутовXS(Схема);
	ПроверитьОпределениеГруппыАтрибутовXS(Schema)
	
КонецПроцедуры

#КонецОбласти

#Область ОбъявлениеНотацииXS 

// Источник:
//	https://www.w3schools.com/xml/el_notation.asp
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemanotation
//
// Результат:
//	см. РезультатОбъявлениеНотацииXS 

Функция ПримерОбъявлениеНотацииXS()

	Схема = Новый СхемаXML;

	// <xs:notation name="gif" public="image/gif" system="view.exe"/>
	НотацияGIF = Новый ОбъявлениеНотацииXS;
	НотацияGIF.Имя = "gif";
	НотацияGIF.ПубличныйИдентификатор = "image/gif";
	НотацияGIF.СистемныйИдентификатор = "view.exe";

	Схема.Содержимое.Добавить(НотацияGIF);

	// <xs:notation name="jpeg" public="image/jpeg" system="view.exe"/>
	НотацияJPEG = Новый ОбъявлениеНотацииXS;
	НотацияJPEG.Имя = "jpeg";
	НотацияJPEG.ПубличныйИдентификатор = "image/jpeg";
	НотацияJPEG.СистемныйИдентификатор = "view.exe";

	Схема.Содержимое.Добавить(НотацияJPEG);

	// <xs:element name="image">
	Элемент = Новый ОбъявлениеЭлементаXS; 
	Элемент.Имя = "image";

	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;

	// <xs:attribute name="type">
	АтрибутТип = Новый ОбъявлениеАтрибутаXS;
	АтрибутТип.Имя = "type";
	СоставнойТип.Атрибуты.Добавить(АтрибутТип);

	// <xs:simpleType>
	ПростойТип = Новый ОпределениеПростогоТипаXS;

	// <xs:restriction base="xs:NOTATION">
	ПростойТип.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "NOTATION"); 

	// <xs:enumeration value="gif"/>
	ПеречислениеGIF = Новый ФасетПеречисленияXS;
	ПеречислениеGIF.Значение = "gif";
	ПростойТип.Фасеты.Добавить(ПеречислениеGIF);

	// <xs:enumeration value="jpeg"/>
	ПеречислениеJPEG = Новый ФасетПеречисленияXS;
	ПеречислениеJPEG.Значение = "jpeg";
	ПростойТип.Фасеты.Добавить(ПеречислениеJPEG);

	АтрибутТип.АнонимноеОпределениеТипа = ПростойТип;
	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSNotationDeclaration()

	schema = New XMLSchema;

	// <xs:notation name="gif" public="image/gif" system="view.exe"/>
	NotationGIF = New XSNotationDeclaration;
	NotationGIF.Name = "gif";
	NotationGIF.PublicId = "image/gif";
	NotationGIF.SystemId = "view.exe";

	schema.Content.Add(NotationGIF);

	// <xs:notation name="jpeg" public="image/jpeg" system="view.exe"/>
	NotationJPEG = New XSNotationDeclaration;
	NotationJPEG.Name = "jpeg";
	NotationJPEG.PublicId = "image/jpeg";
	NotationJPEG.SystemId = "view.exe";

	schema.Content.Add(NotationJPEG);

	// <xs:element name="image">
	Element = New XSElementDeclaration; 
	Element.Name = "image";

	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;

	// <xs:attribute name="type">
	attributeType = New XSAttributeDeclaration;
	attributeType.Name = "type";
	complexType.Attributes.Add(attributeType);

	// <xs:simpleType>
	SimpleType = New XSSimpleTypeDefinition;

	// <xs:restriction base="xs:NOTATION">
	SimpleType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "NOTATION"); 

	// <xs:enumeration value="gif"/>
	EnumerationGIF = New XSEnumerationFacet;
	EnumerationGIF.Value = "gif";
	SimpleType.Facets.Add(EnumerationGIF);

	// <xs:enumeration value="jpeg"/>
	EnumerationJPEG = New XSEnumerationFacet;
	EnumerationJPEG.Value = "jpeg";
	SimpleType.Facets.Add(EnumerationJPEG);

	attributeType.AnonymousTypeDefinition = SimpleType;
	Element.AnonymousTypeDefinition = complexType;
	schema.Content.Add(Element);
	
	return schema;
	
EndFunction

Процедура РезультатОбъявлениеНотацииXS()
	// <xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">
	// 	<xs:notation name="gif" public="image/gif" system="view.exe"/>
	// 	<xs:notation name="jpeg" public="image/jpeg" system="view.exe"/>
	// 	<xs:element name="image">
	// 		<xs:complexType>
	// 			<xs:attribute name="type">
	// 				<xs:simpleType>
	// 					<xs:restriction base="xs:NOTATION">
	// 						<xs:enumeration value="gif"/>
	// 						<xs:enumeration value="jpeg"/>
	// 					</xs:restriction>
	// 				</xs:simpleType>
	// 			</xs:attribute>
	// 		</xs:complexType>
	// 	</xs:element>
	// </xs:schema>
КонецПроцедуры

Процедура ПроверитьОбъявлениеНотацииXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 3);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияНотаций.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	НотацияGIF = Схема.ОбъявленияНотаций.Получить("gif");
	ЮнитТест.ПроверитьЗаполненность(НотацияGIF);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(НотацияGIF), Тип("ОбъявлениеНотацииXS"));
	ЮнитТест.ПроверитьРавенство(НотацияGIF.Имя, "gif");
	ЮнитТест.ПроверитьРавенство(НотацияGIF.ПубличныйИдентификатор, "image/gif");
	ЮнитТест.ПроверитьРавенство(НотацияGIF.СистемныйИдентификатор, "view.exe");

	НотацияJPEG = Схема.ОбъявленияНотаций.Получить("jpeg");
	ЮнитТест.ПроверитьЗаполненность(НотацияJPEG);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(НотацияJPEG), Тип("ОбъявлениеНотацииXS"));
	ЮнитТест.ПроверитьРавенство(НотацияJPEG.Имя, "jpeg");
	ЮнитТест.ПроверитьРавенство(НотацияJPEG.ПубличныйИдентификатор, "image/jpeg");
	ЮнитТест.ПроверитьРавенство(НотацияJPEG.СистемныйИдентификатор, "view.exe");

	Элемент = Схема.ОбъявленияЭлементов.Получить("image");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "image");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутТип = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутТип), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутТип.Имя, "type");

	ПростойТип = АтрибутТип.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(ПростойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПростойТип), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ПростойТип.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "NOTATION"));
	ЮнитТест.ПроверитьРавенство(ПростойТип.Фасеты.Количество(), 2);
	
	ПеречислениеGIF = ПростойТип.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ПеречислениеGIF);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПеречислениеGIF), Тип("ФасетПеречисленияXS"));
	ЮнитТест.ПроверитьРавенство(ПеречислениеGIF.Значение, "gif");

	ПеречислениеJPEG = ПростойТип.Фасеты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(ПеречислениеJPEG);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПеречислениеJPEG), Тип("ФасетПеречисленияXS"));
	ЮнитТест.ПроверитьРавенство(ПеречислениеJPEG.Значение, "jpeg");

КонецПроцедуры

Процедура ТестОбъявлениеНотацииXS() Экспорт
	
	Схема = ПримерОбъявлениеНотацииXS();
	Schema = ExampleXSNotationDeclaration();

	ПроверитьОбъявлениеНотацииXS(Схема);
	ПроверитьОбъявлениеНотацииXS(Schema);
	
КонецПроцедуры

#КонецОбласти

#Область ОпределениеГруппыМоделиXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemagroup
//
// Результат:
//	см. РезультатОпределениеГруппыМоделиXS 

Функция ПримерОпределениеГруппыМоделиXS()

	Схема = Новый СхемаXML;

	// <xs:element name="thing1" type="xs:string"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "thing1";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	Схема.Содержимое.Добавить(Элемент);
	
	// <xs:element name="thing2" type="xs:string"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "thing2";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	Схема.Содержимое.Добавить(Элемент);
	
	// <xs:element name="thing3" type="xs:string"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "thing3";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");
	Схема.Содержимое.Добавить(Элемент);
	
	// <xs:attribute name="myAttribute" type="xs:decimal"/>
	Атрибут = Новый ОбъявлениеАтрибутаXS;
	Атрибут.Имя = "myAttribute";
	Атрибут.ИмяТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "decimal");
	Схема.Содержимое.Добавить(Атрибут);
	
	// <xs:group name="myGroupOfThings">
	Группа = Новый ОпределениеГруппыМоделиXS;
	Группа.Имя = "myGroupOfThings";
	Схема.Содержимое.Добавить(Группа);
	
	// <xs:sequence>
	Последовательность = Новый ГруппаМоделиXS;
	Последовательность.ВидГруппы =  ВидГруппыМоделиXS.Последовательность;
	Фрагмент = Новый ФрагментXS;
	Фрагмент.Часть = Последовательность;
	Группа.ГруппаМодели = Фрагмент;
	
	// <xs:element ref="thing1"/>
	ЭлементСсылка = Новый ОбъявлениеЭлементаXS;
	ЭлементСсылка.Ссылка = Новый РасширенноеИмяXML("", "thing1");
	Последовательность.Фрагменты.Добавить(ЭлементСсылка);
	
	// <xs:element ref="thing2"/>
	ЭлементСсылка = Новый ОбъявлениеЭлементаXS;
	ЭлементСсылка.Ссылка = Новый РасширенноеИмяXML("", "thing2");
	Последовательность.Фрагменты.Добавить(ЭлементСсылка);
	
	// <xs:element ref="thing3"/>
	ЭлементСсылка = Новый ОбъявлениеЭлементаXS;
	ЭлементСсылка.Ссылка = Новый РасширенноеИмяXML("", "thing3");
	Последовательность.Фрагменты.Добавить(ЭлементСсылка);
	
	// <xs:complexType name="myComplexType">
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	СоставнойТип.Имя = "myComplexType";
	Схема.Содержимое.Добавить(СоставнойТип);
	
	// <xs:group ref="myGroupOfThings"/>
	ГруппаСсылка = Новый ОпределениеГруппыМоделиXS;
	ГруппаСсылка.Ссылка = Новый РасширенноеИмяXML("", "myGroupOfThings");
	СоставнойТип.Содержимое = ГруппаСсылка;
	
	// <xs:attribute ref="myAttribute"/>
	АтрибутСсылка = Новый ОбъявлениеАтрибутаXS;
	АтрибутСсылка.Ссылка = Новый РасширенноеИмяXML("", "myAttribute");
	СоставнойТип.Атрибуты.Добавить(АтрибутСсылка);
	
	Возврат Схема;

КонецФункции

Function ExampleXSModelGroupDefinition()

	schema = New XMLSchema;

	// <xs:element name="thing1" type="xs:string"/>
	elementThing1 = New XSElementDeclaration;
	elementThing1.Name = "thing1";
	elementThing1.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	schema.Content.Add(elementThing1);
	
	// <xs:element name="thing2" type="xs:string"/>
	elementThing2 = New XSElementDeclaration;
	elementThing2.Name = "thing2";
	elementThing2.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	schema.Content.Add(elementThing2);
	
	// <xs:element name="thing3" type="xs:string"/>
	elementThing3 = New XSElementDeclaration;
	elementThing3.Name = "thing3";
	elementThing3.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");
	schema.Content.Add(elementThing3);
	
	// <xs:attribute name="myAttribute" type="xs:decimal"/>
	myAttribute = New XSAttributeDeclaration;
	myAttribute.Name = "myAttribute";
	myAttribute.TypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "decimal");
	schema.Content.Add(myAttribute);
	
	// <xs:group name="myGroupOfThings">
	myGroupOfThings = New XSModelGroupDefinition;
	myGroupOfThings.Name = "myGroupOfThings";
	schema.Content.Add(myGroupOfThings);
	
	// <xs:sequence>
	sequence = New XSModelGroup;
	sequence.Compositor = XSCompositor.Sequence;
	particle = New XSParticle;
	particle.Term = sequence;
	myGroupOfThings.ModelGroup = particle;
	
	// <xs:element ref="thing1"/>
	elementThing1Ref = New XSElementDeclaration;
	elementThing1Ref.Reference = New XMLExpandedName("", "thing1");
	sequence.Particles.Add(elementThing1Ref);
	
	// <xs:element ref="thing2"/>
	elementThing2Ref = New XSElementDeclaration;
	elementThing2Ref.Reference = New XMLExpandedName("", "thing2");
	sequence.Particles.Add(elementThing2Ref);
	
	// <xs:element ref="thing3"/>
	elementThing3Ref = New XSElementDeclaration;
	elementThing3Ref.Reference = New XMLExpandedName("", "thing3");
	sequence.Particles.Add(elementThing3Ref);
	
	// <xs:complexType name="myComplexType">
	myComplexType = New XSComplexTypeDefinition;
	myComplexType.Name = "myComplexType";
	schema.Content.Add(myComplexType);
	
	// <xs:group ref="myGroupOfThings"/>
	myGroupOfThingsRef = New XSModelGroupDefinition;
	myGroupOfThingsRef.Reference = New XMLExpandedName("", "myGroupOfThings");
	myComplexType.Content = myGroupOfThingsRef;
	
	// <xs:attribute ref="myAttribute"/>
	myAttributeRef = New XSAttributeDeclaration;
	myAttributeRef.Reference = New XMLExpandedName("", "myAttribute");
	myComplexType.Attributes.Add(myAttributeRef);
	
	return schema;
	
EndFunction

Процедура РезультатОпределениеГруппыМоделиXS()
	//<xs:schema  xmlns:xs="http://www.w3.org/2001/XMLSchema">
	//    <xs:element name="thing1" type="xs:string"/>
	//    <xs:element name="thing2" type="xs:string"/>
	//    <xs:element name="thing3" type="xs:string"/>
	//    <xs:attribute name="myAttribute" type="xs:decimal"/>
	//    <xs:group name="myGroupOfThings">
	//     <xs:sequence>
	//      <xs:element ref="thing1"/>
	//      <xs:element ref="thing2"/>
	//      <xs:element ref="thing3"/>
	//     </xs:sequence>
	//    </xs:group>
	//    <xs:complexType name="myComplexType">
	//        <xs:group ref="myGroupOfThings"/>
	//        <xs:attribute ref="myAttribute"/>
	//    </xs:complexType>
	//</xs:schema>
КонецПроцедуры

Процедура ПроверитьОпределениеГруппыМоделиXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 6);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 3);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияАтрибутов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияГруппМоделей.Количество(), 1);

	Элемент = Схема.ОбъявленияЭлементов.Получить("thing1");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "thing1");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	Элемент = Схема.ОбъявленияЭлементов.Получить("thing2");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "thing2");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	Элемент = Схема.ОбъявленияЭлементов.Получить("thing3");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "thing3");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));

	Атрибут = Схема.ОбъявленияАтрибутов.Получить("myAttribute");
	ЮнитТест.ПроверитьЗаполненность(Атрибут);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Атрибут), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(Атрибут.Имя, "myAttribute");
	ЮнитТест.ПроверитьРавенство(Атрибут.ИмяТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "decimal"));

	Группа = Схема.ОпределенияГруппМоделей.Получить("myGroupOfThings");
	ЮнитТест.ПроверитьЗаполненность(Группа);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Группа), Тип("ОпределениеГруппыМоделиXS"));
	ЮнитТест.ПроверитьРавенство(Группа.Имя, "myGroupOfThings");

	Фрагмент = Группа.ГруппаМодели;
	ЮнитТест.ПроверитьЗаполненность(Фрагмент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Фрагмент), Тип("ФрагментXS"));

	Последовательность = Фрагмент.Часть;
	ЮнитТест.ПроверитьЗаполненность(Последовательность);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Последовательность), Тип("ГруппаМоделиXS"));
	ЮнитТест.ПроверитьРавенство(Последовательность.ВидГруппы, ВидГруппыМоделиXS.Последовательность);
	ЮнитТест.ПроверитьРавенство(Последовательность.Фрагменты.Количество(), 3);

	ЭлементСсылка = Последовательность.Фрагменты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ЭлементСсылка);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЭлементСсылка), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(ЭлементСсылка.Ссылка, Новый РасширенноеИмяXML("", "thing1"));
	ЮнитТест.ПроверитьИстину(ЭлементСсылка.ЭтоСсылка);

	ЭлементСсылка = Последовательность.Фрагменты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(ЭлементСсылка);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЭлементСсылка), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(ЭлементСсылка.Ссылка, Новый РасширенноеИмяXML("", "thing2"));
	ЮнитТест.ПроверитьИстину(ЭлементСсылка.ЭтоСсылка);

	ЭлементСсылка = Последовательность.Фрагменты.Получить(2);
	ЮнитТест.ПроверитьЗаполненность(ЭлементСсылка);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ЭлементСсылка), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(ЭлементСсылка.Ссылка, Новый РасширенноеИмяXML("", "thing3"));
	ЮнитТест.ПроверитьИстину(ЭлементСсылка.ЭтоСсылка);

	СоставнойТип = Схема.ОпределенияТипов.Получить("myComplexType");
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Имя, "myComplexType");
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	ГруппаСсылка = СоставнойТип.Содержимое;
	ЮнитТест.ПроверитьЗаполненность(ГруппаСсылка);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ГруппаСсылка), Тип("ОпределениеГруппыМоделиXS"));
	ЮнитТест.ПроверитьРавенство(ГруппаСсылка.Ссылка, Новый РасширенноеИмяXML("", "myGroupOfThings"));
	ЮнитТест.ПроверитьИстину(ГруппаСсылка.ЭтоСсылка);

	АтрибутСсылка = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутСсылка);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутСсылка), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутСсылка.Ссылка, Новый РасширенноеИмяXML("", "myAttribute"));
	ЮнитТест.ПроверитьИстину(АтрибутСсылка.ЭтоСсылка);

КонецПроцедуры

Процедура ТестОпределениеГруппыМоделиXS() Экспорт
	
	Схема = ПримерОпределениеГруппыМоделиXS();
	Schema = ExampleXSModelGroupDefinition();

	ПроверитьОпределениеГруппыМоделиXS(Схема);
	ПроверитьОпределениеГруппыМоделиXS(Schema);

КонецПроцедуры

#КонецОбласти

#КонецОбласти

ЗаписьXML = Новый ЗаписьXML;
ЧтениеXML = Новый ЧтениеXML;
СериализаторXDTO = Новый СериализаторXDTO;

Если СтартовыйСценарий().Источник = ТекущийСценарий().Источник Тогда
	
	СхемаXML = ПримерФормированияСхемыXML();
	
	ТекстXML = ТекстСхемыXML(СхемаXML);

	Сообщить(ТекущаяДата());
	Сообщить("НачалоПримера");
	Сообщить(ТекстXML);
	Сообщить("КонецПримера");
	
КонецЕсли;