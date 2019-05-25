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
	СписокТестов.Добавить("ТестПереопределениеXS");
	СписокТестов.Добавить("ТестДокументацияXS");
	СписокТестов.Добавить("ТестИнформацияДляПриложенияXS");
	СписокТестов.Добавить("ТестОпределениеПростогоТипаXS");
	СписокТестов.Добавить("ТестОпределениеПростогоТипаXS_Объединение");
	СписокТестов.Добавить("ТестФасетДлиныXS");
	СписокТестов.Добавить("ТестФасетМинимальнойДлиныXS");
	СписокТестов.Добавить("ТестФасетМаксимальнойДлиныXS");
	СписокТестов.Добавить("ТестФасетКоличестваРазрядовДробнойЧастиXS");
	СписокТестов.Добавить("ТестФасетМинимальногоИсключающегоЗначенияXS");
	СписокТестов.Добавить("ТестФасетОбразцаXS");
	СписокТестов.Добавить("ТестМаскаXS");
	СписокТестов.Добавить("ТестОпределениеГруппыАтрибутовXS");
	СписокТестов.Добавить("ТестОбъявлениеНотацииXS");
	СписокТестов.Добавить("ТестОпределениеГруппыМоделиXS");
	
	Возврат СписокТестов;

КонецФункции

#КонецОбласти

#Область СлужебныеПроцедурыИФункции

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

#КонецОбласти

#Область ВыборПримера

Функция ПримерФормированияСхемыXML()

	СхемаXML = Новый СхемаXML;
	
	//////////////////////////

	//СхемаXML = ПримерСхемаXML();
	//СхемаXML = ExampleXMLSchema();

	//СхемаXML = ПримерВключениеXS();
	//СхемаXML = ExampleXSInclude();

	//СхемаXML = ПримерПереопределениеXS();
	//СхемаXML = ExampleXSRedefine();

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

	//СхемаXML = ПримерФасетОбразцаXS();
	//СхемаXML = ExampleXSPatternFacet();

	СхемаXML = ПримерФасетПробельныхСимволовXS();
	//СхемаXML = ExampleXSWhitespaceFacet();

	//СхемаXML = ПримерМаскаXS();
	//СхемаXML = ExampleXSWildcard();

	//СхемаXML = ПримерОпределениеГруппыАтрибутовXS();
	//СхемаXML = ExampleXSAttributeGroupDefinition();
	
	//СхемаXML = ПримерОбъявлениеНотацииXS();
	//СхемаXML = ExampleXSNotationDeclaration();

	//СхемаXML = ПримерОпределениеГруппыМоделиXS();
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
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:element name='cat' type='xs:string'/>
	|	<xs:element name='dog' type='xs:string'/>
	|	<xs:element name='redDog' type='xs:string' substitutionGroup='dog'/>
	|	<xs:element name='brownDog' type='xs:string' substitutionGroup='dog' />
	|	<xs:element name='pets'>
	|		<xs:complexType>
	|			<xs:choice minOccurs='0' maxOccurs='unbounded'>
	|				<xs:element ref='cat'/>
	|				<xs:element ref='dog'/>
	|			</xs:choice>
	|		</xs:complexType>
	|	</xs:element>
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
	ПроверитьСхемаXML(Схема);

	Schema = ExampleXMLSchema();
	ПроверитьСхемаXML(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатСхемаXML());
	ПроверитьСхемаXML(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьСхемаXML(СхемаСериализатор);

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
	"<schema elementFormDefault='qualified' targetNamespace='http://www.w3.org/2001/05/XMLInfoset' xmlns='http://www.w3.org/2001/XMLSchema'>
	| 	<import namespace='http://www.example.com/IPO' />
	| 	<include schemaLocation='example.xsd' />
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
	ПроверитьВключениеXS(Схема);

	Schema = ExampleXSInclude();
	ПроверитьВключениеXS(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатВключениеXS());
	ПроверитьВключениеXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьВключениеXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ПереопределениеXS

// Источник:
// 	https://www.w3schools.com/xml/el_redefine.asp
//
// Результат:
//	см. РезультатПереопределениеXS

Функция ПримерПереопределениеXS()

	Схема = Новый СхемаXML;

	// <xs:redefine schemaLocation="Myschema1.xsd">
	Переопределение = Новый ПереопределениеXS;
	Переопределение.РасположениеСхемы = "Myschema1.xsd";
	Схема.Директивы.Добавить(Переопределение);

	// <xs:complexType name="pname">
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;
	СоставнойТип.Имя = "pname";
	Переопределение.Содержимое.Добавить(СоставнойТип);

	// <xs:complexContent>
	СоставнойТип.МодельСодержимого = МодельСодержимогоXS.Составная;

	// <xs:extension base="pname">
	СоставнойТип.МетодНаследования = МетодНаследованияXS.Расширение;
	СоставнойТип.ИмяБазовогоТипа = Новый РасширенноеИмяXML("", "pname");

	// <xs:sequence>
	Последовательность = Новый ГруппаМоделиXS;
	Последовательность.ВидГруппы = ВидГруппыМоделиXS.Последовательность;
	СоставнойТип.Содержимое = Последовательность;

	// <xs:element name="country"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "country";
	Последовательность.Фрагменты.Добавить(Элемент);

	// <xs:element name="author" type="pname"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "author";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("", "pname");
	Схема.Содержимое.Добавить(Элемент);

	Возврат Схема;

КонецФункции

Function ExampleXSRedefine()
	
	Schema = New XMLSchema;

	// <xs:redefine schemaLocation="Myschema1.xsd">
	Redefine = New XSRedefine;
	Redefine.SchemaLocation = "Myschema1.xsd";
	Schema.Directives.Add(Redefine);

	// <xs:complexType name="pname">
	ComplexType = New XSComplexTypeDefinition;
	ComplexType.Name = "pname";
	Redefine.Content.Add(ComplexType);

	// <xs:complexContent>
	ComplexType.ContentModel = XSContentModel.Complex;

	// <xs:extension base="pname">
	ComplexType.DerivationMethod = XSDerivationMethod.Extension;
	ComplexType.BaseTypeName = New XMLExpandedName("", "pname");

	// <xs:sequence>
	Sequence = New XSModelGroup;
	Sequence.ВидГруппы = XSCompositor.Sequence;
	ComplexType.Content = Sequence;

	// <xs:element name="country"/>
	Element = New XSElementDeclaration;
	Element.Name = "country";
	Sequence.Particles.Add(Element);

	// <xs:element name="author" type="pname"/>
	Element = New XSElementDeclaration;
	Element.Name = "author";
	Element.TypeName = New XMLExpandedName("", "pname");
	Schema.Content.Add(Element);

	Return Schema;

EndFunction

Функция РезультатПереопределениеXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:redefine schemaLocation='Myschema1.xsd'>
	|		<xs:complexType name='pname'>
	|			<xs:complexContent mixed='false'>
	|				<xs:extension base='pname'>
	|					<xs:sequence>
	|						<xs:element name='country' />
	|					</xs:sequence>
	|				</xs:extension>
	|			</xs:complexContent>
	|		</xs:complexType>
	|	</xs:redefine>
	|	<xs:element name='author' type='pname' />
	|</xs:schema>";

КонецФункции

Процедура ПроверитьПереопределениеXS(схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Директивы.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	Переопределение = Схема.Директивы.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(Переопределение);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Переопределение), Тип("ПереопределениеXS"));
	ЮнитТест.ПроверитьРавенство(Переопределение.РасположениеСхемы, "Myschema1.xsd");
	ЮнитТест.ПроверитьРавенство(Переопределение.Содержимое.Количество(), 1);

	СоставнойТип = Переопределение.Содержимое.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Имя, "pname");
	ЮнитТест.ПроверитьРавенство(СоставнойТип.МодельСодержимого, МодельСодержимогоXS.Составная);
	ЮнитТест.ПроверитьРавенство(СоставнойТип.МетодНаследования, МетодНаследованияXS.Расширение);
	ЮнитТест.ПроверитьРавенство(СоставнойТип.ИмяБазовогоТипа, Новый РасширенноеИмяXML("", "pname"));

	Последовательность = СоставнойТип.Содержимое;
	ЮнитТест.ПроверитьЗаполненность(Последовательность);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Последовательность), Тип("ГруппаМоделиXS"));
	ЮнитТест.ПроверитьРавенство(Последовательность.ВидГруппы, ВидГруппыМоделиXS.Последовательность);
	ЮнитТест.ПроверитьРавенство(Последовательность.Фрагменты.Количество(), 1);

	Элемент = Последовательность.Фрагменты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "country");
	
	Элемент = Схема.ОбъявленияЭлементов.Получить("author");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "author");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("", "pname"));

