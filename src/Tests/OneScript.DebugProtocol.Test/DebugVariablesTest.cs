/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Linq;
using FluentAssertions;
using OneScript.DebugServices;
using OneScript.StandardLibrary;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Collections.ValueTree;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Types;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    // TODO стандартный визуализатор ничего не знает про особенности коллекций
    // После завершения ветки breaking-refactory визуализатор, возможно переедет в другую сборку
    // пока оставляю так, что IVariableVisualizer живет в DebugServices
    public class DebugVariablesTest
    {
        
        public DebugVariablesTest()
        {
            Visualizer = new DefaultVariableVisualizer();
            Manager = new DefaultTypeManager();
        }

        private ITypeManager Manager { get; }
        
        private IValue GetInstance(Type valueType, IValue[] args)
        {
            Manager.RegisterClass(valueType);

            var registeredType = Manager.GetTypeByFrameworkType(valueType);
            var factory = (TypeFactory)Manager.GetFactoryFor(registeredType);
            return factory.Activate(new TypeActivationContext
            {
                TypeManager = Manager,
                TypeName = registeredType.Name
            },args);
        }
        
        private T GetInstance<T>(params IValue[] args) where T : IValue
        {
            return (T) GetInstance(typeof(T), args);
        }

        public IVariableVisualizer Visualizer { get; set; }
        
        [Fact]
        public void SimpleValuePresentation()
        {
            var str = ValueFactory.Create("string value");
            
            var debuggerVar = Visualizer.GetVariable(Contexts.Variable.Create(str,"myString"));

            debuggerVar.Name.Should().Be("myString");
            
            debuggerVar.TypeName.Should().Be("Строка");
            debuggerVar.Presentation.Should().Be("string value");
            debuggerVar.IsStructured.Should().BeFalse();
            
            var number = ValueFactory.Create(27.2m);
            debuggerVar = Visualizer.GetVariable(Contexts.Variable.Create(number,"myInt"));
            debuggerVar.Name.Should().Be("myInt");
            debuggerVar.TypeName.Should().Be("Число");
            debuggerVar.Presentation.Should().Be("27.2");
            debuggerVar.IsStructured.Should().BeFalse();
        }
        
        [Fact]
        public void ObjectPresentation()
        {
            var obj = GetInstance<FileContext>(ValueFactory.Create("somefile.txt")); 

            var debuggerVar = Visualizer.GetVariable(Contexts.Variable.Create(obj,"myFile"));
            debuggerVar.Name.Should().Be("myFile");
            debuggerVar.TypeName.Should().Be("Файл");
            debuggerVar.Presentation.Should().Be("Файл");
            debuggerVar.IsStructured.Should().BeTrue();

            var children = Visualizer.GetChildVariables(obj)
                .Select(x => Visualizer.GetVariable(x))
                .ToArray();
            
            children.Should().NotBeEmpty();
            children.All(x => x.IsStructured == false).Should().BeTrue();
            
            var childrenMap = children.ToDictionary(x => x.Name);
            childrenMap["Имя"].IsStructured.Should().BeFalse();
            childrenMap["Имя"].Presentation.Should().Be("somefile.txt");
            childrenMap["Расширение"].IsStructured.Should().BeFalse();
            childrenMap["Расширение"].Presentation.Should().Be(".txt");
        }

        [Fact]
        public void ArrayPresentation()
        {
            var obj = GetInstance<ArrayImpl>();
            obj.Add(ValueFactory.Create(1));
            obj.Add(ValueFactory.Create(2));

            var debuggerVar = Visualizer.GetVariable(Contexts.Variable.Create(obj, "myArray"));
            Assert.Equal("Массив", debuggerVar.Presentation);
            Assert.True(debuggerVar.IsStructured);

            var items = Visualizer.GetChildVariables(obj).ToArray();
            items.Should().HaveCount(2);
            Assert.Equal("0", items[0].Name);
            Assert.Equal("1", items[1].Name);
        }
        
        [Fact]
        public void StructurePresentation()
        {
            Manager.RegisterClass(typeof(KeyAndValueImpl));
            
            var obj = GetInstance<StructureImpl>();
            obj.Insert("first", ValueFactory.Create(1));
            obj.Insert("second", ValueFactory.Create(2));

            var debuggerVar = Visualizer.GetVariable(Contexts.Variable.Create(obj, "myVar"));
            Assert.Equal("Структура", debuggerVar.Presentation);
            Assert.True(debuggerVar.IsStructured);

            var items = Visualizer
                .GetChildVariables(obj)
                .Select(x => Visualizer.GetVariable(x))
                .ToArray();
            
            items.Should().HaveCount(2);
            Assert.Equal("first", items[0].Name);
            Assert.Equal("Число", items[0].TypeName);
            Assert.Equal("1", items[0].Presentation);
            Assert.False(items[0].IsStructured);
            
            Assert.Equal("second", items[1].Name);
            Assert.Equal("Число", items[1].TypeName);
            Assert.Equal("2", items[1].Presentation);
            Assert.False(items[1].IsStructured);
        }
        
        [Fact]
        public void MapPresentation()
        {
            Manager.RegisterClass(typeof(KeyAndValueImpl));
            
            var obj = GetInstance<MapImpl>();
            obj.Insert(ValueFactory.Create("first"), ValueFactory.Create(1));
            obj.Insert(ValueFactory.Create("second"), ValueFactory.Create(2));

            var debuggerVar = Visualizer.GetVariable(Contexts.Variable.Create(obj, "myVar"));
            Assert.Equal("Соответствие", debuggerVar.Presentation);
            Assert.True(debuggerVar.IsStructured);

            var items = Visualizer.GetChildVariables(obj)
                .Select(x => Visualizer.GetVariable(x))
                .ToArray();
            
            items.Should().HaveCount(2);
            
            Assert.Equal("0", items[0].Name);
            Assert.Equal("КлючИЗначение", items[0].TypeName);
            Assert.Equal("КлючИЗначение", items[0].Presentation);
            Assert.True(items[0].IsStructured);
            
            Assert.Equal("1", items[1].Name);
            Assert.Equal("КлючИЗначение", items[1].TypeName);
            Assert.Equal("КлючИЗначение", items[1].Presentation);
            Assert.True(items[1].IsStructured);

            var keyValue = Visualizer.GetChildVariables(obj.First())
                .Select(x => Visualizer.GetVariable(x))
                .ToArray();
            
            Assert.Equal("Ключ", keyValue[0].Name);
            Assert.Equal("first", keyValue[0].Presentation);
            Assert.Equal("Значение", keyValue[1].Name);
            Assert.Equal("1", keyValue[1].Presentation);
        }
        
        [Fact]
        public void ValueTreePresentation()
        {
            Manager.RegisterClass(typeof(ValueTreeRowCollection));
            Manager.RegisterClass(typeof(ValueTreeRow));
            Manager.RegisterClass(typeof(ValueTreeColumn));
            Manager.RegisterClass(typeof(ValueTreeColumnCollection));
            
            var obj = GetInstance<ValueTree>();
            obj.Columns.Add("first");
            obj.Columns.Add("second");

            var row = obj.Rows.Add();
            row.Set(0, ValueFactory.Create("val1"));
            row.Set(1, ValueFactory.Create("val2"));

            var variables = Visualizer.GetChildVariables(obj)
                .Select(x => Visualizer.GetVariable(x))
                .ToDictionary(x => x.Name);

            variables.Should().HaveCount(2);
            
            Assert.True(variables["Строки"].IsStructured);
            Assert.True(variables["Колонки"].IsStructured);

            var rows = Visualizer.GetChildVariables(obj.Rows)
                .Select(x => Visualizer.GetVariable(x))
                .ToArray();

            rows.Should().HaveCount(2);
            Assert.Equal("Родитель", rows[0].Name);
            Assert.False(rows[0].IsStructured);
            Assert.Equal("0", rows[1].Name);
            Assert.Equal("СтрокаДереваЗначений", rows[1].TypeName);
            Assert.True(rows[1].IsStructured);

            var rowData = Visualizer.GetChildVariables(row)
                .Select(x => Visualizer.GetVariable(x))
                .ToArray();

            rowData.Should().HaveCount(4);
            Assert.Equal("Родитель", rowData[0].Name);
            Assert.Equal("Строки", rowData[1].Name);
            Assert.Equal("first", rowData[2].Name);
            Assert.Equal("val1", rowData[2].Presentation);
            Assert.Equal("second", rowData[3].Name);
            Assert.Equal("val2", rowData[3].Presentation);
        }
    }
}