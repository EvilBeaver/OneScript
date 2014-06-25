using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("Консоль")]
    class ConsoleContext : AutoContext<ConsoleContext>
    {
        [ContextProperty("НажатаКлавиша")]
        public bool HasKey
        {
            get
            {
                return Console.KeyAvailable;
            }
        }

        [ContextProperty("КурсорЛево")]
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

        [ContextProperty("КурсорВерх")]
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

        [ContextMethod("ПрочитатьСтроку")]
        public string ReadLine()
        {
            return Console.ReadLine();
        }

        [ContextMethod("Прочитать")]
        public int ReadKey()
        {
            var kki = Console.ReadKey(true);
            return (int)kki.Key;
        }

        [ContextMethod("Очистить")]
        public void Clear()
        {
            Console.Clear();
        }

        [ContextMethod("ВывестиСтроку")]
        public void WriteLine(string text)
        {
            Console.WriteLine(text);
        }

        [ContextMethod("Вывести")]
        public void Write(string text)
        {
            Console.Write(text);
        }

        [ContextProperty("Ширина")]
        public int Width
        {
            get
            {
                return Console.WindowWidth;
            }
        }

        [ContextProperty("Высота")]
        public int Высота
        {
            get
            {
                return Console.WindowHeight;
            }
        }

        [ContextMethod("ВидимостьКурсора")]
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