КонецПроцедуры

Процедура ТестПереопределениеXS() Экспорт

	Схема = ПримерПереопределениеXS();
	ПроверитьПереопределениеXS(Схема);

	Schema = ExampleXSRedefine();
	ПроверитьПереопределениеXS(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатПереопределениеXS());
	ПроверитьПереопределениеXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьПереопределениеXS(СхемаТекст);

КонецПроцедуры

#КонецОбласти

#Область ДокументацияXS

// Источник:
// 	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemadocumentation
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

Функция РезультатДокументацияXS()
	
	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:simpleType name='northwestStates'>
	|		<xs:annotation>
	|			<xs:documentation>States in the Pacific Northwest of US</xs:documentation>
	|		</xs:annotation>
	|		<xs:restriction base='xs:string'>
	|			<xs:enumeration value='WA'>
	|				<xs:annotation>
	|					<xs:documentation>Washington</xs:documentation>
	|				</xs:annotation>
	|			</xs:enumeration>
	|			<xs:enumeration value='OR'>
	|				<xs:annotation>
	|					<xs:documentation>Oregon</xs:documentation>
	|				</xs:annotation>
	|			</xs:enumeration>
	|			<xs:enumeration value='ID'>
	|				<xs:annotation>
	|					<xs:documentation>Idaho</xs:documentation>
	|				</xs:annotation>
	|			</xs:enumeration>
	|		</xs:restriction>
	|	</xs:simpleType>
	|</xs:schema>";
	
КонецФункции

