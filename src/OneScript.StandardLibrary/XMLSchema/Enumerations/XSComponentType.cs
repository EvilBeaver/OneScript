/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("ТипКомпонентыXS", "XSComponentType")]
    public enum XSComponentType
    {

        [EnumValue("Аннотация", "Annotation")]
        Annotation,

        [EnumValue("Включение", "Include")]
        Include,

        [EnumValue("ГруппаМодели", "ModelGroup")]
        ModelGroup,

        [EnumValue("Документация", "Documentation")]
        Documentation,

        [EnumValue("Импорт", "Import")]
        Import,

        [EnumValue("ИнформацияПриложения", "AppInfo")]
        AppInfo,

        [EnumValue("ИспользованиеАтрибута", "AttributeUse")]
        AttributeUse,

        [EnumValue("МаксимальноВключающийФасет", "MaxInclusiveFacet")]
        MaxInclusiveFacet,

        [EnumValue("МаксимальноИсключающийФасет", "MaxExclusiveFacet")]
        MaxExclusiveFacet,

        [EnumValue("Маска", "Wildcard")]
        Wildcard,

        [EnumValue("МинимальноВключающийФасет", "MinInclusiveFacet")]
        MinInclusiveFacet,

        [EnumValue("МинимальноИсключающийФасет", "MinExclusiveFacet")]
        MinExclusiveFacet,

        [EnumValue("ОбъявлениеАтрибута", "AttributeDeclaration")]
        AttributeDeclaration,

        [EnumValue("ОбъявлениеНотации", "NotationDeclaration")]
        NotationDeclaration,

        [EnumValue("ОбъявлениеЭлемента", "ElementDeclaration")]
        ElementDeclaration,

        [EnumValue("ОпределениеXPath", "XPathDefinition")]
        XPathDefinition,

        [EnumValue("ОпределениеГруппыАтрибутов", "AttributeGroupDefinition")]
        AttributeGroupDefinition,

        [EnumValue("ОпределениеГруппыМодели", "ModelGroupDefinition")]
        ModelGroupDefinition,

        [EnumValue("ОпределениеОграниченияИдентичности", "IdentityConstraintDefinition")]
        IdentityConstraintDefinition,

        [EnumValue("ОпределениеПростогоТипа", "SimpleTypeDefinition")]
        SimpleTypeDefinition,

        [EnumValue("ОпределениеСоставногоТипа", "ComplexTypeDefinition")]
        ComplexTypeDefinition,

        [EnumValue("Переопределение", "Redefine")]
        Redefine,

        [EnumValue("Схема", "Schema")]
        Schema,

        [EnumValue("ФасетДлины", "LengthFacet")]
        LengthFacet,

        [EnumValue("ФасетКоличестваРазрядовДробнойЧасти", "FractionDigitsFacet")]
        FractionDigitsFacet,

        [EnumValue("ФасетМаксимальнойДлины", "MaxLengthFacet")]
        MaxLengthFacet,

        [EnumValue("ФасетМинимальнойДлины", "MinLengthFacet")]
        MinLengthFacet,

        [EnumValue("ФасетОбразца", "PatternFacet")]
        PatternFacet,

        [EnumValue("ФасетОбщегоКоличестваРазрядов", "TotalDigitsFacet")]
        TotalDigitsFacet,

        [EnumValue("ФасетПеречисления", "EnumerationFacet")]
        EnumerationFacet,

        [EnumValue("ФасетПробельныхСимволов", "WhitespaceFacet")]
        WhitespaceFacet,

        [EnumValue("Фрагмент", "Particle")]
        Particle
    }
}
