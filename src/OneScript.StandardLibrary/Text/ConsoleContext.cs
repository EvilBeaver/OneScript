/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Collections;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    /// <summary>
    /// Класс представляет собой инструмент доступа к системной консоли.
    /// Предназначен для низкоуровневого манипулирования выводом в консоль.
    ///
    /// Поддерживается регистрация обработчика для нажатия Ctrl+C.
    /// Обработчик регистрируется для события с именем CancelKeyPressed. Стоит учитывать, что обработчик вызывается
    /// в отдельном потоке и необходимо помнить о потокобезопасности общих данных.
    /// 
    /// </summary>
    /// <example>
    ///    Перем Хватит;
    /// 
    ///    Процедура Обработчик(Отказ)
    ///        Сообщить("Обработчик остановки вызван", СтатусСообщения.Важное);
    ///        Отказ = Истина; // не даем системе убить процесс
    ///        Хватит = Истина; // выставим свой флаг завершения цикла
    ///    КонецПроцедуры
    ///
    ///    Хватит = Ложь;
    ///    ДобавитьОбработчик Консоль.CancelKeyPressed, Обработчик;
    ///
    ///    Пока Не Хватит Цикл
    ///        Сообщить("Пинг", СтатусСообщения.Информация);
    ///        Приостановить(1000);
    ///        Сообщить("Понг", СтатусСообщения.Информация);
    ///        Приостановить(1000);
    ///    КонецЦикла;
    ///
    ///    Сообщить("Мягкая остановка процесса", СтатусСообщения.Информация);
    /// </example>
    [ContextClass("Консоль", "Console")]
    public class ConsoleContext : AutoContext<ConsoleContext>
    {
        private readonly ExecutionContext _executionContext;

        public ConsoleContext(ExecutionContext executionContext)
        {
            _executionContext = executionContext;
            Console.CancelKeyPress += ConsoleOnCancelKeyPress;
        }

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

        /// <summary>
        /// Читает нажатие клавиши и возвращает подробное описание нажатия.
        /// В отличие от <see cref="ReadKey"/>, возвращает не только код клавиши, но и введённый символ
        /// (в том числе кириллический), а также состояние модификаторов.
        /// Символ не выводится в консоль (чтение выполняется без эха).
        /// </summary>
        /// <returns>
        /// Структура с полями:
        /// <list type="bullet">
        /// <item><description><c>КодКнопки</c> — код клавиши (значение перечисления <see cref="ConsoleKey"/>),
        /// совпадает с результатом <see cref="ReadKey"/>;</description></item>
        /// <item><description><c>Символ</c> — введённый символ; пустая строка для служебных клавиш
        /// и управляющих сочетаний;</description></item>
        /// <item><description><c>Модификаторы</c> — структура с булевыми полями <c>Shift</c>, <c>Alt</c>, <c>Control</c>.</description></item>
        /// </list>
        /// </returns>
        /// <remarks>
        /// Возможности платформы различаются:
        /// <list type="bullet">
        /// <item><description>в Windows код клавиши и состояние модификаторов определяются для любых нажатий;</description></item>
        /// <item><description>в Unix-подобных системах .NET распознаёт клавиши и модификаторы только для известных
        /// символов и управляющих последовательностей. Для символов за пределами ASCII (в том числе кириллицы)
        /// <c>КодКнопки</c> может быть равен 0, а <c>Shift</c> для заглавной кириллической буквы может не определиться
        /// (при этом сам символ всегда возвращается в поле <c>Символ</c>).</description></item>
        /// </list>
        /// </remarks>
        [ContextMethod("ПрочитатьНажатие", "ReadKeyPress")]
        public StructureImpl ReadKeyPress()
        {
            return CreateKeyPressInfo(Console.ReadKey(true));
        }

        /// <summary>
        /// Преобразует нажатие, полученное от <see cref="Console.ReadKey(bool)"/>,
        /// в структуру, доступную из BSL.
        /// </summary>
        internal static StructureImpl CreateKeyPressInfo(ConsoleKeyInfo keyInfo)
        {
            var modifiers = new StructureImpl(
                "Shift, Alt, Control",
                ValueFactory.Create((keyInfo.Modifiers & ConsoleModifiers.Shift) != 0),
                ValueFactory.Create((keyInfo.Modifiers & ConsoleModifiers.Alt) != 0),
                ValueFactory.Create((keyInfo.Modifiers & ConsoleModifiers.Control) != 0));

            var result = new StructureImpl();
            result.Insert("КодКнопки", ValueFactory.Create((int)keyInfo.Key));
            result.Insert("Символ", ValueFactory.Create(GetPrintableSymbol(keyInfo.KeyChar)));
            result.Insert("Модификаторы", modifiers);

            return result;
        }

        private static string GetPrintableSymbol(char keyChar)
        {
            return char.IsControl(keyChar) ? string.Empty : keyChar.ToString();
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
        public ClrEnumValueWrapper<ConsoleColor> TextColor
        {
            get =>
                ConsoleColorEnum.Instance.TryGetFromNativeValue(Console.ForegroundColor) 
                ?? ConsoleColorEnum.Instance.FromNativeValue(ConsoleColor.Gray);
            
            set => Console.ForegroundColor = value.UnderlyingValue;
        }

        [ContextProperty("ЦветФона", "BackgroundColor")]
        public ClrEnumValueWrapper<ConsoleColor> BackgroundColor
        {
            get =>
                ConsoleColorEnum.Instance.TryGetFromNativeValue(Console.BackgroundColor) 
                ?? ConsoleColorEnum.Instance.FromNativeValue(ConsoleColor.Black);
            
            set => Console.BackgroundColor = value.UnderlyingValue;
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
                var encodingEnum = _executionContext.GlobalInstances.GetInstance<TextEncodingEnum>();
                return encodingEnum.GetValue(Console.InputEncoding);
            }
            set 
            {
                Console.InputEncoding = TextEncodingEnum.GetEncoding(value);                
            }
        }

        /// <summary>
        /// Сбрасывает цвета консоли к их исходному состоянию
        /// </summary>
        [ContextMethod("СброситьЦвет", "ResetColor")]
        public void ResetColor()
        {
            Console.ResetColor();
        }
        
        /// <summary>
        /// Возвращает или задает кодировку консоли, используемую при выводе данных.
        /// </summary>
        /// <returns>КодировкаТекста</returns>
        [ContextProperty("КодировкаВыходногоПотока", "OutputEncoding")]
        public IValue OutputEncoding 
        {
            get
            {
                var encodingEnum = _executionContext.GlobalInstances.GetInstance<TextEncodingEnum>();
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
        [ContextMethod("Сигнал", "Beep")]
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

        public const string ConsoleCancelKeyEvent = "CancelKeyPressed";

        private void ConsoleOnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            if (e.SpecialKey != ConsoleSpecialKey.ControlC) 
                return;
            
            // Сначала проверим, что события вообще включены
            var eventProcessor = _executionContext.Services.TryResolve<IEventProcessor>();
            if (eventProcessor == null)
                return;

            var process = _executionContext.Services.Resolve<IBslProcessFactory>().NewProcess();
            
            var cancelVar = Variable.Create(BslBooleanValue.False, "Cancel");
            var reference = Variable.CreateReference(cancelVar, "Cancel");
            var args = new IValue[] { reference };

            eventProcessor.HandleEvent(this, ConsoleCancelKeyEvent, args, process);
            e.Cancel = reference.Value.AsBoolean();
        }
    }
}
