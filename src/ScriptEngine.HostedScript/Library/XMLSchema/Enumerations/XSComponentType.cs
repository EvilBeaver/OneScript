/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine.HostedScript.Library.XMLSchema
{
    [EnumerationType("ТипКомпонентыXS", "XSComponentType")]
    public enum XSComponentType
    {

        [EnumItem("Аннотация", "Annotation")]
        Annotation,

        [EnumItem("ИнформацияПриложения", "AppInfo")]
        AppInfo,

        [EnumItem("Документация", "Documentation")]
        Documentation,

        [EnumItem("ОпределениеОграниченияИдентичности", "IdentityConstraintDefinition")]
        IdentityConstraintDefinition,

        [EnumItem("ОбъявлениеАтрибута", "AttributeDeclaration")]
        AttributeDeclaration,

        [EnumItem("ОбъявлениеЭлемента", "ElementDeclaration")]
        ElementDeclaration,

        [EnumItem("ОбъявлениеНотации", "NotationDeclaration")]
        NotationDeclaration,

        [EnumItem("ОпределениеПростогоТипа", "SimpleTypeDefinition")]
        SimpleTypeDefinition,

        [EnumItem("ОпределениеСоставногоТипа", "ComplexTypeDefinition")]
        ComplexTypeDefinition,

        [EnumItem("ОпределениеГруппыАтрибутов", "AttributeGroupDefinition")]
        AttributeGroupDefinition,

        [EnumItem("ОпределениеГруппыМодели", "ModelGroupDefinition")]
        ModelGroupDefinition,

        [EnumItem("Фрагмент", "Particle")]
        Particle,

        [EnumItem("ОпределениеXPath", "XPathDefinition")]
        XPathDefinition,

        [EnumItem("ФасетОбщегоКоличестваРазрядов", "TotalDigitsFacet")]
        TotalDigitsFacet,

        [EnumItem("ФасетКоличестваРазрядовДробнойЧасти", "FractionDigitsFacet")]
        FractionDigitsFacet,

        [EnumItem("ФасетДлины", "LengthFacet")]
        LengthFacet,

        [EnumItem("ФасетМинимальнойДлины", "MinLengthFacet")]
        MinLengthFacet,

        [EnumItem("ФасетМаксимальнойДлины", "MaxLengthFacet")]
        MaxLengthFacet,

        [EnumItem("МинимальноИсключающийФасет", "MinExclusiveFacet")]
        MinExclusiveFacet,

        [EnumItem("МинимальноВключающийФасет", "MinInclusiveFacet")]
        MinInclusiveFacet,

        [EnumItem("МаксимальноИсключающийФасет", "MaxExclusiveFacet")]
        MaxExclusiveFacet,

        [EnumItem("МаксимальноВключающийФасет", "MaxInclusiveFacet")]
        MaxInclusiveFacet,

        [EnumItem("ФасетПеречисления", "EnumerationFacet")]
        EnumerationFacet,

        [EnumItem("ФасетОбразца", "PatternFacet")]
        PatternFacet,

        [EnumItem("ФасетПробельныхСимволов", "WhitespaceFacet")]
        WhitespaceFacet,

        [EnumItem("Импорт", "Import")]
        Import,

        [EnumItem("Переопределение", "Redefine")]
        Redefine,

        [EnumItem("Включение", "Include")]
        Include,

        [EnumItem("Маска", "Wildcard")]
        Wildcard,

        [EnumItem("ГруппаМодели", "ModelGroup")]
        ModelGroup,

        [EnumItem("ИспользованиеАтрибута", "AttributeUse")]
        AttributeUse,

        [EnumItem("Схема", "Schema")]
        Schema
    }
}
