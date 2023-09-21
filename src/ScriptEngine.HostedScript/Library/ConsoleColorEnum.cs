﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ScriptEngine.HostedScript.Library
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

        public static ConsoleColorEnum CreateInstance()
        {
            ConsoleColorEnum instance;
            var type = TypeManager.RegisterType("ПеречислениеЦветКонсоли", typeof(ConsoleColorEnum));
            var enumValueType = TypeManager.RegisterType("ЦветКонсоли", typeof(CLREnumValueWrapper<ConsoleColor>));

            instance = new ConsoleColorEnum(type, enumValueType);

            instance.AddValue("Черный", "Black", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Black));
            instance.AddValue("ТемноСиний", "DarkBlue", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.DarkBlue));
            instance.AddValue("ТемноЗеленый", "DarkGreen", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.DarkGreen));
            instance.AddValue("ТемноБирюзовый", "DarkCyan", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.DarkCyan));
            instance.AddValue("ТемноКрасный", "DarkRed", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.DarkRed));
            instance.AddValue("ТемноМалиновый", "DarkMagenta", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.DarkMagenta));
            instance.AddValue("ТемноЖелтый", "DarkYellow", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.DarkYellow));
            instance.AddValue("Серый", "Gray", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Gray));

            instance.AddValue("Серый", "DarkGray", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.DarkGray));
            instance.AddValue("Синий", "Blue", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Blue));
            instance.AddValue("Зеленый", "Green", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Green));
            instance.AddValue("Бирюза", "Cyan", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Cyan));
            instance.AddValue("Красный", "Red", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Red));
            instance.AddValue("Малиновый", "Magenta", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Magenta));
            instance.AddValue("Желтый", "Yellow", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Yellow));
            instance.AddValue("Белый", "White", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.White));

            return instance;
        }
    }
}
