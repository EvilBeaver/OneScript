/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Net;
using System.Net.Sockets;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Net
{
    /// <summary>
    /// Простой однопоточный tcp-сокет. Слушает входящие соединения на определенном порту
    /// </summary>
    [ContextClass("TCPСервер", "TCPServer")]
    public class TCPServer : AutoContext<TCPServer>
    {
        private readonly TcpListener _listener;

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
