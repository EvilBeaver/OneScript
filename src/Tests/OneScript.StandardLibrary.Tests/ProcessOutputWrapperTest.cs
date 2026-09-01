/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Xunit;
using FluentAssertions;
using OneScript.StandardLibrary.Processes;

namespace OneScript.StandardLibrary.Tests
{
    public class ProcessOutputWrapperTest : IDisposable
    {
        // Источник, отдающий текст порциями, как пайп работающего процесса:
        // Read блокируется до появления следующей порции, Complete() дает EOF.
        private class ChunkedReader : TextReader
        {
            private readonly BlockingCollection<string> _chunks = new BlockingCollection<string>();
            private string _current = string.Empty;
            private int _pos;
            private Exception _error;

            public void Push(string chunk) => _chunks.Add(chunk);

            public void Complete() => _chunks.CompleteAdding();

            public void Fail(Exception error)
            {
                _error = error;
                _chunks.CompleteAdding();
            }

            public bool AllTaken => _chunks.Count == 0;

            public override int Read(char[] buffer, int index, int count)
            {
                while (_pos >= _current.Length)
                {
                    if (!_chunks.TryTake(out _current, Timeout.Infinite))
                    {
                        if (_error != null)
                            throw _error;

                        return 0; // EOF
                    }

                    _pos = 0;
                }

                int n = Math.Min(count, _current.Length - _pos);
                _current.CopyTo(_pos, buffer, index, n);
                _pos += n;
                return n;
            }
        }

        private readonly ChunkedReader _source = new ChunkedReader();
        private readonly ProcessOutputWrapper _wrapper;

        public ProcessOutputWrapperTest()
        {
            _wrapper = new ProcessOutputWrapper(_source);
            _wrapper.StartReading();
        }

        public void Dispose()
        {
            _source.Complete();
            _wrapper.Dispose();
        }

        // Перекачка фоновая, ждем видимого эффекта от отданной порции
        private T Eventually<T>(Func<T> read, Func<T, bool> arrived)
        {
            var sw = Stopwatch.StartNew();
            while (true)
            {
                var value = read();
                if (arrived(value) || sw.ElapsedMilliseconds > 5000)
                    return value;

                Thread.Sleep(1);
            }
        }

        private string EventuallyReadLine() => Eventually(() => _wrapper.ReadLine(), line => line != null);

        [Fact]
        public void ReadLine_ReturnsLinesInOrder()
        {
            _source.Push("L1\nL2\nL3\n");

            EventuallyReadLine().Should().Be("L1");
            EventuallyReadLine().Should().Be("L2");
            EventuallyReadLine().Should().Be("L3");
            _wrapper.ReadLine().Should().BeNull();
        }

        // Гонка из issue #1726: читатель вычерпал буфер по границе строки,
        // затем поступила следующая порция. Лишняя пустая строка не появляется.
        [Fact]
        public void ReadLine_DoesNotInsertEmptyLine_WhenBufferDrainedBetweenWrites()
        {
            _source.Push("L1\nL2\n");

            EventuallyReadLine().Should().Be("L1");
            EventuallyReadLine().Should().Be("L2");
            _wrapper.ReadLine().Should().BeNull();

            _source.Push("L3\n");

            EventuallyReadLine().Should().Be("L3");
            _wrapper.ReadLine().Should().BeNull();
        }

        // Незавершенная строка не возвращается ни целиком, ни по кускам,
        // пока не придет ее терминатор
        [Fact]
        public void ReadLine_DoesNotTearLine_SplitBetweenWrites()
        {
            _source.Push("L1\nПол");

            EventuallyReadLine().Should().Be("L1");
            _wrapper.ReadLine().Should().BeNull();

            _source.Push("овина\n");

            EventuallyReadLine().Should().Be("Половина");
        }

        // Длинная строка, приходящая многими порциями: промежуточные опросы
        // возвращают null, итоговая строка собирается целиком
        [Fact]
        public void ReadLine_AssemblesLine_FromManyChunks()
        {
            _source.Push("aa");
            Eventually(() => _wrapper.Peek(), c => c != -1);
            _wrapper.ReadLine().Should().BeNull();

            _source.Push("bb");
            _wrapper.ReadLine().Should().BeNull();

            _source.Push("cc\n");
            EventuallyReadLine().Should().Be("aabbcc");
            _wrapper.ReadLine().Should().BeNull();
        }

