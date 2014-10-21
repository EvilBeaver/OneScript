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
    class ConsoleContext : AutoContext<ConsoleContext>
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

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new ConsoleContext();
        }
    }

    
}
