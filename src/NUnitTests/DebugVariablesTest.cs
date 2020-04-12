/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using OneScript.DebugServices;
using ScriptEngine;
using ScriptEngine.HostedScript.Library;
using ScriptEngine.HostedScript.Library.ValueTree;
using ScriptEngine.Machine;

namespace NUnitTests
{
    // TODO стандартный визуализатор ничего не знает про особенности коллекций
    // После завершения ветки breaking-refactory визуализатор, возможно переедет в другую сборку
    // пока оставляю так, что IVariableVisualizer живет в DebugServices
    [TestFixture]
    public class DebugVariablesTest
    {
        [OneTimeSetUp]
        public void GlobalSetup()
        {
            // для инициализации долбаного глобального TypeManager
            var e = new ScriptingEngine();
            e.AttachAssembly(typeof(FileContext).Assembly);
        }

        [SetUp]
        public void Init()
        {
            Visualizer = new DefaultVariableVisualizer();
        }
        
        public IVariableVisualizer Visualizer { get; set; }
        
        [Test]
        public void SimpleValuePresentation()
        {
            var str = ValueFactory.Create("string value");
            
            var debuggerVar = Visualizer.GetVariable(Variable.Create(str,"myString"));
            Assert.That(debuggerVar.Name, Is.EqualTo("myString"));
            Assert.That(debuggerVar.TypeName, Is.EqualTo("Строка"));
            Assert.That(debuggerVar.Presentation, Is.EqualTo("string value"));
            Assert.That(debuggerVar.IsStructured, Is.False);
            
            var number = ValueFactory.Create(27.2m);
            debuggerVar = Visualizer.GetVariable(Variable.Create(number,"myInt"));
            Assert.That(debuggerVar.Name, Is.EqualTo("myInt"));
            Assert.That(debuggerVar.TypeName, Is.EqualTo("Число"));
            Assert.That(debuggerVar.Presentation, Is.EqualTo("27.2"));
            Assert.That(debuggerVar.IsStructured, Is.False);
        }
        
        [Test]
        public void ObjectPresentation()
        {
            var obj = new FileContext("somefile.txt");
            
            var debuggerVar = Visualizer.GetVariable(Variable.Create(obj,"myFile"));
            Assert.That(debuggerVar.Name, Is.EqualTo("myFile"));
            Assert.That(debuggerVar.TypeName, Is.EqualTo("Файл"));
            Assert.That(debuggerVar.Presentation, Is.EqualTo("Файл"));
            Assert.That(debuggerVar.IsStructured, Is.True);

            var children = Visualizer.GetChildVariables(obj);
            Assert.That(children, Is.Not.Empty);
            Assert.That(children.All(x => x.IsStructured == false), Is.True);

            var childrenMap = children.ToDictionary(x => x.Name);
            Assert.That(childrenMap["Имя"].IsStructured, Is.False);
            Assert.That(childrenMap["Имя"].Presentation, Is.EqualTo("somefile.txt"));
            Assert.That(childrenMap["Расширение"].IsStructured, Is.False);
            Assert.That(childrenMap["Расширение"].Presentation, Is.EqualTo(".txt"));
        }

        [Test]
        public void ArrayPresentation()
        {
            var obj = new ArrayImpl();
            obj.Add(ValueFactory.Create(1));
            obj.Add(ValueFactory.Create(2));

            var debuggerVar = Visualizer.GetVariable(Variable.Create(obj, "myArray"));
            Assert.That(debuggerVar.Presentation, Is.EqualTo("Массив"));
            Assert.That(debuggerVar.IsStructured, Is.True);

            var items = Visualizer.GetChildVariables(obj).ToArray();
            Assert.That(items, Has.Length.EqualTo(2));
            Assert.That(items[0].Name, Is.EqualTo("0"));
            Assert.That(items[1].Name, Is.EqualTo("1"));
        }
        
