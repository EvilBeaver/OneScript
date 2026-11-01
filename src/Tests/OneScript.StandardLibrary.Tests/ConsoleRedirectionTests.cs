/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using OneScript.StandardLibrary.Binary;
using OneScript.StandardLibrary.Text;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    public class ConsoleRedirectionTests
    {
        [Fact]
        public void ErrorStreamIsWrittenWithoutFlush()
        {
            var console = new ConsoleContext(null);
            var target = MemoryStreamContext.Constructor();

            RunInThreads(1, _ =>
            {
                console.SetError(target);
                Console.Error.Write("ошибка");
            });

            ReadText(target).Should().Be("ошибка");
        }

        [Fact]
        public void EachThreadWritesToItsOwnErrorStream()
        {
            const int threadsCount = 8;
            var console = new ConsoleContext(null);
            var targets = Enumerable.Range(0, threadsCount).Select(_ => MemoryStreamContext.Constructor()).ToArray();
            using var barrier = new Barrier(threadsCount);

            RunInThreads(threadsCount, number =>
            {
                console.SetError(targets[number]);
                // Все потоки перенаправлены до того, как кто-то начнет писать
                barrier.SignalAndWait();
                for (var i = 0; i < 100; i++)
                {
                    Console.Error.WriteLine($"поток {number}");
                }
            });

            for (var number = 0; number < threadsCount; number++)
            {
                ReadText(targets[number]).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
                    .Should().HaveCount(100).And.OnlyContain(x => x == $"поток {number}");
            }
        }

        private static void RunInThreads(int threadsCount, Action<int> action)
        {
            Exception error = null;
            var threads = Enumerable.Range(0, threadsCount).Select(number => new Thread(() =>
            {
                try
                {
                    action(number);
                }
                catch (Exception e)
                {
                    error = e;
                }
            })).ToArray();

            foreach (var thread in threads)
            {
                thread.Start();
            }
            foreach (var thread in threads)
            {
                thread.Join();
            }

            error.Should().BeNull();
        }

        private static string ReadText(MemoryStreamContext stream)
        {
            var bytes = ((MemoryStream)stream.GetUnderlyingStream()).ToArray();
            using var reader = new StreamReader(new MemoryStream(bytes), Console.OutputEncoding, true);
            return reader.ReadToEnd();
        }
    }
}
