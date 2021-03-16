/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using System.Threading.Tasks;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Tasks
{
    [ContextClass("ФоновоеЗадание", "BackgroundTask")]
    public class BackgroundTask : AutoContext<BackgroundTask>
    {
        private readonly MethodInfo _method;
        private readonly int _methIndex;
        
        public BackgroundTask(IRuntimeContextInstance target, string methodName, ArrayImpl parameters = default)
        {
            Target = target;
            MethodName = methodName;
            Status = TaskStatusEnum.NotRunned;
            if(parameters != default)
                Parameters = new ArrayImpl(parameters);
            
            Identifier = new GuidWrapper();
            
            _methIndex = Target.FindMethod(MethodName);
            _method = Target.GetMethodInfo(_methIndex);
        }
        
        public Task WorkerTask { get; set; } 
        
        [ContextProperty("Идентификатор","Identifier")]
        public GuidWrapper Identifier { get; private set; }
        
        [ContextProperty("ИмяМетода","MethodName")]
        public string MethodName { get; private set; }
        
        [ContextProperty("Объект","Object")]
        public IRuntimeContextInstance Target { get; private set; }
        
        [ContextProperty("Состояние", "Status")]
        public TaskStatusEnum Status { get; private set; }
        
        [ContextProperty("Параметры", "Parameters")]
        public ArrayImpl Parameters { get; private set; }

        [ContextProperty("Результат", "Result")]
        public IValue Result { get; private set; } = ValueFactory.Create();

        [ContextProperty("ИнформацияОбОшибке", "ExceptionInfo")]
        public ExceptionInfoContext ExceptionInfo { get; private set; }

        /// <summary>
        /// Ждать завершения задания указанное число миллисекунд
        /// </summary>
        /// <param name="timeout">Таймаут. Если ноль - ждать вечно</param>
        /// <returns>Истина - дождались завершения. Ложь - сработал таймаут</returns>
        [ContextMethod("ОжидатьЗавершения", "Wait")]
        public bool Wait(int timeout = 0)
        {
            if(timeout < 0)
                throw RuntimeException.InvalidArgumentValue();
            if (timeout == 0)
                timeout = -1;
            
            return WorkerTask.Wait(timeout);
        }
        
        public void ExecuteOnCurrentThread()
        {
            if (Status != TaskStatusEnum.NotRunned)
                throw new RuntimeException(Locale.NStr("ru = 'Неверное состояние задачи';en = 'Incorrect task status'"));
            
            var parameters = Parameters?.ToArray() ?? new IValue[0];

            try
            {
                Status = TaskStatusEnum.Running;
                if (_method.IsFunction)
                {
                    Target.CallAsFunction(_methIndex, parameters, out var result);
                    Result = result;
                }
                else
                {
                    Target.CallAsProcedure(_methIndex, parameters);
                }

                Status = TaskStatusEnum.Completed;
            }
            catch (RuntimeException exception)
            {
                Status = TaskStatusEnum.CompletedWithErrors;
                ExceptionInfo = new ExceptionInfoContext(exception);
            }
        }
    }
}