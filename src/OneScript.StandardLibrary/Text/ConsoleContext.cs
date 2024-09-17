﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    /// <summary>
    /// Класс представляет собой инструмент доступа к системной консоли.
    /// Предназначен для низкоуровнего манипулирования выводом в консоль.
    /// </summary>
    [ContextClass("Консоль", "Console")]
    public class ConsoleContext : AutoContext<ConsoleContext>
    {
        [ContextProperty("НажатаКлавиша", "KeyPressed")]
        public bool HasKey => Console.KeyAvailable;

        [ContextProperty("КурсорЛево", "CursorLeft")]
        public int XPos
        {
            get => Console.CursorLeft;
            set => Console.CursorLeft = Math.Min(value, Console.WindowWidth-1);
        }

        [ContextProperty("КурсорВерх", "CursorTop")]
        public int YPos
        {
            get => Console.CursorTop;
            set => Console.CursorTop = Math.Min(value, Console.WindowHeight-1);
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
        public int Width => Console.WindowWidth;

        [ContextProperty("Высота", "Height")]
        public int Height => Console.WindowHeight;

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
                try
                {
                    return ConsoleColorEnum.Instance.FromNativeValue(Console.ForegroundColor);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
            set
            {
                if (value.GetRawValue() is ClrEnumValueWrapper<ConsoleColor> typed)
                {
                    Console.ForegroundColor = typed.UnderlyingValue;
                }
                else
                    throw new TypeConversionException();
            }
        }

        [ContextProperty("ЦветФона", "BackgroundColor")]
        public IValue BackgroundColor
        {
            get
            {
                try
                {
                    return GlobalsHelper.GetEnum<ConsoleColorEnum>().FromNativeValue(Console.BackgroundColor);
                }
                catch (InvalidOperationException)
                {
                    return null;
                }
            }
            set
            {
                if (value.GetRawValue() is ClrEnumValueWrapper<ConsoleColor> typed)
                {
                    Console.BackgroundColor = typed.UnderlyingValue;
                }
                else
                    throw new TypeConversionException();
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
                var encodingEnum = GlobalsHelper.GetEnum<TextEncodingEnum>();
                return encodingEnum.GetValue(Console.InputEncoding);
            }
            set 
            {
                Console.InputEncoding = TextEncodingEnum.GetEncoding(value);                
            }
        }
        
        /// <summary>
        /// Возвращает или задает кодировку консоли, используемую при чтении входных данных.
        /// </summary>
        /// <returns>КодировкаТекста</returns>
        [ContextProperty("КодировкаВыходногоПотока", "InputEncoding")]
        public IValue OutputEncoding 
        {
            get
            {
                var encodingEnum = GlobalsHelper.GetEnum<TextEncodingEnum>();
                return encodingEnum.GetValue(Console.OutputEncoding);
            }
            set 
            {
                Console.OutputEncoding = TextEncodingEnum.GetEncoding(value);                
            }
        }

        /// <summary>
        /// Воспроизводит звуковой сигнал.
        /// </summary>
        [ContextMethod("Сигнал")]
        public void Beep()
        {
            Console.Beep();
        }

        /// <summary>
        /// Получает системный поток ввода stdin
        /// </summary>
        /// <returns>Поток</returns>
        [ContextMethod("ОткрытьСтандартныйПотокВвода", "OpenStandardInput")]
        public GenericStream OpenStandardInput()
        {
            var stream = Console.OpenStandardInput();
            var streamWithTimeout = new StreamWithTimeout(stream);
            return new GenericStream(streamWithTimeout, true);
        }
        
        /// <summary>
        /// Получает системный поток вывода ошибок stderr
        /// </summary>
        /// <returns>Поток</returns>
        [ContextMethod("ОткрытьСтандартныйПотокОшибок", "OpenStandardError")]
        public GenericStream OpenStandardError()
        {
            var stream = Console.OpenStandardError();
            return new GenericStream(stream);
        }
        
        /// <summary>
        /// Получает системный поток вывода stdout
        /// </summary>
        /// <returns>Поток</returns>
        [ContextMethod("ОткрытьСтандартныйПотокВывода", "OpenStandardOutput")]
        public GenericStream OpenStandardOutput()
        {
            var stream = Console.OpenStandardOutput();
            return new GenericStream(stream);
        }

        /// <summary>
        /// Глобально переопределяет стандартный вывод и направляет в другой поток
        /// </summary>
        /// <param name="target">Поток назначения</param>
        [ContextMethod("УстановитьПотокВывода", "SetOutput")]
        public void SetOutput(IValue target)
        {
            if (!(target.AsObject() is IStreamWrapper stream))
                throw RuntimeException.InvalidArgumentType(nameof(target));
            
            var writer = new StreamWriter(stream.GetUnderlyingStream(), Console.OutputEncoding)
            {
                AutoFlush = true,
            };
            Console.SetOut(writer);
        }
        
        /// <summary>
        /// Глобально переопределяет стандартный поток ошибок и направляет в другой поток
        /// </summary>
        /// <param name="target">Поток назначения</param>
        [ContextMethod("УстановитьПотокОшибок", "SetError")]
        public void SetError(IValue target)
        {
            if (!(target.AsObject() is IStreamWrapper stream))
                throw RuntimeException.InvalidArgumentType(nameof(target));
            
            var writer = new StreamWriter(stream.GetUnderlyingStream());
            Console.SetError(writer);
        }
    }
}
