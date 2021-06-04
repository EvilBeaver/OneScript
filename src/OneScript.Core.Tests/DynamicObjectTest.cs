/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary.Collections;
using Xunit;

namespace OneScript.Core.Tests
{
    public class DynamicObjectTest
    {
        [Fact]
        public void CanCallMethodsOfStruct()
        {
            var structImpl = new StructureImpl();
            dynamic dyn = structImpl;

            dyn.Insert("Свойство", 1);
            dyn.Вставить("Свойство2", "Привет");
            
            Assert.Equal(2, structImpl.Count());
            Assert.True(structImpl.HasProperty("Свойство"));
            Assert.True(structImpl.HasProperty("Свойство2"));
        }
        
        [Fact]
        public void CanAccessPropertiesOfStruct()
        {
            var structImpl = new StructureImpl();
            dynamic dyn = structImpl;

            dyn.Вставить("Свойство", 1);
            dyn.Вставить("Свойство2", "Привет");
            
            Assert.Equal(1, dyn.Свойство);
            Assert.Equal("Привет",dyn.Свойство2);
        }
        
        [Fact]
        public void CanAccessIndexOfStruct()
        {
            var structImpl = new StructureImpl();
            dynamic dyn = structImpl;

            dyn.Вставить("Свойство", 1);
            dyn.Вставить("Свойство2", "Привет");
            
            Assert.Equal(1, dyn["Свойство"]);
            Assert.Equal("Привет",dyn["Свойство2"]);
        }
        
        [Fact]
        public void CanAccessArraysByIndex()
        {
            var arr = new ArrayImpl();
            dynamic Массив = new ArrayImpl();

            Массив.Добавить(1);
            Массив.Добавить(2);
            Массив.Добавить("Привет");
            
            Assert.Equal(3, Массив.Количество());
            Assert.Equal(1, Массив[0]);
            Assert.Equal(2, Массив[1]);
            Assert.Equal("Привет", Массив[2]);
        }
                
    }
}