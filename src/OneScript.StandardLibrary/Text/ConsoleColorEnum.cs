/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    [SystemEnum("ЦветКонсоли", "ConsoleColor")]
    public class ConsoleColorEnum : ClrEnumWrapper<ConsoleColor>
    {
        private ConsoleColorEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        public static ConsoleColorEnum CreateInstance(ITypeManager typeManager)
        {
            var instance = EnumContextHelper.CreateClrEnumInstance<ConsoleColorEnum, ConsoleColor>(
                typeManager,
                (t,v) => new ConsoleColorEnum(t,v));
            
            instance.WrapClrValue("Черный", "Black", ConsoleColor.Black);
            instance.WrapClrValue("ТемноСиний", "DarkBlue", ConsoleColor.DarkBlue);
            instance.WrapClrValue("ТемноЗеленый", "DarkGreen", ConsoleColor.DarkGreen);
            instance.WrapClrValue("ТемноБирюзовый", "DarkCyan", ConsoleColor.DarkCyan);
            instance.WrapClrValue("ТемноКрасный", "DarkRed", ConsoleColor.DarkRed);
            instance.WrapClrValue("ТемноМалиновый", "DarkMagenta", ConsoleColor.DarkMagenta);
            instance.WrapClrValue("ТемноЖелтый", "DarkYellow", ConsoleColor.DarkYellow);
            instance.WrapClrValue("Серый", "Gray", ConsoleColor.Gray);
            
            instance.WrapClrValue("ТемноСерый", "DarkGray", ConsoleColor.DarkGray);
            instance.WrapClrValue("Синий", "Blue", ConsoleColor.Blue);
            instance.WrapClrValue("Зеленый", "Green", ConsoleColor.Green);
            instance.WrapClrValue("Бирюза", "Cyan", ConsoleColor.Cyan);
            instance.WrapClrValue("Красный", "Red", ConsoleColor.Red);
            instance.WrapClrValue("Малиновый", "Magenta", ConsoleColor.Magenta);
            instance.WrapClrValue("Желтый", "Yellow", ConsoleColor.Yellow);
            instance.WrapClrValue("Белый", "White", ConsoleColor.White);

            OnInstanceCreation(instance);
            
            return instance;
        }
    }
}
