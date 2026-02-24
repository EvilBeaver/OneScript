/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Contexts.Converters;
using OneScript.Execution;
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
    public int ParameterConversion([BslValueConverter(typeof(TestDtoConverter))] TestDto dto)
    {
        return dto.Integer;
    }

    [ContextMethod("КонвертацияВозвращаемогоЗначения")]
    [BslValueConverter(typeof(TestDtoConverter))]
    public TestDto ReturnValueConversion()
    {
        return new TestDto { Integer = 42 };
    }

    [ScriptConstructor]
    public static IValue DefaultConstructor([BslValueConverter(typeof(TestDtoConverter))] TestDto dto)
    {
        var instance = new TestClassWithConverters
        {
            ValueFromConstructor = dto
        };
        return instance;
    }
}

public class TestDtoConverter : IBslValueConverter
{
    public BslValue ToBslValue(object value, IBslValueConverter defaultConverter, IBslProcess process)
    {
        var realValue = (TestDto)value;
        var wrapper = new StructureImpl();
        wrapper.Insert("Integer", BslNumericValue.Create(realValue.Integer));
        
        return wrapper;
    }

    public object ToClrValue(BslValue value, IBslValueConverter defaultConverter, IBslProcess process)
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