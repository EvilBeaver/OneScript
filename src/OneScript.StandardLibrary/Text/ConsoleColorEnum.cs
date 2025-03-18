/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts.Enums;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    [SystemEnum("ЦветКонсоли", "ConsoleColor")]
    public class ConsoleColorEnum : ClrEnumWrapper<ConsoleColor>
    {
        private ConsoleColorEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            this.WrapClrValue("Черный", "Black", ConsoleColor.Black);
            this.WrapClrValue("ТемноСиний", "DarkBlue", ConsoleColor.DarkBlue);
            this.WrapClrValue("ТемноЗеленый", "DarkGreen", ConsoleColor.DarkGreen);
            this.WrapClrValue("ТемноБирюзовый", "DarkCyan", ConsoleColor.DarkCyan);
            this.WrapClrValue("ТемноКрасный", "DarkRed", ConsoleColor.DarkRed);
            this.WrapClrValue("ТемноМалиновый", "DarkMagenta", ConsoleColor.DarkMagenta);
            this.WrapClrValue("ТемноЖелтый", "DarkYellow", ConsoleColor.DarkYellow);
            this.WrapClrValue("Серый", "Gray", ConsoleColor.Gray);
            
            this.WrapClrValue("ТемноСерый", "DarkGray", ConsoleColor.DarkGray);
            this.WrapClrValue("Синий", "Blue", ConsoleColor.Blue);
            this.WrapClrValue("Зеленый", "Green", ConsoleColor.Green);
            this.WrapClrValue("Бирюза", "Cyan", ConsoleColor.Cyan);
            this.WrapClrValue("Красный", "Red", ConsoleColor.Red);
            this.WrapClrValue("Малиновый", "Magenta", ConsoleColor.Magenta);
            this.WrapClrValue("Желтый", "Yellow", ConsoleColor.Yellow);
            this.WrapClrValue("Белый", "White", ConsoleColor.White);
        }

        public static ConsoleColorEnum CreateInstance(ITypeManager typeManager)
        {
            return CreateInstance(typeManager, (t, v) => new ConsoleColorEnum(t, v));
        }
    }
}