        // CRLF, разорванный между порциями, не дает ни фантомной пустой строки,
        // ни '\r' в теле строки
        [Fact]
        public void ReadLine_HandlesCrLfSplitBetweenWrites()
        {
            _source.Push("L1\r");

            _wrapper.ReadLine().Should().BeNull();

            _source.Push("\nL2\r\n");

            EventuallyReadLine().Should().Be("L1");
            EventuallyReadLine().Should().Be("L2");
            _wrapper.ReadLine().Should().BeNull();
        }

        [Fact]
        public void ReadLine_PreservesRealEmptyLines()
        {
            _source.Push("L1\n\nL2\n");

            EventuallyReadLine().Should().Be("L1");
            EventuallyReadLine().Should().Be("");
            EventuallyReadLine().Should().Be("L2");
            _wrapper.ReadLine().Should().BeNull();
        }

        // Пустая последняя строка вывода больше не теряется
        [Fact]
        public void ReadLine_PreservesTrailingEmptyLine()
        {
            _source.Push("L1\n\n");
            _source.Complete();

            EventuallyReadLine().Should().Be("L1");
            EventuallyReadLine().Should().Be("");
            _wrapper.ReadLine().Should().BeNull();
        }

        // Вывод, не завершенный переводом строки, возвращается по концу потока
        [Fact]
        public void ReadLine_ReturnsUnterminatedTail_AfterStreamEnd()
        {
            _source.Push("L1\nL2");

            EventuallyReadLine().Should().Be("L1");
            _wrapper.ReadLine().Should().BeNull();

            _source.Complete();

            EventuallyReadLine().Should().Be("L2");
            _wrapper.ReadLine().Should().BeNull();
        }

        // Прочитать() возвращает подлинные символы вывода процесса,
        // без синтеза и без подрезки
        [Fact]
        public void ReadToEnd_ReturnsRawOutput()
        {
            _source.Push("L1\nL2\n");

            Eventually(() => _wrapper.Peek(), c => c != -1);
            EventuallyReadLine().Should().Be("L1");

            _wrapper.ReadToEnd().Should().Be("L2\n");
        }

        // Ошибка чтения источника не глотается: она перебрасывается читателю,
        // но только после того, как он вычитает накопленные данные
        [Fact]
        public void SourceFailure_SurfacesToReader_AfterBufferedDataIsConsumed()
        {
            _source.Push("L1\n");
            _source.Fail(new IOException("pipe failure"));

            EventuallyReadLine().Should().Be("L1");

            var error = Eventually(ReadLineError, e => e != null);
            error.Should().BeOfType<IOException>().Which.Message.Should().Be("pipe failure");

            // ошибка не одноразовая: последующие обращения тоже ее видят
            ReadLineError().Should().BeOfType<IOException>();
        }

        private Exception ReadLineError()
        {
            try
            {
                _wrapper.ReadLine();
                return null;
            }
            catch (Exception e)
            {
                return e;
            }
        }

        // После закрытия читателя источник продолжает дренироваться
        // (иначе процесс заблокируется на записи в переполненный пайп),
        // но данные больше не накапливаются
        [Fact]
        public void AfterDispose_SourceIsStillDrained_ButDataIsDiscarded()
        {
            _source.Push("L1\n");
            EventuallyReadLine().Should().Be("L1");

            _wrapper.Dispose();
            _source.Push("L2\n");

            Eventually(() => _source.AllTaken, taken => taken).Should().BeTrue();
            _wrapper.ReadLine().Should().BeNull();
        }

        // Вычитанный префикс буфера освобождается: вывод долгоживущего
        // процесса не накапливается в памяти при аккуратном читателе
        [Fact]
        public void ConsumedPrefix_IsCompacted()
        {
            var line = new string('x', 1000);
            for (int i = 0; i < 20; i++)
            {
                _source.Push(line + "\n");
                EventuallyReadLine().Should().Be(line);
            }

            _wrapper.InternalBufferSize.Should().BeLessThan(2 * 4096);
        }

        [Fact]
        public void IsDrained_BecomesTrue_OnlyAfterSourceEnd()
        {
            _source.Push("L1\n");
            EventuallyReadLine().Should().Be("L1");

            _wrapper.IsDrained.Should().BeFalse();

            _source.Complete();

            Eventually(() => _wrapper.IsDrained, drained => drained).Should().BeTrue();
        }
    }
}