Процедура ПроверитьДокументацияXS(Схема)
	
	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);

	ПростойТип = Схема.ОпределенияТипов.Получить("northwestStates");
	ЮнитТест.ПроверитьЗаполненность(ПростойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПростойТип), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ПростойТип.Имя, "northwestStates");
	ЮнитТест.ПроверитьРавенство(ПростойТип.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));
	ЮнитТест.ПроверитьРавенство(ПростойТип.Фасеты.Количество(), 3);

	АннотацияNorthwestStates = ПростойТип.Аннотация;
	ЮнитТест.ПроверитьЗаполненность(АннотацияNorthwestStates);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АннотацияNorthwestStates), Тип("АннотацияXS"));
	ЮнитТест.ПроверитьРавенство(АннотацияNorthwestStates.Состав.Количество(), 1);

	ДокументацияNorthwestStates = АннотацияNorthwestStates.Состав.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ДокументацияNorthwestStates);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ДокументацияNorthwestStates), Тип("ДокументацияXS"));
	// ЮнитТест.ПроверитьРавенство(ДокументацияNorthwestStates.Источник, "States in the Pacific Northwest of US");
	// //ЮнитТест.ПроверитьРавенство(ДокументацияNorthwestStates.Markup, "States in the Pacific Northwest of US");

	ПеречислениеWA = ПростойТип.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ПеречислениеWA);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПеречислениеWA), Тип("ФасетПеречисленияXS"));
	ЮнитТест.ПроверитьРавенство(ПеречислениеWA.Значение, "WA");

	АннотацияWA = ПеречислениеWA.Аннотация;
	ЮнитТест.ПроверитьЗаполненность(АннотацияWA);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АннотацияWA), Тип("АннотацияXS"));
	ЮнитТест.ПроверитьРавенство(АннотацияWA.Состав.Количество(), 1);

	ДокументацияWA = АннотацияWA.Состав.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ДокументацияWA);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ДокументацияWA), Тип("ДокументацияXS"));
	// ЮнитТест.ПроверитьРавенство(ДокументацияWA.Источник, "Washington");
	// //ЮнитТест.ПроверитьРавенство(ДокументацияWA.Markup, "Washington");

	ПеречислениеOR = ПростойТип.Фасеты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(ПеречислениеOR);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПеречислениеOR), Тип("ФасетПеречисленияXS"));
	ЮнитТест.ПроверитьРавенство(ПеречислениеOR.Значение, "OR");

	АннотацияOR = ПеречислениеOR.Аннотация;
	ЮнитТест.ПроверитьЗаполненность(АннотацияOR);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АннотацияOR), Тип("АннотацияXS"));
	ЮнитТест.ПроверитьРавенство(АннотацияOR.Состав.Количество(), 1);

	ДокументацияOR = АннотацияOR.Состав.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ДокументацияOR);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ДокументацияOR), Тип("ДокументацияXS"));
	// ЮнитТест.ПроверитьРавенство(ДокументацияOR.Источник, "Oregon");
	// //ЮнитТест.ПроверитьРавенство(ДокументацияOR.Markup, "Oregon");

	ПеречислениеID = ПростойТип.Фасеты.Получить(2);
	ЮнитТест.ПроверитьЗаполненность(ПеречислениеID);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПеречислениеID), Тип("ФасетПеречисленияXS"));
	ЮнитТест.ПроверитьРавенство(ПеречислениеID.Значение, "ID");

	АннотацияID = ПеречислениеID.Аннотация;
	ЮнитТест.ПроверитьЗаполненность(АннотацияID);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АннотацияID), Тип("АннотацияXS"));
	ЮнитТест.ПроверитьРавенство(АннотацияID.Состав.Количество(), 1);

	ДокументацияID = АннотацияID.Состав.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ДокументацияID);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ДокументацияID), Тип("ДокументацияXS"));
	// ЮнитТест.ПроверитьРавенство(ДокументацияID.Источник, "Idaho");
	// //ЮнитТест.ПроверитьРавенство(ДокументацияID.Markup, "Idaho");

КонецПроцедуры

Процедура ТестДокументацияXS() Экспорт

	Схема = ПримерДокументацияXS();
	ПроверитьДокументацияXS(Схема);

	Schema = ExampleXSDocumentation();
	ПроверитьДокументацияXS(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатДокументацияXS());
	ПроверитьДокументацияXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьДокументацияXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ИнформацияДляПриложенияXS

// Источник:
// 	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemaappinfo
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

Функция РезультатИнформацияДляПриложенияXS()
	
	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:element name='State'>
	|		<xs:annotation>
	|			<xs:documentation>State Name</xs:documentation>
	|			<xs:appinfo>Application Information</xs:appinfo>
	|		</xs:annotation>
	|	</xs:element>
	|</xs:schema>";

КонецФункции

Процедура ПроверитьИнформацияДляПриложенияXS(Схема)
	
	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	Элемент = Схема.ОбъявленияЭлементов.Получить("State");
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "State");

	АннотацияNorthwestStates = Элемент.Аннотация;
	ЮнитТест.ПроверитьЗаполненность(АннотацияNorthwestStates);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АннотацияNorthwestStates), Тип("АннотацияXS"));
	ЮнитТест.ПроверитьРавенство(АннотацияNorthwestStates.Состав.Количество(), 2);

	ДокументацияNorthwestStates = АннотацияNorthwestStates.Состав.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ДокументацияNorthwestStates);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ДокументацияNorthwestStates), Тип("ДокументацияXS"));
	// ЮнитТест.ПроверитьРавенство(ДокументацияNorthwestStates.Источник, "State Name");
	// //ЮнитТест.ПроверитьРавенство(ДокументацияNorthwestStates.Markup, "State Name");

	ИнформацияДляПриложения = АннотацияNorthwestStates.Состав.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(ИнформацияДляПриложения);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ИнформацияДляПриложения), Тип("ИнформацияДляПриложенияXS"));
	// ЮнитТест.ПроверитьРавенство(ИнформацияДляПриложения.Источник, "Application Information");
	// //ЮнитТест.ПроверитьРавенство(ИнформацияДляПриложения.Markup, "Application Information");

КонецПроцедуры

