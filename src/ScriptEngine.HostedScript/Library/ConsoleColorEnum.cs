/*----------------------------------------------------------
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
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [SystemEnum("ЦветКонсоли", "ConsoleColor")]
    class ConsoleColorEnum : EnumerationContext
    {
        Dictionary<ConsoleColor, IValue> _valuesCache = new Dictionary<ConsoleColor, IValue>();

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
                val = ValuesInternal.First(x => ((CLREnumValueWrapper<ConsoleColor>)x).UnderlyingObject == native);
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

            instance.AddValue("Белый", "White", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.White));
            instance.AddValue("Черный", "Black", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Black));
            instance.AddValue("Синий", "Blue", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Blue));
            instance.AddValue("Желтый", "Yellow", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Yellow));
            instance.AddValue("Красный", "Red", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Red));
            instance.AddValue("Зеленый", "Green", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Green));
            instance.AddValue("Бирюза", "Cyan", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Cyan));
            instance.AddValue("Малиновый", "Magenta", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Magenta));
            instance.AddValue("Серый", "Gray", new CLREnumValueWrapper<ConsoleColor>(instance, ConsoleColor.Gray));

            return instance;
        }
    }
}