        [Test]
        public void StructurePresentation()
        {
            var obj = new StructureImpl();
            obj.Insert("first", ValueFactory.Create(1));
            obj.Insert("second", ValueFactory.Create(2));

            var debuggerVar = Visualizer.GetVariable(Variable.Create(obj, "myVar"));
            Assert.That(debuggerVar.Presentation, Is.EqualTo("Структура"));
            Assert.That(debuggerVar.IsStructured, Is.True);

            var items = Visualizer.GetChildVariables(obj).ToArray();
            Assert.That(items, Has.Length.EqualTo(2));
            Assert.That(items[0].Name, Is.EqualTo("first"));
            Assert.That(items[0].TypeName, Is.EqualTo("Число"));
            Assert.That(items[0].Presentation, Is.EqualTo("1"));
            Assert.That(items[0].IsStructured, Is.False);
            
            Assert.That(items[1].Name, Is.EqualTo("second"));
            Assert.That(items[1].TypeName, Is.EqualTo("Число"));
            Assert.That(items[1].Presentation, Is.EqualTo("2"));
            Assert.That(items[1].IsStructured, Is.False);
        }
        
        [Test]
        public void MapPresentation()
        {
            var obj = new MapImpl();
            obj.Insert(ValueFactory.Create("first"), ValueFactory.Create(1));
            obj.Insert(ValueFactory.Create("second"), ValueFactory.Create(2));

            var debuggerVar = Visualizer.GetVariable(Variable.Create(obj, "myVar"));
            Assert.That(debuggerVar.Presentation, Is.EqualTo("Соответствие"));
            Assert.That(debuggerVar.IsStructured, Is.True);

            var items = Visualizer.GetChildVariables(obj).ToArray();
            Assert.That(items, Has.Length.EqualTo(2));
            Assert.That(items[0].Name, Is.EqualTo("0"));
            Assert.That(items[0].TypeName, Is.EqualTo("КлючИЗначение"));
            Assert.That(items[0].Presentation, Is.EqualTo("КлючИЗначение"));
            Assert.That(items[0].IsStructured, Is.True);
            
            Assert.That(items[1].Name, Is.EqualTo("1"));
            Assert.That(items[1].TypeName, Is.EqualTo("КлючИЗначение"));
            Assert.That(items[1].Presentation, Is.EqualTo("КлючИЗначение"));
            Assert.That(items[1].IsStructured, Is.True);

            var keyValue = Visualizer.GetChildVariables(obj.First()).ToArray();
            
            Assert.That(keyValue[0].Name, Is.EqualTo("Ключ"));
            Assert.That(keyValue[0].Presentation, Is.EqualTo("first"));
            Assert.That(keyValue[1].Name, Is.EqualTo("Значение"));
            Assert.That(keyValue[1].Presentation, Is.EqualTo("1"));
        }
        
        [Test]
        public void ValueTreePresentation()
        {
            var obj = new ValueTree();
            obj.Columns.Add("first");
            obj.Columns.Add("second");

            var row = obj.Rows.Add();
            row.Set(0, ValueFactory.Create("val1"));
            row.Set(1, ValueFactory.Create("val2"));

            var variables = Visualizer.GetChildVariables(obj)
                .ToDictionary(x => x.Name);
            
            Assert.That(variables, Has.Count.EqualTo(2));
            Assert.That(variables["Строки"].IsStructured);
            Assert.That(variables["Колонки"].IsStructured);

            var rows = Visualizer.GetChildVariables(obj.Rows)
                .ToArray();
            
            Assert.That(rows, Has.Length.EqualTo(2));
            Assert.That(rows[0].Name, Is.EqualTo("Родитель"));
            Assert.That(rows[0].IsStructured, Is.False);
            Assert.That(rows[1].Name, Is.EqualTo("0"));
            Assert.That(rows[1].TypeName, Is.EqualTo("СтрокаДереваЗначений"));
            Assert.That(rows[1].IsStructured);

            var rowData = Visualizer.GetChildVariables(row)
                .ToArray();
            Assert.That(rowData, Has.Length.EqualTo(4));
            Assert.That(rowData[0].Name, Is.EqualTo("Родитель"));
            Assert.That(rowData[1].Name, Is.EqualTo("Строки"));
            Assert.That(rowData[2].Name, Is.EqualTo("first"));
            Assert.That(rowData[2].Presentation, Is.EqualTo("val1"));
            Assert.That(rowData[3].Name, Is.EqualTo("second"));
            Assert.That(rowData[3].Presentation, Is.EqualTo("val2"));
        }
    }
}