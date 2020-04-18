/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if NETFRAMEWORK
using System.ServiceModel;
#endif

namespace OneScript.DebugProtocol
{
#if NETFRAMEWORK
    [ServiceContract(
        Namespace = "http://oscript.io/services/debugger", 
        SessionMode = SessionMode.Required,
        CallbackContract = typeof(IDebugEventListener))]
#endif
    public interface IDebuggerService
    {
        /// <summary>
        /// Разрешает потоку виртуальной машины начать выполнение скрипта
        /// Все точки останова уже установлены, все настройки сделаны
        /// </summary>
#if NETFRAMEWORK
        [OperationContract(IsOneWay = true)]
#endif
        void Execute(int threadId);
        
        /// <summary>
        /// Установка точек остановки
        /// </summary>
        /// <param name="breaksToSet"></param>
        /// <returns>Возвращает установленные точки (те, которые смог установить)</returns>
#if NETFRAMEWORK
        [OperationContract]
#endif
        Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet);

        /// <summary>
        /// Запрашивает состояние кадров стека вызовов
        /// </summary>
#if NETFRAMEWORK
        [OperationContract]
#endif
        StackFrame[] GetStackFrames(int threadId);

        /// <summary>
        /// Получает значения переменных
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <param name="path"></param>
        /// <returns></returns>
#if NETFRAMEWORK
        [OperationContract]
#endif
        Variable[] GetVariables(int threadId, int frameIndex, int[] path);

        /// <summary>
        /// Получает значения переменных вычисленного выражения
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="frameIndex"></param>
        /// <param name="path"></param>
        /// <returns></returns>
#if NETFRAMEWORK
        [OperationContract]
#endif
        Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path);

        /// <summary>
        /// Вычисление выражения на остановленном процессе
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="contextFrame">Кадр стека, относительно которого вычисляем</param>
        /// <param name="expression">Выражение</param>
        /// <returns>Переменная с результатом</returns>
#if NETFRAMEWORK
        [OperationContract]
#endif
        Variable Evaluate(int threadId, int contextFrame, string expression);

#if NETFRAMEWORK
        [OperationContract(IsOneWay = true)]
#endif
        void Next(int threadId);

#if NETFRAMEWORK
        [OperationContract(IsOneWay = true)]
#endif
        void StepIn(int threadId);

#if NETFRAMEWORK
        [OperationContract(IsOneWay = true)]
#endif
        void StepOut(int threadId);

#if NETFRAMEWORK
        [OperationContract]
#endif
        int[] GetThreads();
    }
}
