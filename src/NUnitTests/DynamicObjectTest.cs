/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Dynamic;
using NUnit.Framework;
using ScriptEngine.HostedScript.Library;

namespace NUnitTests
{
    [TestFixture]
    public class DynamicObjectTest
    {
        [Test]
        public void CanCallMethodsOfStruct()
        {
            var structImpl = new StructureImpl();
            dynamic dyn = structImpl;

            dyn.Insert("Свойство", 1);
            dyn.Вставить("Свойство2", "Привет");
            
            Assert.AreEqual(2, structImpl.Count());
            Assert.True(structImpl.HasProperty("Свойство"));
            Assert.True(structImpl.HasProperty("Свойство2"));
        }
        
        [Test]
        public void CanAccessPropertiesOfStruct()
        {
            var structImpl = new StructureImpl();
            dynamic dyn = structImpl;

            dyn.Вставить("Свойство", 1);
            dyn.Вставить("Свойство2", "Привет");
            
            Assert.AreEqual(1, dyn.Свойство);
            Assert.AreEqual("Привет",dyn.Свойство2);
        }
        
        [Test]
        public void CanAccessIndexOfStruct()
        {
            var structImpl = new StructureImpl();
            dynamic dyn = structImpl;

            dyn.Вставить("Свойство", 1);
            dyn.Вставить("Свойство2", "Привет");
            
            Assert.AreEqual(1, dyn["Свойство"]);
            Assert.AreEqual("Привет",dyn["Свойство2"]);
        }
        
        [Test]
        public void CanAccessArraysByIndex()
        {
            var arr = new ArrayImpl();
            dynamic Массив = new ArrayImpl();

            Массив.Добавить(1);
            Массив.Добавить(2);
            Массив.Добавить("Привет");
            
            Assert.AreEqual(3, Массив.Количество());
            Assert.AreEqual(1, Массив[0]);
            Assert.AreEqual(2, Массив[1]);
            Assert.AreEqual("Привет", Массив[2]);
        }
                
    }
}