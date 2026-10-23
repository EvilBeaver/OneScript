/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Threading;
using FluentAssertions;
using OneScript.DebugProtocol.TcpServer;
using OneScript.DebugProtocol.Test.Tools;
using OneScript.DebugServices;
using OneScript.StandardLibrary;
using ScriptEngine.Hosting;
using Xunit;

namespace OneScript.DebugProtocol.Test
{
    public class DebuggerEvaluationTests
    {
        private const int StopLine = 11;
        private const int LineInsideCalledMethod = 6;

        private const string Script =
            "Перем Блок;\n" +
            "\n" +
            "Функция ВзятьБлокировку() Экспорт\n" +
            "	Блок.Заблокировать();\n" +
            "	Блок.Разблокировать();\n" +
            "	Возврат \"взял\";\n" +
            "КонецФункции\n" +
            "\n" +
            "Блок = Новый БлокировкаРесурса;\n" +
            "Блок.Заблокировать();\n" +
            "Сообщение = \"стоп\";\n" +
            "Блок.Разблокировать();\n";

        [Fact]
        public void WatchExpressionRunsOnStoppedThread()
        {
            var transport = new TcpDebugServer(0);
            var debugger = new DefaultDebugger(transport) { AttachMode = true };
            var engine = DefaultEngineBuilder.Create()
                .SetDefaultOptions()
                .UseBinaryDataOptions()
                .SetupEnvironment(e => e.AddStandardLibrary())
                .WithDebugger(debugger)
                .Build();
            engine.Initialize();
            debugger.Start();

            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.os");
            File.WriteAllText(path, Script);
            try
            {
                var client = new TestDebuggerClient();
                client.Connect(transport.ActualPort());
                SpinWait.SpinUntil(() => debugger.GetSession().IsActive, 2000).Should().BeTrue();

                Call(client, nameof(IDebuggerService.SetMachineBreakpoints), (object)new[]
                {
                    new Breakpoint { Source = path, Line = StopLine },
                    new Breakpoint { Source = path, Line = LineInsideCalledMethod }
                });

                Exception scriptError = null;
                var scriptThread = new Thread(() =>
                {
                    try
                    {
                        engine.AttachedScriptsFactory.LoadFromPath(engine.GetCompilerService(), path, engine.NewProcess());
                    }
                    catch (Exception e)
                    {
                        scriptError = e;
                    }
                }) { IsBackground = true };
                scriptThread.Start();

                var stopped = WaitForEvent(client, nameof(IDebugEventListener.ThreadStoppedEx));
                var threadId = Convert.ToInt32(stopped.Parameters[0]);
                Call(client, nameof(IDebuggerService.GetStackFrames), threadId);

                // Метод берет блокировку, которую держит остановленный поток, и в нем тоже точка останова.
                // В потоке отладчика это зависало навсегда
                var result = (Variable)Call(client, nameof(IDebuggerService.Evaluate), threadId, 0, "ВзятьБлокировку()");
                result.Presentation.Should().Be("взял");

                // Execute ничего не возвращает, ответа на него нет
                client.Send(RpcCall.Create(nameof(IDebuggerService), nameof(IDebuggerService.Execute), threadId));
                scriptThread.Join(5000).Should().BeTrue();
                scriptError.Should().BeNull();
            }
            finally
            {
                debugger.NotifyProcessExit(0);
                File.Delete(path);
            }
        }

        private static object Call(TestDebuggerClient client, string method, params object[] parameters)
        {
            client.Send(RpcCall.Create(nameof(IDebuggerService), method, parameters));
            while (true)
            {
                if (client.Read(5000) is RpcCallResult result && result.Id == method)
                {
                    if (result.ReturnValue is RpcExceptionDto error)
                        throw new InvalidOperationException(error.Description);

                    return result.ReturnValue;
                }
            }
        }

        private static RpcCall WaitForEvent(TestDebuggerClient client, string eventName)
        {
            while (true)
            {
                if (client.Read(5000) is RpcCall call && call.Id == eventName)
                    return call;
            }
        }
    }
}
