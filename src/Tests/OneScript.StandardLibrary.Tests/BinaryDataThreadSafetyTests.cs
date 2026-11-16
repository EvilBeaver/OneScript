/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using FluentAssertions;
using OneScript.StandardLibrary.Binary;
using Xunit;

namespace OneScript.StandardLibrary.Tests
{
    public class BinaryDataThreadSafetyTests
    {
        [Fact]
        public void FileBackedDataIsReadFromManyThreads()
        {
            const int threadsCount = 8;
            var expected = new byte[64 * 1024];
            new Random(42).NextBytes(expected);
            var errors = new ConcurrentQueue<string>();

            for (var iteration = 0; iteration < 50; iteration++)
            {
                // Больше лимита памяти: данные во временном файле
                using var data = new BinaryDataContext(new MemoryStream(expected), 1);
                using var barrier = new Barrier(threadsCount);

                var threads = Enumerable.Range(0, threadsCount).Select(number => new Thread(() =>
                {
                    barrier.SignalAndWait();
                    try
                    {
                        if (!ReadAll(data, viaBuffer: number % 2 == 0).AsSpan().SequenceEqual(expected))
                            errors.Enqueue("данные отличаются");
                    }
                    catch (Exception e)
                    {
                        errors.Enqueue($"{e.GetType().Name}: {e.Message}");
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
            }

            errors.Should().BeEmpty();
        }

        [Fact]
        public void BufferKeepsFileBackedDataInFile()
        {
            var expected = new byte[] { 1, 2, 3 };
            using var data = new BinaryDataContext(new MemoryStream(expected), 1);

            data.Buffer.Should().Equal(expected);
            data.InMemory.Should().BeFalse();
            ReadAll(data, viaBuffer: false).Should().Equal(expected);
        }

        private static byte[] ReadAll(BinaryDataContext data, bool viaBuffer)
        {
            if (viaBuffer)
                return data.Buffer;

            using var stream = data.GetStream();
            using var copy = new MemoryStream();
            stream.CopyTo(copy);
            return copy.ToArray();
        }
    }
}
