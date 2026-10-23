/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Text;
using System.Threading;
using ScriptEngine.Machine;

namespace OneScript.StandardLibrary.Text
{
    /// <summary>
    /// Стандартный вывод или поток ошибок консоли со своей целью у каждого потока выполнения.
    /// Цель хранится в AsyncLocal: фоновые задания наследуют ее от запустившего их кода,
    /// а переопределение внутри задания не затрагивает остальных.
    /// </summary>
    internal sealed class ConsoleWriterRouter : TextWriter
    {
        private static readonly object InstallLock = new object();
        private static ConsoleWriterRouter _out;
        private static ConsoleWriterRouter _error;

        private readonly AsyncLocal<Target> _current = new AsyncLocal<Target>();
        // Открывает стандартный поток, если вывод по умолчанию - собственный вывод консоли, а не заданный хостом
        private readonly Func<Stream> _openConsoleStream;
        private volatile TextWriter _default;
        private TextWriter _installed;

        private ConsoleWriterRouter(TextWriter defaultWriter, Func<Stream> openConsoleStream)
        {
            _default = defaultWriter;
            _openConsoleStream = openConsoleStream;
        }

        /// <summary>
        /// Направляет стандартный вывод текущего потока выполнения в writer; null возвращает вывод по умолчанию.
        /// </summary>
        public static void SetOut(TextWriter writer, IValue source)
        {
            lock (InstallLock)
            {
                // Console.Out мог заменить хост: тогда он становится выводом по умолчанию
                if (!IsInstalled(_out, Console.Out))
                {
                    if (writer == null)
                        return;

                    var isConsoleWriter = IsConsoleWriter(() => Console.Out);
                    _out = new ConsoleWriterRouter(Console.Out, isConsoleWriter ? Console.OpenStandardOutput : null);
                    Console.SetOut(_out);
                    _out._installed = Console.Out;
                }

                _out.SetCurrent(writer, source);
            }
        }

        /// <summary>
        /// Направляет поток ошибок текущего потока выполнения в writer; null возвращает поток ошибок по умолчанию.
        /// </summary>
        public static void SetError(TextWriter writer, IValue source)
        {
            lock (InstallLock)
            {
                if (!IsInstalled(_error, Console.Error))
                {
                    if (writer == null)
                        return;

                    var isConsoleWriter = IsConsoleWriter(() => Console.Error);
                    _error = new ConsoleWriterRouter(Console.Error, isConsoleWriter ? Console.OpenStandardError : null);
                    Console.SetError(_error);
                    _error._installed = Console.Error;
                }

                _error.SetCurrent(writer, source);
            }
        }

        /// <summary>
        /// Меняет кодировку вывода консоли, в том числе вывода по умолчанию у установленных роутеров.
        /// </summary>
        public static void SetOutputEncoding(Encoding encoding)
        {
            lock (InstallLock)
            {
                Console.OutputEncoding = encoding;

                // Console пересоздает свой вывод с новой кодировкой, только пока его не подменили через SetOut
                _out?.RecreateConsoleWriter();
                _error?.RecreateConsoleWriter();
            }
        }

        // Цели текущего потока выполнения - для мест, куда контекст выполнения не передается сам
        internal static (object Out, object Error) CaptureTargets()
        {
            lock (InstallLock)
            {
                return (_out?._current.Value, _error?._current.Value);
            }
        }

        internal static void ApplyTargets((object Out, object Error) targets)
        {
            lock (InstallLock)
            {
                if (_out != null)
                    _out._current.Value = (Target)targets.Out;
                if (_error != null)
                    _error._current.Value = (Target)targets.Error;
            }
        }

        /// <summary>
        /// Поток, в который перенаправлен стандартный вывод текущего потока выполнения, или null.
        /// </summary>
        public static IValue GetOutSource() => GetSource(_out, Console.Out);

        /// <summary>
        /// Поток, в который перенаправлен поток ошибок текущего потока выполнения, или null.
        /// </summary>
        public static IValue GetErrorSource() => GetSource(_error, Console.Error);

        private static bool IsInstalled(ConsoleWriterRouter router, TextWriter console)
            => router != null && ReferenceEquals(console, router._installed);

        private static IValue GetSource(ConsoleWriterRouter router, TextWriter console)
            => IsInstalled(router, console) ? router._current.Value?.Source : null;

        // Публично не узнать, подменял ли хост вывод консоли. Но при смене кодировки Console пересоздает
        // только свой вывод: присваиваем ту же кодировку и смотрим, сменился ли объект
        private static bool IsConsoleWriter(Func<TextWriter> consoleWriter)
        {
            var writer = consoleWriter();
            try
            {
                var encoding = Console.OutputEncoding;
                Console.OutputEncoding = encoding;
            }
            catch (Exception e) when (e is IOException || e is UnauthorizedAccessException || e is PlatformNotSupportedException)
            {
                // Без консоли кодировку не сменить, и пересоздавать вывод не придется
                return false;
            }

            return !ReferenceEquals(writer, consoleWriter());
        }

        private void RecreateConsoleWriter()
        {
            if (_openConsoleStream == null)
                return;

            var previous = _default;
            _default = CreateConsoleWriter(_openConsoleStream());
            previous.Flush();
        }

        // Как свой вывод создает Console: без BOM и со сбросом после каждой записи
        private static TextWriter CreateConsoleWriter(Stream stream)
        {
            var writer = new StreamWriter(stream, WithoutPreamble(Console.OutputEncoding), 256, leaveOpen: true)
            {
                AutoFlush = true
            };
            return TextWriter.Synchronized(writer);
        }

        private static Encoding WithoutPreamble(Encoding encoding)
        {
            if (encoding.GetPreamble().Length == 0)
                return encoding;

            return encoding.CodePage switch
            {
                65001 => new UTF8Encoding(false),
                1200 => new UnicodeEncoding(false, false),
                1201 => new UnicodeEncoding(true, false),
                12000 => new UTF32Encoding(false, false),
                12001 => new UTF32Encoding(true, false),
                _ => encoding
            };
        }

        private void SetCurrent(TextWriter writer, IValue source)
        {
            _current.Value = writer == null ? null : new Target(writer, source);
        }

        private TextWriter Writer => _current.Value?.Writer ?? _default;

        public override Encoding Encoding => Writer.Encoding;

        public override IFormatProvider FormatProvider => Writer.FormatProvider;

        public override void Write(char value) => Writer.Write(value);

        public override void Write(char[] buffer, int index, int count) => Writer.Write(buffer, index, count);

        public override void Write(ReadOnlySpan<char> buffer) => Writer.Write(buffer);

        public override void Write(string value) => Writer.Write(value);

        public override void WriteLine() => Writer.WriteLine();

        public override void WriteLine(ReadOnlySpan<char> buffer) => Writer.WriteLine(buffer);

        public override void WriteLine(string value) => Writer.WriteLine(value);

        public override void Flush() => Writer.Flush();

        private sealed class Target
        {
            public Target(TextWriter writer, IValue source)
            {
                Writer = writer;
                Source = source;
            }

            public TextWriter Writer { get; }

            public IValue Source { get; }
        }
    }
}
