/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Contexts.Converters;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.Core.Tests;

[ContextClass("КлассСКонвертером", "Convertable")]
public class TestClassWithConverters : AutoContext<TestClassWithConverters>
{
    public TestDto ValueFromConstructor { get; set; }

    [ContextMethod("КонвертацияПараметра")]
    public int ParameterConversion([BslValueConverter<TestDtoConverter>] TestDto dto)
    {
        return dto.Integer;
    }

    [ContextMethod("КонвертацияВозвращаемогоЗначения")]
    [BslValueConverter<TestDtoConverter>]
    public TestDto ReturnValueConversion()
    {
        return new TestDto { Integer = 42 };
    }

    [ContextProperty("ДТО", "Dto")]
    [BslValueConverter<TestDtoConverter>]
    public TestDto DtoProperty { get; set; }

    [ContextProperty("ДТОТолькоЧтение", "DtoReadOnly", CanWrite = false)]
    [BslValueConverter<TestDtoConverter>]
    public TestDto DtoReadOnlyProperty => new TestDto { Integer = 100 };

    [ScriptConstructor]
    public static IValue DefaultConstructor([BslValueConverter<TestDtoConverter>] TestDto dto)
    {
        var instance = new TestClassWithConverters
        {
            ValueFromConstructor = dto
        };
        return instance;
    }
}

public sealed class TestDtoConverter : IBslValueConverter
{
    public static BslValue ToBslValue(object value)
    {
        var realValue = (TestDto)value;
        var wrapper = new StructureImpl();
        wrapper.Insert("Integer", BslNumericValue.Create(realValue.Integer));
        return wrapper;
    }

    public static object ToClrValue(BslValue value)
    {
        var integer = ((StructureImpl)value).GetIndexedValue(ValueFactory.Create("Integer")).AsNumber();
        return new TestDto
        {
            Integer = (int)integer
        };
    }
}

public class TestDto
{
    public int Integer { get; set; }
}
