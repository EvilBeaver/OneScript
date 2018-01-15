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
using System.ServiceModel;

namespace OneScript.DebugProtocol
{
    public interface IDebuggerService
    {
        /// <summary>
        /// Разрешает потоку виртуальной машины начать выполнение скрипта
        /// Все точки останова уже установлены, все настройки сделаны
        /// </summary>
        void Execute();
        
        /// <summary>
        /// Установка точек остановки
        /// </summary>
        /// <param name="breaksToSet"></param>
        /// <returns>Возвращает установленные точки (те, которые смог установить)</returns>
        Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet);

        /// <summary>
        /// Запрашивает состояние кадров стека вызовов
        /// </summary>
        StackFrame[] GetStackFrames();

        /// <summary>
        /// Получает значения переменных
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Variable[] GetVariables(int frameIndex, int[] path);

        /// <summary>
        /// Вычисление выражения на остановленном процессе
        /// </summary>
        /// <param name="contextFrame">Кадр стека, относительно которого вычисляем</param>
        /// <param name="expression">Выражение</param>
        /// <returns>Переменная с результатом</returns>
        Variable Evaluate(int contextFrame, string expression);

        void Next();

        void StepIn();

        void StepOut();

        event EventHandler<ProcessExitedEventArgs> ProcessExited;
        event EventHandler<ThreadStoppedEventArgs> ThreadStopped;
    }

    public class ThreadStoppedEventArgs : EventArgs
    {
        public ThreadStopReason Reason { get; set; }
    }

    public class ProcessExitedEventArgs : EventArgs
    {
        public int ThreadId { get; set; }
        public ThreadStopReason Reason { get; set; }
    }
    
}
