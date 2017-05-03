using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace OneScript.DebugProtocol
{
    [ServiceContract(
        Namespace = "http://oscript.io/services/debugger", 
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(IDebugEventListener))]
    public interface IDebuggerService
    {
        /// <summary>
        /// Разрешает потоку виртуальной машины начать выполнение скрипта
        /// Все точки останова уже установлены, все настройки сделаны
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Execute();

        /// <summary>
        /// Регистрирует адаптер отладки в качестве подписчика событий от Машины
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void RegisterEventListener();

        /// <summary>
        /// Установка точек остановки
        /// </summary>
        /// <param name="breaksToSet"></param>
        /// <returns>Возвращает установленные точки (те, которые смог установить)</returns>
        [OperationContract]
        Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet);

        /// <summary>
        /// Запрашивает состояние кадров стека вызовов
        /// </summary>
        [OperationContract]
        StackFrame[] GetStackFrames();

        [OperationContract]
        Variable[] GetVariables(int frameIndex);
    }

    public interface IDebugEventListener
    {
        [OperationContract(IsOneWay = true)]
        void ThreadStopped(int threadId, ThreadStopReason reason);
    }
}
