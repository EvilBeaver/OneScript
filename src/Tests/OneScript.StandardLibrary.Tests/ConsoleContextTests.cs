/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Contexts;
using OneScript.StandardLibrary.Text;
using OneScript.Values;
using ScriptEngine.Machine;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    public class ConsoleContextTests
    {
        private static ConsoleKeyPress CreateKeyPress(ConsoleKeyInfo keyInfo) => new ConsoleKeyPress(keyInfo);

        private static IValue GetProperty(ConsoleKeyPress keyPress, string name) =>
            keyPress.GetPropValue(keyPress.GetPropertyNumber(name));

        private static int GetKeyCode(ConsoleKeyPress keyPress, string name) =>
            (BslNumericValue)GetProperty(keyPress, name);

        private static string GetString(ConsoleKeyPress keyPress, string name) =>
            (BslStringValue)GetProperty(keyPress, name);

        private static bool GetBoolean(ConsoleKeyPress keyPress, string name) =>
            (bool)(BslBooleanValue)GetProperty(keyPress, name);

        [Fact]
        public void ReadKeyPress_Is_Exposed_With_Bilingual_Name()
        {
            var method = typeof(ConsoleContext).GetMethod(nameof(ConsoleContext.ReadKeyPress));

            Assert.NotNull(method);
            Assert.Equal(typeof(ConsoleKeyPress), method.ReturnType);

            var attribute = method!.GetCustomAttribute<ContextMethodAttribute>();
            Assert.NotNull(attribute);
            Assert.Equal("ПрочитатьНажатие", attribute!.Name);
            Assert.Equal("ReadKeyPress", attribute.Alias);
        }

        [Fact]
        public void KeyPress_Type_Has_Bilingual_Names()
        {
            var attribute = typeof(ConsoleKeyPress).GetCustomAttribute<ContextClassAttribute>();

            Assert.NotNull(attribute);
            Assert.Equal("НажатиеКлавиши", attribute!.Name);
            Assert.Equal("KeyPress", attribute.Alias);

            var keyPress = CreateKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

            Assert.Equal("НажатиеКлавиши", keyPress.SystemType.Name);
            Assert.Equal("KeyPress", keyPress.SystemType.Alias);
        }

        [Fact]
        public void KeyPress_Exposes_Expected_Properties()
        {
            var keyPress = CreateKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

            Assert.Equal(5, keyPress.GetPropCount());
            Assert.Equal("КодКнопки", keyPress.GetPropName(0));
            Assert.Equal("Символ", keyPress.GetPropName(1));
            Assert.Equal("Shift", keyPress.GetPropName(2));
            Assert.Equal("Alt", keyPress.GetPropName(3));
            Assert.Equal("Control", keyPress.GetPropName(4));
        }

        [Fact]
        public void KeyPress_Properties_Are_ReadOnly()
        {
            var keyPress = CreateKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

            for (var i = 0; i < keyPress.GetPropCount(); i++)
            {
                Assert.True(keyPress.IsPropReadable(i));
                Assert.False(keyPress.IsPropWritable(i));
            }
        }

        [Fact]
        public void KeyPress_Properties_Are_Accessible_By_English_Names()
        {
            var keyPress = CreateKeyPress(new ConsoleKeyInfo('a', ConsoleKey.A, false, false, false));

            Assert.Equal(keyPress.GetPropertyNumber("КодКнопки"), keyPress.GetPropertyNumber("KeyCode"));
            Assert.Equal(keyPress.GetPropertyNumber("Символ"), keyPress.GetPropertyNumber("Symbol"));
            Assert.Equal(keyPress.GetPropertyNumber("Shift"), keyPress.GetPropertyNumber("Shift"));
            Assert.Equal(keyPress.GetPropertyNumber("Alt"), keyPress.GetPropertyNumber("Alt"));
            Assert.Equal(keyPress.GetPropertyNumber("Control"), keyPress.GetPropertyNumber("Control"));

            Assert.Equal((int)ConsoleKey.A, GetKeyCode(keyPress, "KeyCode"));
            Assert.Equal("a", GetString(keyPress, "Symbol"));
        }

        [Fact]
        public void KeyPress_Returns_Cyrillic_Symbol()
        {
            var keyPress = CreateKeyPress(new ConsoleKeyInfo('ф', (ConsoleKey)0, false, false, false));

            Assert.Equal("ф", GetString(keyPress, "Символ"));
            Assert.Equal(0, GetKeyCode(keyPress, "КодКнопки"));
            Assert.Equal("ф", keyPress.ToString());
        }

        [Fact]
        public void KeyPress_Returns_Upper_Case_Cyrillic_Symbol_With_Shift()
        {
            var keyPress = CreateKeyPress(
                new ConsoleKeyInfo('Ф', (ConsoleKey)0, shift: true, alt: true, control: false));

            Assert.Equal("Ф", GetString(keyPress, "Символ"));
            Assert.True(GetBoolean(keyPress, "Shift"));
            Assert.True(GetBoolean(keyPress, "Alt"));
            Assert.False(GetBoolean(keyPress, "Control"));
            Assert.Equal("Shift+Alt+Ф", keyPress.ToString());
        }

        [Fact]
        public void KeyPress_Returns_Empty_Symbol_For_Control_Key_Combination()
        {
            // Ctrl+A: .NET сообщает управляющий символ и код клавиши A
            var keyPress = CreateKeyPress(
                new ConsoleKeyInfo('\u0001', ConsoleKey.A, shift: false, alt: false, control: true));

            Assert.Equal(string.Empty, GetString(keyPress, "Символ"));
            Assert.Equal((int)ConsoleKey.A, GetKeyCode(keyPress, "КодКнопки"));
            Assert.True(GetBoolean(keyPress, "Control"));
            Assert.False(GetBoolean(keyPress, "Shift"));
            Assert.False(GetBoolean(keyPress, "Alt"));
            Assert.Equal("Control+A", keyPress.ToString());
        }

        [Fact]
        public void KeyPress_Returns_Empty_Symbol_For_Function_Key()
        {
            var keyPress = CreateKeyPress(new ConsoleKeyInfo('\0', ConsoleKey.F1, false, false, false));

            Assert.Equal(string.Empty, GetString(keyPress, "Символ"));
            Assert.Equal((int)ConsoleKey.F1, GetKeyCode(keyPress, "КодКнопки"));
            Assert.Equal("F1", keyPress.ToString());
        }

        [Fact]
        public void KeyPress_Keeps_Space_As_Symbol()
        {
            var keyPress = CreateKeyPress(new ConsoleKeyInfo(' ', ConsoleKey.Spacebar, false, false, false));

            Assert.Equal(" ", GetString(keyPress, "Символ"));
            Assert.Equal(" ", keyPress.ToString());
        }
    }
}
