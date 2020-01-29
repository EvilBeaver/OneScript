/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;

namespace OneScript.StandardLibrary.XMLSchema.Enumerations
{
    [EnumerationType("XSComponentType", "ТипКомпонентыXS")]
    public enum XSComponentType
    {

        [EnumItem("Annotation", "Аннотация")]
        Annotation,

        [EnumItem("Include", "Включение")]
        Include,

        [EnumItem("ModelGroup", "ГруппаМодели")]
        ModelGroup,

        [EnumItem("Documentation", "Документация")]
        Documentation,

        [EnumItem("Import", "Импорт")]
        Import,

        [EnumItem("AppInfo", "ИнформацияПриложения")]
        AppInfo,

        [EnumItem("AttributeUse", "ИспользованиеАтрибута")]
        AttributeUse,

        [EnumItem("MaxInclusiveFacet", "МаксимальноВключающийФасет")]
        MaxInclusiveFacet,

        [EnumItem("MaxExclusiveFacet", "МаксимальноИсключающийФасет")]
        MaxExclusiveFacet,

        [EnumItem("Wildcard", "Маска")]
        Wildcard,

        [EnumItem("MinInclusiveFacet", "МинимальноВключающийФасет")]
        MinInclusiveFacet,

        [EnumItem("MinExclusiveFacet", "МинимальноИсключающийФасет")]
        MinExclusiveFacet,

        [EnumItem("AttributeDeclaration", "ОбъявлениеАтрибута")]
        AttributeDeclaration,

        [EnumItem("NotationDeclaration", "ОбъявлениеНотации")]
        NotationDeclaration,

        [EnumItem("ElementDeclaration", "ОбъявлениеЭлемента")]
        ElementDeclaration,

        [EnumItem("XPathDefinition", "ОпределениеXPath")]
        XPathDefinition,

        [EnumItem("AttributeGroupDefinition", "ОпределениеГруппыАтрибутов")]
        AttributeGroupDefinition,

        [EnumItem("ModelGroupDefinition", "ОпределениеГруппыМодели")]
        ModelGroupDefinition,

        [EnumItem("IdentityConstraintDefinition", "ОпределениеОграниченияИдентичности")]
        IdentityConstraintDefinition,

        [EnumItem("SimpleTypeDefinition", "ОпределениеПростогоТипа")]
        SimpleTypeDefinition,

        [EnumItem("ComplexTypeDefinition", "ОпределениеСоставногоТипа")]
        ComplexTypeDefinition,

        [EnumItem("Redefine", "Переопределение")]
        Redefine,

        [EnumItem("Schema", "Схема")]
        Schema,

        [EnumItem("LengthFacet", "ФасетДлины")]
        LengthFacet,

        [EnumItem("FractionDigitsFacet", "ФасетКоличестваРазрядовДробнойЧасти")]
        FractionDigitsFacet,

        [EnumItem("MaxLengthFacet", "ФасетМаксимальнойДлины")]
        MaxLengthFacet,

        [EnumItem("MinLengthFacet", "ФасетМинимальнойДлины")]
        MinLengthFacet,

        [EnumItem("PatternFacet", "ФасетОбразца")]
        PatternFacet,

        [EnumItem("TotalDigitsFacet", "ФасетОбщегоКоличестваРазрядов")]
        TotalDigitsFacet,

        [EnumItem("EnumerationFacet", "ФасетПеречисления")]
        EnumerationFacet,

        [EnumItem("WhitespaceFacet", "ФасетПробельныхСимволов")]
        WhitespaceFacet,

        [EnumItem("Particle", "Фрагмент")]
        Particle
    }
}
