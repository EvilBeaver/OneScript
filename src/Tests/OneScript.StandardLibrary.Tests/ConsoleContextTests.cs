/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Contexts;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Text;
using OneScript.Values;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    public class ConsoleContextTests
    {
        private static IValue GetProperty(StructureImpl structure, string name) =>
            structure.GetPropValue(structure.GetPropertyNumber(name));

        private static int GetNumber(StructureImpl structure, string name) =>
            (BslNumericValue)GetProperty(structure, name);

        private static string GetString(StructureImpl structure, string name) =>
            (BslStringValue)GetProperty(structure, name);

        private static bool GetBoolean(StructureImpl structure, string name) =>
            (bool)(BslBooleanValue)GetProperty(structure, name);

        private static StructureImpl GetNestedStructure(StructureImpl structure, string name) =>
            (StructureImpl)GetProperty(structure, name);

        [Fact]
        public void ReadKeyPress_Is_Exposed_With_Bilingual_Name()
        {
            var method = typeof(ConsoleContext).GetMethod(nameof(ConsoleContext.ReadKeyPress));

            Assert.NotNull(method);

            var attribute = method!.GetCustomAttribute<ContextMethodAttribute>();
            Assert.NotNull(attribute);
            Assert.Equal("ПрочитатьНажатие", attribute!.Name);
            Assert.Equal("ReadKeyPress", attribute.Alias);
        }

        [Fact]
        public void KeyPressInfo_Results_Have_Expected_Structure()
        {
            var keyPress = ConsoleContext.CreateKeyPressInfo(
                new ConsoleKeyInfo('ф', (ConsoleKey)0, false, false, false));

            Assert.Equal(3, keyPress.GetPropCount());
            Assert.Equal("КодКнопки", keyPress.GetPropName(0));
            Assert.Equal("Символ", keyPress.GetPropName(1));
            Assert.Equal("Модификаторы", keyPress.GetPropName(2));

            var modifiers = GetNestedStructure(keyPress, "Модификаторы");
            Assert.Equal(3, modifiers.GetPropCount());
            Assert.Equal("Shift", modifiers.GetPropName(0));
            Assert.Equal("Alt", modifiers.GetPropName(1));
            Assert.Equal("Control", modifiers.GetPropName(2));
        }

        [Fact]
        public void KeyPressInfo_Returns_Cyrillic_Symbol()
        {
            var keyPress = ConsoleContext.CreateKeyPressInfo(
                new ConsoleKeyInfo('ф', (ConsoleKey)0, false, false, false));

            Assert.Equal("ф", GetString(keyPress, "Символ"));
            Assert.Equal(0, GetNumber(keyPress, "КодКнопки"));
        }

        [Fact]
        public void KeyPressInfo_Returns_Upper_Case_Cyrillic_Symbol_With_Shift()
        {
            var keyPress = ConsoleContext.CreateKeyPressInfo(
                new ConsoleKeyInfo('Ф', (ConsoleKey)0, shift: true, alt: true, control: false));

            Assert.Equal("Ф", GetString(keyPress, "Символ"));

            var modifiers = GetNestedStructure(keyPress, "Модификаторы");
            Assert.True(GetBoolean(modifiers, "Shift"));
            Assert.True(GetBoolean(modifiers, "Alt"));
            Assert.False(GetBoolean(modifiers, "Control"));
        }

        [Fact]
        public void KeyPressInfo_Returns_Empty_Symbol_For_Control_Key_Combination()
        {
            // Ctrl+A: .NET сообщает управляющий символ и код клавиши A
            var keyPress = ConsoleContext.CreateKeyPressInfo(
                new ConsoleKeyInfo('\u0001', ConsoleKey.A, shift: false, alt: false, control: true));

            Assert.Equal(string.Empty, GetString(keyPress, "Символ"));
            Assert.Equal((int)ConsoleKey.A, GetNumber(keyPress, "КодКнопки"));

            var modifiers = GetNestedStructure(keyPress, "Модификаторы");
            Assert.True(GetBoolean(modifiers, "Control"));
            Assert.False(GetBoolean(modifiers, "Shift"));
            Assert.False(GetBoolean(modifiers, "Alt"));
        }

        [Fact]
        public void KeyPressInfo_Returns_Empty_Symbol_For_Function_Key()
        {
            var keyPress = ConsoleContext.CreateKeyPressInfo(
                new ConsoleKeyInfo('\0', ConsoleKey.F1, false, false, false));

            Assert.Equal(string.Empty, GetString(keyPress, "Символ"));
            Assert.Equal((int)ConsoleKey.F1, GetNumber(keyPress, "КодКнопки"));
        }

        [Fact]
        public void KeyPressInfo_Keeps_Space_As_Symbol()
        {
            var keyPress = ConsoleContext.CreateKeyPressInfo(
                new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

            Assert.Equal(" ", GetString(keyPress, "Символ"));
        }
    }
}
