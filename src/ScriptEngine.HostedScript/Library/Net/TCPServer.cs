using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Net
{
    /// <summary>
    /// Простой однопоточный tcp-сокет. Слушает входящие соединения на определенном порту
    /// </summary>
    [ContextClass("TCPСервер", "TCPServer")]
    public class TCPServer : AutoContext<TCPServer>
    {
        private TcpListener _listener;

        public TCPServer(int port)
        {
            _listener = new TcpListener(IPAddress.Any, port);
        }

        /// <summary>
        /// Метод инициализирует TCP-сервер и подготавливает к приему входящих соединений
        /// </summary>
        [ContextMethod("Запустить", "Start")]
        public void Start()
        {
            _listener.Start();
        }

        /// <summary>
        /// Останавливает прослушивание порта.
        /// </summary>
        [ContextMethod("Остановить", "Stop")]
        public void Stop()
        {
            _listener.Stop();
        }

        /// <summary>
        /// Приостановить выполнение скрипта и ожидать соединений по сети.
        /// После получения соединения выполнение продолжается
        /// </summary>
        /// <param name="timeout">Значение таймаута в миллисекундах.</param>
        /// <returns>TCPСоединение. Объект, позволяющий обмениваться данными с удаленным хостом.</returns>
        [ContextMethod("ОжидатьСоединения","WaitForConnection")]
        public TCPClient WaitForConnection(int timeout = 0)
        {
            if (0 != timeout && !_listener.Pending())
            {
                System.Threading.Thread.Sleep(timeout);
                if (!_listener.Pending())
                      return null;
            }

            var client = _listener.AcceptTcpClient();
            return new TCPClient(client);
        }

        /// <summary>
        /// Создает новый сокет с привязкой к порту.
        /// </summary>
        /// <param name="port">Порт, который требуется слушать.</param>
        [ScriptConstructor]
        public static TCPServer ConstructByPort(IValue port)
        {
            return new TCPServer((int)port.AsNumber());
        }
    }
}
