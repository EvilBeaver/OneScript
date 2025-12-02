/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Threading;
using FluentAssertions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugProtocol.Test.Tools;
using OneScript.DebugServices;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class DebuggerTests
    {
        [Fact(DisplayName = "В режиме Launch сессия до соединения неактивна, после - активна.")]
        public void TestActivatingOnIncomingClient()
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport);
            debugger.AttachMode = false;
            debugger.Start();
            
            debugger.GetSession().IsActive.Should().BeFalse();
            
            var client = new TestDebuggerClient();
            client.Connect(transport.ActualPort());

            WaitEvent(() => debugger.GetSession().IsActive, 2000).Should().BeTrue();
        }
        
        [Theory(DisplayName = "В режиме Attach метод WaitReadyToRun не блокируется.")]
        [InlineData(true)]
        [InlineData(false)]
        public void TestNonBlockingSessionWaitToRun(bool connectBeforeWait)
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport);
            debugger.AttachMode = true;
            debugger.Start();
            
            debugger.GetSession().IsActive.Should().BeFalse();
            
            var client = new TestDebuggerClient();
            if (connectBeforeWait)
            {
                client.Connect(transport.ActualPort());
                Thread.Sleep(100);
            }
            
            var exitEvent = new ManualResetEventSlim(false);
            
            ThreadPool.QueueUserWorkItem(_ =>
            {
                debugger.GetSession().WaitReadyToRun(); // не должно зависнуть
                exitEvent.Set();
            });
            
            exitEvent.Wait(2000).Should().BeTrue();
        }
        
        [Theory(DisplayName = "В режиме Launch метод WaitReadyToRun блокируется.")]
        [InlineData(true)]
        [InlineData(false)]
        public void TestBlockingSessionWaitToRun(bool connectBeforeWait)
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport);
            debugger.AttachMode = false;
            debugger.Start();
            
            debugger.GetSession().IsActive.Should().BeFalse();
            
            var client = new TestDebuggerClient();
            if (connectBeforeWait)
            {
                client.Connect(transport.ActualPort());
                Thread.Sleep(100);
            }
            
            var exitEvent = new ManualResetEventSlim(false);
            
            ThreadPool.QueueUserWorkItem(_ =>
            {
                debugger.GetSession().WaitReadyToRun(); // должно зависнуть до получения команды Execute
                exitEvent.Set();
            });
            
            if (!connectBeforeWait)
            {
                client.Connect(transport.ActualPort());
                Thread.Sleep(100);
            }
            
            client.Send(RpcCall.Create(nameof(IDebuggerService), nameof(IDebuggerService.Execute), 0));
            
            exitEvent.Wait(2000).Should().BeTrue();
        }

        [Fact]
        public void CanCreateAnotherSessionAfterDisconnect()
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport);
            debugger.AttachMode = true;
            debugger.Start();
            
            debugger.GetSession().IsActive.Should().BeFalse();
            
            var client = new TestDebuggerClient();
            client.Connect(transport.ActualPort());

            var session = debugger.GetSession();
            
            WaitEvent(() => session.IsActive, 2000).Should().BeTrue();
            
            client.Send(RpcCall.Create(nameof(IDebuggerService), nameof(IDebuggerService.Disconnect), false));

            WaitEvent(() => !session.IsActive, 2000).Should().BeTrue();
            
            var client2 = new TestDebuggerClient();
            client2.Connect(transport.ActualPort());
            var session2 = debugger.GetSession();

            WaitEvent(() => session2.IsActive, 2000).Should().BeTrue();
            
            session2.Should().NotBe(session);
        }
        
        private bool WaitEvent(Func<bool> predicate, int timeout)
        {
            var wait = new SpinWait();
            int start = Environment.TickCount;
            while (!predicate())
            {
                wait.SpinOnce();
                var elapsed = Environment.TickCount - start;
                if (elapsed > timeout)
                {
                    return false;
                }
            }
            
            return true;
        }
    }
}