Процедура ТестИнформацияДляПриложенияXS() Экспорт

	Схема = ПримерИнформацияДляПриложенияXS();
	ПроверитьИнформацияДляПриложенияXS(Схема);

	Schema = ExampleXSAppInfo();
	ПроверитьИнформацияДляПриложенияXS(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатИнформацияДляПриложенияXS());
	ПроверитьИнформацияДляПриложенияXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьИнформацияДляПриложенияXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ОпределениеПростогоТипаXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemasimpletype
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

Функция РезультатОпределениеПростогоТипаXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:simpleType name='LotteryNumber'>
	|		<xs:restriction base='xs:int'>
	|			<xs:minInclusive value='1'/>
	|			<xs:maxInclusive value='99'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|
	|	<xs:simpleType name='LotteryNumberList'>
	|		<xs:list itemType='LotteryNumber'/>
	|	</xs:simpleType>
	|	
	|	<xs:simpleType name='LotteryNumbers'>
	|		<xs:restriction base='LotteryNumberList'>
	|			<xs:length value='5'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|	
	|	<xs:element name='TodaysLottery' type='LotteryNumbers'/>
	|	
	|</xs:schema>";

КонецФункции

Процедура ПроверитьОпределениеПростогоТипаXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 3);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ТипLotteryNumber = Схема.ОпределенияТипов.Получить("LotteryNumber");
	ЮнитТест.ПроверитьЗаполненность(ТипLotteryNumber);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипLotteryNumber), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumber.Имя, "LotteryNumber");
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumber.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumber.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "int"));
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumber.Фасеты.Количество(), 2);

	МинимальноеВключающееЗначения = ТипLotteryNumber.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(МинимальноеВключающееЗначения);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(МинимальноеВключающееЗначения), Тип("ФасетМинимальногоВключающегоЗначенияXS"));
	ЮнитТест.ПроверитьРавенство(МинимальноеВключающееЗначения.ЛексическоеЗначение, "1");

	МаксимальноеВключающееЗначения = ТипLotteryNumber.Фасеты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(МаксимальноеВключающееЗначения);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(МаксимальноеВключающееЗначения), Тип("ФасетМаксимальногоВключающегоЗначенияXS"));
	ЮнитТест.ПроверитьРавенство(МаксимальноеВключающееЗначения.ЛексическоеЗначение, "99");

	ТипСписокLotteryNumber = Схема.ОпределенияТипов.Получить("LotteryNumberList");
	ЮнитТест.ПроверитьЗаполненность(ТипСписокLotteryNumber);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипСписокLotteryNumber), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипСписокLotteryNumber.Имя, "LotteryNumberList");
	ЮнитТест.ПроверитьРавенство(ТипСписокLotteryNumber.Вариант, ВариантПростогоТипаXS.Список);
	ЮнитТест.ПроверитьРавенство(ТипСписокLotteryNumber.ИмяТипаЭлемента, Новый РасширенноеИмяXML("", "LotteryNumber"));

	ТипLotteryNumbers = Схема.ОпределенияТипов.Получить("LotteryNumbers");
	ЮнитТест.ПроверитьЗаполненность(ТипLotteryNumbers);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ТипLotteryNumbers), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumbers.Имя, "LotteryNumbers");
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumbers.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumbers.ИмяБазовогоТипа, Новый РасширенноеИмяXML("", "LotteryNumberList"));
	ЮнитТест.ПроверитьРавенство(ТипLotteryNumbers.Фасеты.Количество(), 1);

	Длина = ТипLotteryNumbers.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(Длина);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Длина), Тип("ФасетДлиныXS"));
	ЮнитТест.ПроверитьРавенство(Длина.Значение, 5);

КонецПроцедуры

Процедура ТестОпределениеПростогоТипаXS() Экспорт

	Схема = ПримерОпределениеПростогоТипаXS();
	ПроверитьОпределениеПростогоТипаXS(Схема);

	Schema = ExampleXSSimpleTypeDefinition();
	ПроверитьОпределениеПростогоТипаXS(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатОпределениеПростогоТипаXS());
	ПроверитьОпределениеПростогоТипаXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьОпределениеПростогоТипаXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ОпределениеПростогоТипаXS_Объединение

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemasimpletypeunion
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

Функция РезультатОпределениеПростогоТипаXS_Объединение()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:simpleType name='StringOrIntType'>
	|		<xs:union>
	|			<xs:simpleType>
	|				<xs:restriction base='xs:string'/>
	|			</xs:simpleType>
	|
	|			<xs:simpleType>
	|				<xs:restriction base='xs:int'/>
	|			</xs:simpleType>
	|		</xs:union>
	|	</xs:simpleType>
	|
	|	<xs:element name='size' type='StringOrIntType'/>
	|</xs:schema>";

КонецФункции

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

