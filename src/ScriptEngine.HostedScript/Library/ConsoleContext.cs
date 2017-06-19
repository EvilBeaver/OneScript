/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    /// <summary>
    /// Класс представляет собой инструмент доступа к системной консоли.
    /// Предназначен для низкоуровнего манипулирования выводом в консоль.
    /// </summary>
    [ContextClass("Консоль", "Console")]
    public class ConsoleContext : AutoContext<ConsoleContext>
    {
        [ContextProperty("НажатаКлавиша", "KeyPressed")]
        public bool HasKey
        {
            get
            {
                return Console.KeyAvailable;
            }
        }

        [ContextProperty("КурсорЛево", "CursorLeft")]
        public int XPos
        {
            get
            {
                return Console.CursorLeft;
            }
            set
            {
                Console.CursorLeft = Math.Min(value, Console.WindowWidth-1);
            }
        }

        [ContextProperty("КурсорВерх", "CursorTop")]
        public int YPos
        {
            get
            {
                return Console.CursorTop;
            }
            set
            {
                Console.CursorTop = Math.Min(value, Console.WindowHeight-1);
            }
        }

        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public string ReadLine()
        {
            return Console.ReadLine();
        }

        [ContextMethod("Прочитать", "Read")]
        public int ReadKey()
        {
            var kki = Console.ReadKey(true);
            return (int)kki.Key;
        }

        [ContextMethod("Очистить", "Clear")]
        public void Clear()
        {
            Console.Clear();
        }

        [ContextMethod("ВывестиСтроку", "WriteLine")]
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        [ContextMethod("Вывести", "Write")]
        public void Write(string text)
        {
            Console.Write(text);
        }

        [ContextProperty("Ширина", "Width")]
        public int Width
        {
            get
            {
                return Console.WindowWidth;
            }
        }

        [ContextProperty("Высота", "Height")]
        public int Высота
        {
            get
            {
                return Console.WindowHeight;
            }
        }

        [ContextMethod("ВидимостьКурсора", "CursorVisible")]
        public bool CursorVisible(bool visible)
        {
            bool oldVal = Console.CursorVisible;
            Console.CursorVisible = visible;
            return oldVal;
        }
        
        [ContextProperty("ЦветТекста", "TextColor")]
        public IValue TextColor
        {
            get
            {
                return (CLREnumValueWrapper<ConsoleColor> )GlobalsManager.GetEnum<ConsoleColorEnum>().FromNativeValue(Console.ForegroundColor);
            }
            set
            {
                var typed = value.GetRawValue() as CLREnumValueWrapper<ConsoleColor>;
                Console.ForegroundColor = typed.UnderlyingValue;
            }
        }

        [ContextProperty("ЦветФона", "BackgroundColor")]
        public IValue BackgroundColor
        {
            get
            {
                return (CLREnumValueWrapper<ConsoleColor>)GlobalsManager.GetEnum<ConsoleColorEnum>().FromNativeValue(Console.BackgroundColor);
            }
            set
            {
                var typed = value.GetRawValue() as CLREnumValueWrapper<ConsoleColor>;
                Console.BackgroundColor = typed.UnderlyingValue;
            }
        }

        /// <summary>
        /// Возвращает или задает кодировку консоли, используемую при чтении входных данных.
        /// </summary>
        /// <returns>КодировкаТекста</returns>
        [ContextProperty("КодировкаВходногоПотока", "InputEncoding")]
        public IValue InputEncoding 
        {
            get
            {
                var encodingEnum = GlobalsManager.GetEnum<TextEncodingEnum>();
                return encodingEnum.GetValue(Console.InputEncoding);
            }
            set 
            {
                Console.InputEncoding = TextEncodingEnum.GetEncoding(value);                
            }
        }

        /// <summary>
        /// Воспроизводит звуковой сигнал.
        /// </summary>
        [ContextMethod("Сигнал")]
        public void Beep()
        {
            System.Media.SystemSounds.Beep.Play();
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new ConsoleContext();
        }
    }

    
}
