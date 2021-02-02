/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Text
{
    [SystemEnum("ЦветКонсоли", "ConsoleColor")]
    public class ConsoleColorEnum : EnumerationContext
    {
        readonly Dictionary<ConsoleColor, IValue> _valuesCache = new Dictionary<ConsoleColor, IValue>();

        private ConsoleColorEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {

        }

        public IValue FromNativeValue(ConsoleColor native)
        {
            IValue val;
            if (_valuesCache.TryGetValue(native, out val))
            {
                return val;
            }
            else
            {
                val = ValuesInternal.First(x => ((CLREnumValueWrapper<ConsoleColor>)x).UnderlyingValue == native);
                _valuesCache.Add(native, val);
            }

            return val;
        }

        public static ConsoleColorEnum CreateInstance(ITypeManager typeManager)
        {
            var instance = EnumContextHelper.CreateClrEnumInstance<ConsoleColorEnum, ConsoleColor>(
                typeManager,
                (t,v) => new ConsoleColorEnum(t,v));
            
            instance.WrapClrValue("Белый", "White", ConsoleColor.White);
            instance.WrapClrValue("Черный", "Black", ConsoleColor.Black);
            instance.WrapClrValue("Синий", "Blue", ConsoleColor.Blue);
            instance.WrapClrValue("Желтый", "Yellow", ConsoleColor.Yellow);
            instance.WrapClrValue("Красный", "Red", ConsoleColor.Red);
            instance.WrapClrValue("Зеленый", "Green", ConsoleColor.Green);
            instance.WrapClrValue("Бирюза", "Cyan", ConsoleColor.Cyan);
            instance.WrapClrValue("Малиновый", "Magenta", ConsoleColor.Magenta);
            instance.WrapClrValue("Серый", "Gray", ConsoleColor.Gray);

            return instance;
        }
    }
}