Процедура ТестОпределениеПростогоТипаXS_Объединение() Экспорт

	Схема = ПримерОпределениеПростогоТипаXS_Объединение();
	ПроверитьОпределениеПростогоТипаXS_Объединение(Схема);

	Schema = ExampleXSSimpleTypeDefinition_Union();
	ПроверитьОпределениеПростогоТипаXS_Объединение(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатОпределениеПростогоТипаXS_Объединение());
	ПроверитьОпределениеПростогоТипаXS_Объединение(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьОпределениеПростогоТипаXS_Объединение(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ФасетДлиныXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemalengthfacet
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

Функция РезультатФасетДлиныXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:simpleType name='ZipCodeType'>
	|		<xs:restriction base='xs:string'>
	|			<xs:length value='5'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|
	|	<xs:element name='Address'>
	|		<xs:complexType>
	|			<xs:attribute name='ZipCode' type='ZipCodeType'/>
	|		</xs:complexType>
	|	</xs:element>
	|
	|</xs:schema>";

КонецФункции

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

Процедура ТестФасетДлиныXS() Экспорт

	Схема = ПримерФасетДлиныXS();
	ПроверитьФасетДлиныXS(Схема);

	Schema = ExampleXSLengthFacet();
	ПроверитьФасетДлиныXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатФасетДлиныXS());
	ПроверитьФасетДлиныXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьФасетДлиныXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ФасетМинимальнойДлиныXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemaminlengthfacet
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

Функция РезультатФасетМинимальнойДлиныXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:simpleType name='ZipCodeType'>
	|		<xs:restriction base='xs:string'>
	|			<xs:minLength value='5'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|
	|	<xs:element name='Address'>
	|		<xs:complexType>
	|			<xs:attribute name='ZipCode' type='ZipCodeType'/>
	|		</xs:complexType>
	|	</xs:element>
	|</xs:schema>";

КонецФункции

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

Процедура ТестФасетМинимальнойДлиныXS() Экспорт

	Схема = ПримерФасетМинимальнойДлиныXS();
	ПроверитьФасетМинимальнойДлиныXS(Схема);

	Schema = ExampleXSMinLengthFacet();
	ПроверитьФасетМинимальнойДлиныXS(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатФасетМинимальнойДлиныXS());
	ПроверитьФасетМинимальнойДлиныXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьФасетМинимальнойДлиныXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ФасетМаксимальнойДлиныXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemamaxlengthfacet
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

Функция РезультатФасетМаксимальнойДлиныXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:simpleType name='ZipCodeType'>
	|		<xs:restriction base='xs:string'>
	|			<xs:maxLength value='10'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|
	|	<xs:element name='Address'>
	|		<xs:complexType>
	|			<xs:attribute name='ZipCode' type='ZipCodeType'/>
	|		</xs:complexType>
	|	</xs:element>
	|	
	|</xs:schema>";

КонецФункции

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

Процедура ТестФасетМаксимальнойДлиныXS() Экспорт

	Схема = ПримерФасетМаксимальнойДлиныXS();
	ПроверитьФасетМаксимальнойДлиныXS(Схема);

	Schema = ExampleXSMaxLengthFacet();
	ПроверитьФасетМаксимальнойДлиныXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатФасетМаксимальнойДлиныXS());
	ПроверитьФасетМаксимальнойДлиныXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьФасетМаксимальнойДлиныXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ФасетКоличестваРазрядовДробнойЧастиXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemafractiondigitsfacet
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

Функция РезультатФасетКоличестваРазрядовДробнойЧастиXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:simpleType name='RatingType'>
	|		<xs:restriction base='xs:decimal'>
	|			<xs:totalDigits value='2'/>
	|			<xs:fractionDigits value='1'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|
	|	<xs:element name='movie'>
	|		<xs:complexType>
	|			<xs:attribute name='rating' type='RatingType'/>
	|		</xs:complexType>
	|	</xs:element>
	|
	|</xs:schema>";

КонецФункции

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

Процедура ТестФасетКоличестваРазрядовДробнойЧастиXS() Экспорт

	Схема = ПримерФасетКоличестваРазрядовДробнойЧастиXS();
	ПроверитьФасетКоличестваРазрядовДробнойЧастиXS(Схема);

	Schema = ExampleXSFractionDigitsFacet();
	ПроверитьФасетКоличестваРазрядовДробнойЧастиXS(Schema);

	СхемаТекст = СхемаXMLИзТекста(РезультатФасетКоличестваРазрядовДробнойЧастиXS());
	ПроверитьФасетКоличестваРазрядовДробнойЧастиXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьФасетКоличестваРазрядовДробнойЧастиXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ФасетМинимальногоИсключающегоЗначенияXS 

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemaminexclusivefacet
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemamaxexclusivefacet
//
// Результат:
//	см. РезультатФасетМаксимальногоИсключающегоЗначенияXS 

Функция ПримерФасетМинимальногоИсключающегоЗначенияXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="WaitQueueLengthType">
	ТипДлинаОчереди = Новый ОпределениеПростогоТипаXS;
	ТипДлинаОчереди.Имя = "WaitQueueLengthType";
	
	// <xs:restriction base="xs:int">
	ТипДлинаОчереди.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "int");
	
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

Функция РезультатФасетМинимальногоИсключающегоЗначенияXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:simpleType name='WaitQueueLengthType'>
	|		<xs:restriction base='xs:int'>
	|			<xs:minExclusive value='5' />
	|			<xs:maxExclusive value='10' />
	|		</xs:restriction>
	|	</xs:simpleType>
	|	<xs:element name='Lobby'>
	|		<xs:complexType>
	|			<xs:attribute name='WaitQueueLength' type='WaitQueueLengthType' />
	|		</xs:complexType>
	|	</xs:element>
	|</xs:schema>";

КонецФункции

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
	ЮнитТест.ПроверитьРавенство(МинимальноИсключая.ЛексическоеЗначение, "5");
	// ЮнитТест.ПроверитьРавенство(МинимальноИсключая.Значение, 5);
	
	МаксимальноИсключая = ТипДлинаОчереди.Фасеты.Получить(1);
	ЮнитТест.ПроверитьЗаполненность(МаксимальноИсключая);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(МаксимальноИсключая), Тип("ФасетМаксимальногоИсключающегоЗначенияXS"));
	ЮнитТест.ПроверитьРавенство(МаксимальноИсключая.ЛексическоеЗначение, "10");
	// ЮнитТест.ПроверитьРавенство(МаксимальноИсключая.Значение, 10);
	
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

