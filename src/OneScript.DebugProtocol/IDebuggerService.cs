/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.DebugProtocol
{
    public interface IDebuggerService
    {
        /// <summary>
        /// Разрешает потоку виртуальной машины начать выполнение скрипта
        /// Все точки останова уже установлены, все настройки сделаны
        /// </summary>
        void Execute(int threadId);
        
        /// <summary>
        /// Установка точек остановки
        /// </summary>
        /// <param name="breaksToSet"></param>
        /// <returns>Возвращает установленные точки (те, которые смог установить)</returns>
        Breakpoint[] SetMachineBreakpoints(Breakpoint[] breaksToSet);

        /// <summary>
        /// Запрашивает состояние кадров стека вызовов
        /// </summary>
        StackFrame[] GetStackFrames(int threadId);

        /// <summary>
        /// Получает значения переменных
        /// </summary>
        /// <param name="frameIndex"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Variable[] GetVariables(int threadId, int frameIndex, int[] path);

        /// <summary>
        /// Получает значения переменных вычисленного выражения
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="frameIndex"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        Variable[] GetEvaluatedVariables(string expression, int threadId, int frameIndex, int[] path);

        /// <summary>
        /// Вычисление выражения на остановленном процессе
        /// </summary>
        /// <param name="threadId"></param>
        /// <param name="contextFrame">Кадр стека, относительно которого вычисляем</param>
        /// <param name="expression">Выражение</param>
        /// <returns>Переменная с результатом</returns>
        Variable Evaluate(int threadId, int contextFrame, string expression);

        void Next(int threadId);

        void StepIn(int threadId);

        void StepOut(int threadId);

        int[] GetThreads();
        
        int GetProcessId();
    }
}
