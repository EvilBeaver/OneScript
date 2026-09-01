/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("OneScript.StandardLibrary.Tests")]

namespace OneScript.StandardLibrary.Processes
{
    /// <summary>
    /// Неблокирующий читатель поверх потока вывода процесса
    /// (Process.StandardOutput/StandardError). Фоновая задача перекачивает
    /// сырые символы источника в буфер, включая настоящие переводы строк,
    /// записанные процессом.
    /// Событийный API (BeginOutputReadLine) не используется: он вырезает
    /// терминаторы строк, и их обратный синтез порождал гонку (issue #1726),
    /// а вывод, не завершенный переводом строки, застревал в .NET до конца процесса.
    /// </summary>
    class ProcessOutputWrapper : TextReader
    {
        private const int CompactionThreshold = 4096;

        private readonly TextReader _source;
        private readonly StringBuilder _buffer = new StringBuilder(4096);

        private int _bufferIndex = 0;

        // Пишутся пампом и читаются читателями только под lock(_buffer)
        private bool _streamEnded;
        private Exception _pumpError;

        private volatile bool _stopRequested;

        private Task _pumpTask;

        private bool AlreadyReading { get; set; }

        public ProcessOutputWrapper(TextReader source)
        {
            _source = source;
        }

        public void StartReading()
        {
            if (AlreadyReading)
                return;

            _pumpTask = Task.Run(() => PumpAsync(_source));

            AlreadyReading = true;
        }

        private async Task PumpAsync(TextReader reader)
        {
            var chunk = new char[4096];
            try
            {
                while (true)
                {
                    int read = await reader.ReadAsync(chunk, 0, chunk.Length).ConfigureAwait(false);
                    if (read == 0)
                        break;

                    // После закрытия читателя данные никому не нужны, но пайп
                    // продолжает дренироваться до конца потока: иначе процесс,
                    // заполнив пайп, навсегда заблокируется на записи
                    if (_stopRequested)
                        continue;

                    lock (_buffer)
                    {
                        _buffer.Append(chunk, 0, read);
                    }
                }
            }
            catch (Exception e)
            {
                // Ошибка источника перебрасывается читателю, когда он вычитает
                // накопленные данные (см. ThrowIfPumpFailed). После остановки
                // чтения (Dispose) источник закрыт, его ошибки ожидаемы.
                if (!_stopRequested)
                {
                    lock (_buffer)
                    {
                        _pumpError = e;
                    }
                }
            }
            finally
            {
                lock (_buffer)
                {
                    _streamEnded = true;
                }
            }
        }

        // Ошибка чтения источника отдается после уже накопленных данных,
        // поэтому перебрасывается только при пустом буфере.
        // должна вызываться ТОЛЬКО внутри вышестоящего блока lock.
        private void ThrowIfPumpFailed()
        {
            if (_pumpError != null && _bufferIndex >= _buffer.Length)
                ExceptionDispatchInfo.Capture(_pumpError).Throw();
        }

        /// <summary>
        /// Дождаться, пока весь вывод процесса будет перекачан в буфер.
        /// Вызывается после Process.WaitForExit(), чтобы Прочитать() гарантированно
        /// видел хвост вывода (аналог гарантии WaitForExit() для событийного чтения).
        /// </summary>
        internal void WaitSourceDrained()
        {
            _pumpTask?.Wait();
        }

        /// <summary>
        /// Источник дочитан до конца: после завершения процесса хвост его
        /// вывода гарантированно доступен читателям.
        /// </summary>
        internal bool IsDrained => _pumpTask == null || _pumpTask.IsCompleted;

        // Для тестов: физический размер внутреннего буфера
        internal int InternalBufferSize
        {
            get
            {
                lock (_buffer)
                {
                    return _buffer.Length;
                }
            }
        }