Процедура ТестФасетМинимальногоИсключающегоЗначенияXS() Экспорт

	Схема = ПримерФасетМинимальногоИсключающегоЗначенияXS();
	ПроверитьФасетМинимальногоИсключающегоЗначенияXS(Схема);

	Schema = ExampleXSMinExclusiveFacet();
	ПроверитьФасетМинимальногоИсключающегоЗначенияXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатФасетМинимальногоИсключающегоЗначенияXS());
	ПроверитьФасетМинимальногоИсключающегоЗначенияXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьФасетМинимальногоИсключающегоЗначенияXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ФасетОбразцаXS

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemapatternfacet
//
// Результат:
//	см. РезультатФасетОбразцаXS

Функция ПримерФасетОбразцаXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="ZipCodeType">
	ТипПочтовыйИндекс = Новый ОпределениеПростогоТипаXS;
	ТипПочтовыйИндекс.Имя = "ZipCodeType";

	// <xs:restriction base="xs:string">
	ТипПочтовыйИндекс.Вариант = ВариантПростогоТипаXS.Атомарная;
	ТипПочтовыйИндекс.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");

	// <xs:pattern value="[0-9]{5}(-[0-9]{4})?"/>
	ФасетОбразца = Новый ФасетОбразцаXS;
	ФасетОбразца.Значение = "[0-9]{5}(-[0-9]{4})?";
	ТипПочтовыйИндекс.Фасеты.Добавить(ФасетОбразца);

	Схема.Содержимое.Добавить(ТипПочтовыйИндекс);

	// <xs:element name="Address">
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "Address";

	// <xs:complexType>
	СоставнойТип = Новый ОпределениеСоставногоТипаXS;

	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	АтрибутПочтовыйИндекс = Новый ОбъявлениеАтрибутаXS;
	АтрибутПочтовыйИндекс.Имя = "ZipCode";
	АтрибутПочтовыйИндекс.ИмяТипа = New XMLExpandedName("", "ZipCodeType");
	СоставнойТип.Атрибуты.Добавить(АтрибутПочтовыйИндекс);

	Элемент.АнонимноеОпределениеТипа = СоставнойТип;
	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSPatternFacet()

	schema = New XMLSchema;

	// <xs:simpleType name="ZipCodeType">
	ZipCodeType = New XSSimpleTypeDefinition;
	ZipCodeType.Name = "ZipCodeType";

	// <xs:restriction base="xs:string">
	ZipCodeType.Variety = XSSimpleTypeVariety.Atomic;
	ZipCodeType.BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");

	// <xs:pattern value="[0-9]{5}(-[0-9]{4})?"/>
	pattern = new XSPatternFacet;
	pattern.Value = "[0-9]{5}(-[0-9]{4})?";
	ZipCodeType.Facets.Add(pattern);

	schema.Content.Add(ZipCodeType);

	// <xs:element name="Address">
	element = New XSElementDeclaration;
	element.Name = "Address";

	// <xs:complexType>
	complexType = New XSComplexTypeDefinition;;

	// <xs:attribute name="ZipCode" type="ZipCodeType"/>
	ZipCodeAttribute = New XSAttributeDeclaration;
	ZipCodeAttribute.Name = "ZipCode";
	ZipCodeAttribute.TypeName = New XMLExpandedName("", "ZipCodeType");
	complexType.Attributes.Add(ZipCodeAttribute);

	element.AnonymousTypeDefinition = complexType;
	schema.Content.Add(element);

	return schema;

EndFunction

Функция РезультатФасетОбразцаXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:simpleType name='ZipCodeType'>
	|		<xs:restriction base='xs:string'>
	|			<xs:pattern value='[0-9]{5}(-[0-9]{4})?'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|
	|	<xs:element name='Address'>
	|		<xs:complexType>
	|			<xs:attribute name='ZipCode' type='ZipCodeType'/>
	|		</xs:complexType>
	|	</xs:element>
	|
	|</xs:schema>";

КонецФункции

Процедура ПроверитьФасетОбразцаXS(Схема)

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

	ФасетОбразца = ТипПочтовыйИндекс.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ФасетОбразца);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ФасетОбразца), Тип("ФасетОбразцаXS"));
	ЮнитТест.ПроверитьРавенство(ФасетОбразца.Значение, "[0-9]{5}(-[0-9]{4})?");

	Элемент = Схема.ОбъявленияЭлементов.Получить("Address"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "Address");

	СоставнойТип = Элемент.АнонимноеОпределениеТипа;
	ЮнитТест.ПроверитьЗаполненность(СоставнойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(СоставнойТип), Тип("ОпределениеСоставногоТипаXS"));
	ЮнитТест.ПроверитьРавенство(СоставнойТип.Атрибуты.Количество(), 1);

	АтрибутДлинаОчереди = СоставнойТип.Атрибуты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(АтрибутДлинаОчереди);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(АтрибутДлинаОчереди), Тип("ОбъявлениеАтрибутаXS"));
	ЮнитТест.ПроверитьРавенство(АтрибутДлинаОчереди.Имя, "ZipCode");
	ЮнитТест.ПроверитьРавенство(АтрибутДлинаОчереди.ИмяТипа, Новый РасширенноеИмяXML("", "ZipCodeType"));

КонецПроцедуры

