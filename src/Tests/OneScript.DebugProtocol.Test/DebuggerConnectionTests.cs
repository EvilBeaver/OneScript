/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using FluentAssertions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugProtocol.Test.Tools;
using OneScript.DebugServices;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class DebuggerConnectionTests
    {
        // С запасом для медленных раннеров CI: ожидания заканчиваются, как только условие выполнено
        private const int WaitTimeout = 10000;

        [Fact]
        public void ClientClosedBeforeHandshakeDoesNotStopDebugger()
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport) { AttachMode = true };
            debugger.Start();
            var client = new TestDebuggerClient();
            try
            {
                // Проверка порта: подключились и сразу закрылись
                using (var probe = new TcpClient())
                {
                    probe.Connect(new IPEndPoint(IPAddress.Loopback, transport.ActualPort()));
                    probe.GetStream().WriteByte(0);
                }

                client.Connect(transport.ActualPort());

                WaitEvent(() => debugger.GetSession().IsActive, WaitTimeout).Should().BeTrue();
            }
            finally
            {
                client.Close();
                debugger.NotifyProcessExit(0);
            }
        }

        [Fact]
        public void SessionEndsWhenClientDropsConnection()
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport) { AttachMode = true };
            debugger.Start();
            var client = new TestDebuggerClient();
            var client2 = new TestDebuggerClient();
            try
            {
                client.Connect(transport.ActualPort());
                var session = debugger.GetSession();
                WaitEvent(() => session.IsActive, WaitTimeout).Should().BeTrue();

                client.Close();

                WaitEvent(() => !session.IsActive, WaitTimeout).Should().BeTrue();

                client2.Connect(transport.ActualPort());
                WaitEvent(() => debugger.GetSession().IsActive, WaitTimeout).Should().BeTrue();
            }
            finally
            {
                client.Close();
                client2.Close();
                debugger.NotifyProcessExit(0);
            }
        }

        [Fact]
        public void LaunchContinuesWhenClientDropsBeforeExecute()
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport) { AttachMode = false };
            debugger.Start();
            var client = new TestDebuggerClient();
            try
            {
                client.Connect(transport.ActualPort());
                WaitEvent(() => debugger.GetSession().IsActive, WaitTimeout).Should().BeTrue();

                // Сессия берется до закрытия клиента: после закрытия GetSession отдаст новую,
                // которая ждет следующего подключения
                var session = debugger.GetSession();
                var started = new ManualResetEventSlim();
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    session.WaitReadyToRun();
                    started.Set();
                });

                client.Close();

                started.Wait(WaitTimeout).Should().BeTrue();
            }
            finally
            {
                client.Close();
                debugger.NotifyProcessExit(0);
            }
        }

        [Fact]
        public void ContinueSentWithStopEventIsNotLost()
        {
            var token = new MachineWaitToken();
            token.Reset();
            token.Set();

            RunWithTimeout(token.Wait).Should().BeTrue();
            token.IsStopped.Should().BeFalse();
        }

        [Fact]
        public void ReleasedThreadDoesNotStopAgain()
        {
            var token = new MachineWaitToken();
            token.Dispose();

            token.Reset();

            token.IsStopped.Should().BeFalse();
            RunWithTimeout(token.Wait).Should().BeTrue();
        }

        [Fact]
        public void DebuggerWorkRunsOnStoppedThread()
        {
            var token = new MachineWaitToken();
            var stoppedThreadId = 0;
            var stoppedThread = new Thread(() =>
            {
                stoppedThreadId = Environment.CurrentManagedThreadId;
                token.Reset();
                token.Wait();
            });
            stoppedThread.Start();
            WaitEvent(() => token.IsStopped, WaitTimeout).Should().BeTrue();

            var workThreadId = token.RunOnStoppedThread(() => Environment.CurrentManagedThreadId);

            workThreadId.Should().Be(stoppedThreadId);
            token.IsStopped.Should().BeTrue();

            token.Set();
            stoppedThread.Join(WaitTimeout).Should().BeTrue();
        }

        [Fact]
        public void DebuggerWorkOnRunningThreadIsRejected()
        {
            var token = new MachineWaitToken();

            token.Invoking(t => t.RunOnStoppedThread(() => 1))
                .Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void MessagesFromManyThreadsAreNotMixed()
        {
            const int threadsCount = 4;
            const int messagesPerThread = 10;
            var stream = new SlowStream();
            var channel = new JsonDtoChannel(stream);

            // Сообщение больше части, которую поток отправляет за раз
            var payload = new string('x', 100_000);
            var threads = Enumerable.Range(0, threadsCount).Select(number => new Thread(() =>
            {
                for (var i = 0; i < messagesPerThread; i++)
                {
                    channel.Write(RpcCall.Create("Test", "Message", number, payload));
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

            var reader = new JsonDtoChannel(new MemoryStream(stream.ToArray()));
            var messages = Enumerable.Range(0, threadsCount * messagesPerThread)
                .Select(_ => reader.Read<RpcCall>())
                .ToList();

            messages.Should().OnlyContain(m => (string)m.Parameters[1] == payload);
        }

        [Fact]
        public void StopFromMessageHandlerDoesNotInterruptIt()
        {
            // Пустой поток: канал сразу сообщает об обрыве соединения
            var server = new DefaultMessageServer<RpcCall>(new JsonDtoChannel(new MemoryStream()));
            var busy = new object();
            var holding = new ManualResetEventSlim();
            var release = new ManualResetEventSlim();
            var handled = new ManualResetEventSlim();
            Exception handlerError = null;

            // Блокировку держит другой поток, как OnSessionClose при новом подключении
            var holder = new Thread(() =>
            {
                lock (busy)
                {
                    holding.Set();
                    release.Wait();
                }
            });
            holder.Start();
            holding.Wait();

            server.DataReceived += (sender, e) =>
            {
                // Как DebugSession.Dispose при обрыве соединения
                server.Stop();
                try
                {
                    var waiter = new Thread(() => { Thread.Sleep(200); release.Set(); });
                    waiter.Start();
                    lock (busy)
                    {
                    }
                }
                catch (Exception ex)
                {
                    handlerError = ex;
                }
                handled.Set();
            };
            server.Start();

            handled.Wait(WaitTimeout).Should().BeTrue();
            release.Set();
            holder.Join();
            handlerError.Should().BeNull();
        }

        private static bool RunWithTimeout(Action action)
        {
            var thread = new Thread(() => action()) { IsBackground = true };
            thread.Start();
            return thread.Join(WaitTimeout);
        }

        private static bool WaitEvent(Func<bool> predicate, int timeout)
        {
            var start = Environment.TickCount;
            while (!predicate())
            {
                if (Environment.TickCount - start > timeout)
                    return false;

                Thread.Sleep(10);
            }

            return true;
        }

        /// <summary>
        /// Поток, который, как сокет с заполненным буфером, отправляет большую запись частями
        /// </summary>
        private class SlowStream : Stream
        {
            private const int ChunkSize = 16 * 1024;
            private readonly List<byte> _data = new List<byte>();

            public override void Write(byte[] buffer, int offset, int count)
            {
                for (var sent = 0; sent < count; sent += ChunkSize)
                {
                    lock (_data)
                    {
                        _data.AddRange(buffer.Skip(offset + sent).Take(Math.Min(ChunkSize, count - sent)));
                    }
                    Thread.Sleep(1);
                }
            }

            public byte[] ToArray()
            {
                lock (_data)
                {
                    return _data.ToArray();
                }
            }

            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush()
            {
            }

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }
    }
}