        public override int Peek()
        {
            lock (_buffer)
            {
                if (_bufferIndex >= _buffer.Length)
                {
                    ThrowIfPumpFailed();
                    return -1; // no data
                }

                return _buffer[_bufferIndex];
            }
        }

        public override int Read()
        {
            lock (_buffer)
            {
                int ch = ReadInternal();
                if (ch == -1)
                    ThrowIfPumpFailed();

                CompactBuffer();
                return ch;
            }
        }

        // Вычитанный префикс буфера периодически удаляется, иначе вывод
        // долгоживущего процесса накапливается в памяти даже при аккуратном
        // читателе.
        // должна вызываться ТОЛЬКО внутри вышестоящего блока lock.
        private void CompactBuffer()
        {
            if (_bufferIndex >= CompactionThreshold)
            {
                _buffer.Remove(0, _bufferIndex);
                _bufferIndex = 0;
            }
        }

        // неблокирующий доступ к буферу.
        // должна вызываться ТОЛЬКО внутри вышестоящего блока lock.
        private int ReadInternal()
        {
            if (_bufferIndex < _buffer.Length)
                return _buffer[_bufferIndex++];

            return -1;
        }

        public override int Read(char[] destBuffer, int index, int count)
        {
            if (destBuffer == null)
                throw new ArgumentNullException(nameof(destBuffer));
            if (index < 0)
                throw new ArgumentOutOfRangeException(nameof(index), "Index is below zero");
            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count), "Count is below zero");
            if (destBuffer.Length - index < count)
                throw new ArgumentException("Invalid offset");


            int n = 0;
            lock (_buffer)
            {
                do
                {
                    int ch = ReadInternal();
                    if (ch == -1) break;

                    destBuffer[index + n++] = (char)ch;
                } while (n < count);

                if (n == 0 && count > 0)
                    ThrowIfPumpFailed();

                CompactBuffer();
            }

            return n;
        }

        /// <summary>
        /// Возвращает очередную строку, только когда она гарантированно полна:
        /// ее терминатор уже в буфере либо поток источника закончился.
        /// Для незавершенной строки возвращает null (данных пока нет) —
        /// частично записанная процессом строка не может быть возвращена
        /// ни как строка, ни по кускам.
        /// </summary>
        public override string ReadLine()
        {
            lock (_buffer)
            {
                for (int i = _bufferIndex; i < _buffer.Length; i++)
                {
                    char ch = _buffer[i];

                    if (ch == '\n')
                        return ConsumeLine(i, i + 1);

                    if (ch == '\r')
                    {
                        if (i + 1 < _buffer.Length)
                            return ConsumeLine(i, _buffer[i + 1] == '\n' ? i + 2 : i + 1);

                        if (_streamEnded)
                            return ConsumeLine(i, i + 1);

                        // '\r' — последний символ буфера, а поток еще жив:
                        // парный '\n' может быть в пути, ждем следующей порции
                        return null;
                    }
                }

                // терминатора нет; после конца потока остаток буфера — последняя строка
                if (_streamEnded && _bufferIndex < _buffer.Length)
                    return ConsumeLine(_buffer.Length, _buffer.Length);

                ThrowIfPumpFailed();
                return null;
            }
        }

        // должна вызываться ТОЛЬКО внутри вышестоящего блока lock.
        private string ConsumeLine(int lineEnd, int nextPosition)
        {
            var line = _buffer.ToString(_bufferIndex, lineEnd - _bufferIndex);
            _bufferIndex = nextPosition;
            CompactBuffer();
            return line;
        }

        public override string ReadToEnd()
        {
            lock (_buffer)
            {
                ThrowIfPumpFailed();

                string data = _buffer.ToString(_bufferIndex, _buffer.Length - _bufferIndex);
                ResetBuffer();
                return data;
            }
        }

        private void ResetBuffer()
        {
            _buffer.Clear();
            _bufferIndex = 0;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stopRequested = true;
            }

            base.Dispose(disposing);
        }
    }
}