Процедура ТестФасетОбразцаXS() Экспорт
	
	Схема = ПримерФасетОбразцаXS();
	ПроверитьФасетОбразцаXS(Схема);

	Schema = ExampleXSPatternFacet();
	ПроверитьФасетОбразцаXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатФасетОбразцаXS());
	ПроверитьФасетОбразцаXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьФасетОбразцаXS(СхемаСериализатор);

КонецПроцедуры

#КонецОбласти

#Область ФасетПробельныхСимволовXS

// Источник:
//	https://docs.microsoft.com/dotnet/api/system.xml.schema.xmlschemawhitespacefacet
//
// Результат:
//	см. РезультатФасетПробельныхСимволовXS

Функция ПримерФасетПробельныхСимволовXS()

	Схема = Новый СхемаXML;

	// <xs:simpleType name="NameType">
	ПростойТип = Новый ОпределениеПростогоТипаXS;
	ПростойТип.Имя = "NameType";

	// <xs:restriction base="xs:string">
	ПростойТип.Вариант = ВариантПростогоТипаXS.Атомарная;
	ПростойТип.ИмяБазовогоТипа = Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string");

	// <xs:whiteSpace value="collapse"/>
	ФасетПробельныхСимволов = Новый ФасетПробельныхСимволовXS;
	ФасетПробельныхСимволов.Значение = ОбработкаПробельныхСимволовXS.Сворачивать;
	ПростойТип.Фасеты.Добавить(ФасетПробельныхСимволов);

	Схема.Содержимое.Добавить(ПростойТип);

	// <xs:element name="LastName" type="NameType"/>
	Элемент = Новый ОбъявлениеЭлементаXS;
	Элемент.Имя = "LastName";
	Элемент.ИмяТипа = Новый РасширенноеИмяXML("", "NameType");

	Схема.Содержимое.Добавить(Элемент);
	
	Возврат Схема;

КонецФункции

Function ExampleXSWhitespaceFacet()

	schema = New XMLSchema;

	// <xs:simpleType name="ZipCodeType">
	NameType  = New XSSimpleTypeDefinition;
	NameType .Name = "NameType";

	// <xs:restriction base="xs:string">
	NameType .Variety = XSSimpleTypeVariety.Atomic;
	NameType .BaseTypeName = New XMLExpandedName("http://www.w3.org/2001/XMLSchema", "string");

	// <xs:pattern value="[0-9]{5}(-[0-9]{4})?"/>
	WhitespaceFacet = new XSWhitespaceFacet;
	WhitespaceFacet.Value = XSWhitespaceHandling.Collapse;
	NameType .Facets.Add(WhitespaceFacet);

	schema.Content.Add(NameType );

	// <xs:element name="LastName" type="NameType"/>
	element = New XSElementDeclaration;
	element.Name = "LastName";
	element.TypeName = New XMLExpandedName("", "NameType");

	schema.Content.Add(element);

	return schema;

EndFunction

Функция РезультатФасетПробельныхСимволовXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:simpleType name='NameType'>
	|		<xs:restriction base='xs:string'>
	|			<xs:whiteSpace value='collapse'/>
	|		</xs:restriction>
	|	</xs:simpleType>
	|
	|	<xs:element name='LastName' type='NameType'/>
	|
	|</xs:schema>";

КонецФункции

Процедура ПроверитьФасетПробельныхСимволовXS(Схема)

	ЮнитТест.ПроверитьЗаполненность(Схема);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Схема), Тип("СхемаXML"));
	ЮнитТест.ПроверитьРавенство(Схема.Содержимое.Количество(), 2);
	ЮнитТест.ПроверитьРавенство(Схема.ОпределенияТипов.Количество(), 1);
	ЮнитТест.ПроверитьРавенство(Схема.ОбъявленияЭлементов.Количество(), 1);

	ПростойТип = Схема.ОпределенияТипов.Получить("NameType");
	ЮнитТест.ПроверитьЗаполненность(ПростойТип);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ПростойТип), Тип("ОпределениеПростогоТипаXS"));
	ЮнитТест.ПроверитьРавенство(ПростойТип.Имя, "NameType");
	ЮнитТест.ПроверитьРавенство(ПростойТип.Вариант, ВариантПростогоТипаXS.Атомарная);
	ЮнитТест.ПроверитьРавенство(ПростойТип.ИмяБазовогоТипа, Новый РасширенноеИмяXML("http://www.w3.org/2001/XMLSchema", "string"));
	ЮнитТест.ПроверитьРавенство(ПростойТип.Фасеты.Количество(), 1);

	ФасетПробельныхСимволов = ПростойТип.Фасеты.Получить(0);
	ЮнитТест.ПроверитьЗаполненность(ФасетПробельныхСимволов);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(ФасетПробельныхСимволов), Тип("ФасетПробельныхСимволовXS"));
	ЮнитТест.ПроверитьРавенство(ФасетПробельныхСимволов.Значение, ОбработкаПробельныхСимволовXS.Сворачивать);
	ЮнитТест.ПроверитьРавенство(ФасетПробельныхСимволов.ЛексическоеЗначение, "collapse");

	Элемент = Схема.ОбъявленияЭлементов.Получить("LastName"); 
	ЮнитТест.ПроверитьЗаполненность(Элемент);
	ЮнитТест.ПроверитьРавенство(ТипЗнч(Элемент), Тип("ОбъявлениеЭлементаXS"));
	ЮнитТест.ПроверитьРавенство(Элемент.Имя, "LastName");
	ЮнитТест.ПроверитьРавенство(Элемент.ИмяТипа, Новый РасширенноеИмяXML("", "NameType"));

