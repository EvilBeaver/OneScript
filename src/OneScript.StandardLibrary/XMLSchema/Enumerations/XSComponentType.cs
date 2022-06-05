/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts.Enums;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("XSComponentType", "ТипКомпонентыXS")]
    public enum XSComponentType
    {

        [EnumValue("Annotation", "Аннотация")]
        Annotation,

        [EnumValue("Include", "Включение")]
        Include,

        [EnumValue("ModelGroup", "ГруппаМодели")]
        ModelGroup,

        [EnumValue("Documentation", "Документация")]
        Documentation,

        [EnumValue("Import", "Импорт")]
        Import,

        [EnumValue("AppInfo", "ИнформацияПриложения")]
        AppInfo,

        [EnumValue("AttributeUse", "ИспользованиеАтрибута")]
        AttributeUse,

        [EnumValue("MaxInclusiveFacet", "МаксимальноВключающийФасет")]
        MaxInclusiveFacet,

        [EnumValue("MaxExclusiveFacet", "МаксимальноИсключающийФасет")]
        MaxExclusiveFacet,

        [EnumValue("Wildcard", "Маска")]
        Wildcard,

        [EnumValue("MinInclusiveFacet", "МинимальноВключающийФасет")]
        MinInclusiveFacet,

        [EnumValue("MinExclusiveFacet", "МинимальноИсключающийФасет")]
        MinExclusiveFacet,

        [EnumValue("AttributeDeclaration", "ОбъявлениеАтрибута")]
        AttributeDeclaration,

        [EnumValue("NotationDeclaration", "ОбъявлениеНотации")]
        NotationDeclaration,

        [EnumValue("ElementDeclaration", "ОбъявлениеЭлемента")]
        ElementDeclaration,

        [EnumValue("XPathDefinition", "ОпределениеXPath")]
        XPathDefinition,

        [EnumValue("AttributeGroupDefinition", "ОпределениеГруппыАтрибутов")]
        AttributeGroupDefinition,

        [EnumValue("ModelGroupDefinition", "ОпределениеГруппыМодели")]
        ModelGroupDefinition,

        [EnumValue("IdentityConstraintDefinition", "ОпределениеОграниченияИдентичности")]
        IdentityConstraintDefinition,

        [EnumValue("SimpleTypeDefinition", "ОпределениеПростогоТипа")]
        SimpleTypeDefinition,

        [EnumValue("ComplexTypeDefinition", "ОпределениеСоставногоТипа")]
        ComplexTypeDefinition,

        [EnumValue("Redefine", "Переопределение")]
        Redefine,

        [EnumValue("Schema", "Схема")]
        Schema,

        [EnumValue("LengthFacet", "ФасетДлины")]
        LengthFacet,

        [EnumValue("FractionDigitsFacet", "ФасетКоличестваРазрядовДробнойЧасти")]
        FractionDigitsFacet,

        [EnumValue("MaxLengthFacet", "ФасетМаксимальнойДлины")]
        MaxLengthFacet,

        [EnumValue("MinLengthFacet", "ФасетМинимальнойДлины")]
        MinLengthFacet,

        [EnumValue("PatternFacet", "ФасетОбразца")]
        PatternFacet,

        [EnumValue("TotalDigitsFacet", "ФасетОбщегоКоличестваРазрядов")]
        TotalDigitsFacet,

        [EnumValue("EnumerationFacet", "ФасетПеречисления")]
        EnumerationFacet,

        [EnumValue("WhitespaceFacet", "ФасетПробельныхСимволов")]
        WhitespaceFacet,

        [EnumValue("Particle", "Фрагмент")]
        Particle
    }
}