КонецПроцедуры

Процедура ТестФасетПробельныхСимволовXS() Экспорт
	
	Схема = ПримерФасетПробельныхСимволовXS();
	ПроверитьФасетПробельныхСимволовXS(Схема);

	Schema = ExampleXSWhitespaceFacet();
	ПроверитьФасетПробельныхСимволовXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатФасетПробельныхСимволовXS());
	ПроверитьФасетПробельныхСимволовXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьФасетПробельныхСимволовXS(СхемаСериализатор);

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

Функция РезультатМаскаXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|
	|	<xs:element name='htmlText'>
	|		<xs:complexType>
	|			<xs:sequence>
	|				<xs:any 
	|					namespace='http://www.w3.org/1999/xhtml'
	|					processContents='lax'
	|					minOccurs='1' maxOccurs='unbounded'/>
	|			</xs:sequence>
	|		</xs:complexType>
	|	</xs:element>
	|
	|	<xs:element name='stringElementWithAnyAttribute'>
	|		<xs:complexType>
	|			<xs:simpleContent>
	|				<xs:extension base='xs:string'>
	|					<xs:anyAttribute namespace='##targetNamespace'/>
	|				</xs:extension>
	|			</xs:simpleContent>
	|		</xs:complexType>
	|	</xs:element>
	|
	|</xs:schema>";

КонецФункции

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
	ПроверитьМаскаXS(Схема);

	Schema = ExampleXSWildcard();
	ПроверитьМаскаXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатМаскаXS());
	ПроверитьМаскаXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьМаскаXS(СхемаСериализатор);

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

Функция РезультатОпределениеГруппыАтрибутовXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:attributeGroup name='myAttributeGroup'>
	|		<xs:attribute name='someattribute1' type='xs:integer'/>
	|		<xs:attribute name='someattribute2' type='xs:string'/>
	|	</xs:attributeGroup>
	|	<xs:attributeGroup name='myAttributeGroupB'>
	|		<xs:attribute name='someattribute20' type='xs:date'/>
	|		<xs:attributeGroup ref='myAttributeGroup'/>
	|		<xs:anyAttribute namespace='##targetNamespace'/>
	|	</xs:attributeGroup>
	|	<xs:complexType name='myElementType'>
	|		<xs:attributeGroup ref='myAttributeGroup'/>
	|	</xs:complexType>
	|</xs:schema>";

КонецФункции

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
	ПроверитьОпределениеГруппыАтрибутовXS(Схема);

	Schema = ExampleXSAttributeGroupDefinition();
	ПроверитьОпределениеГруппыАтрибутовXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатОпределениеГруппыАтрибутовXS());
	ПроверитьОпределениеГруппыАтрибутовXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьОпределениеГруппыАтрибутовXS(СхемаСериализатор);

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

Функция РезультатОбъявлениеНотацииXS()

	Возврат
	"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:notation name='gif' public='image/gif' system='view.exe'/>
	|	<xs:notation name='jpeg' public='image/jpeg' system='view.exe'/>
	|	<xs:element name='image'>
	|		<xs:complexType>
	|			<xs:attribute name='type'>
	|				<xs:simpleType>
	|					<xs:restriction base='xs:NOTATION'>
	|						<xs:enumeration value='gif'/>
	|						<xs:enumeration value='jpeg'/>
	|					</xs:restriction>
	|				</xs:simpleType>
	|			</xs:attribute>
	|		</xs:complexType>
	|	</xs:element>
	|</xs:schema>";
	
КонецФункции

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
	ПроверитьОбъявлениеНотацииXS(Схема);

	Schema = ExampleXSNotationDeclaration();
	ПроверитьОбъявлениеНотацииXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатОбъявлениеНотацииXS());
	ПроверитьОбъявлениеНотацииXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьОбъявлениеНотацииXS(СхемаСериализатор);

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
	Группа.ГруппаМодели = Последовательность;
	
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
	myGroupOfThings.ModelGroup = sequence;
	
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

Функция РезультатОпределениеГруппыМоделиXS()

	Возврат
	"<xs:schema  xmlns:xs='http://www.w3.org/2001/XMLSchema'>
	|	<xs:element name='thing1' type='xs:string'/>
	|	<xs:element name='thing2' type='xs:string'/>
	|	<xs:element name='thing3' type='xs:string'/>
	|	<xs:attribute name='myAttribute' type='xs:decimal'/>
	|	<xs:group name='myGroupOfThings'>
	|		<xs:sequence>
	|			<xs:element ref='thing1'/>
	|			<xs:element ref='thing2'/>
	|			<xs:element ref='thing3'/>
	|		</xs:sequence>
	|	</xs:group>
	|	<xs:complexType name='myComplexType'>
	|		<xs:group ref='myGroupOfThings'/>
	|		<xs:attribute ref='myAttribute'/>
	|	</xs:complexType>
	|</xs:schema>";

КонецФункции

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

	Последовательность = Группа.ГруппаМодели;
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
	ПроверитьОпределениеГруппыМоделиXS(Схема);

	Schema = ExampleXSModelGroupDefinition();
	ПроверитьОпределениеГруппыМоделиXS(Schema);
	
	СхемаТекст = СхемаXMLИзТекста(РезультатОпределениеГруппыМоделиXS());
	ПроверитьОпределениеГруппыМоделиXS(СхемаТекст);

	СхемаСериализатор = СериализоватьДесериализоватьСхемуXML(Схема);
	ПроверитьОпределениеГруппыМоделиXS(СхемаСериализатор);

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