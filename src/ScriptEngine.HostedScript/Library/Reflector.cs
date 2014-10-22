using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    /// <summary>
    /// Рефлектор предназначен для получения метаданных объектов во время выполнения.
    /// Как правило, рефлексия используется для проверки наличия у объекта определенных свойств/методов.
    /// В OneScript рефлексию можно применять для вызова методов объектов по именам методов.
    /// </summary>
    [ContextClass("Рефлектор","Reflector")]
    public class ReflectorContext : AutoContext<ReflectorContext>
    {
        public ReflectorContext()
        {

        }

        /// <summary>
        /// Вызывает метод по его имени.
        /// </summary>
        /// <param name="target">Объект, метод которого нужно вызвать.</param>
        /// <param name="methodName">Имя метода для вызова</param>
        /// <param name="arguments">Массив аргументов, передаваемых методу</param>
        /// <returns>Если вызывается функция, то возвращается ее результат. В противном случае возвращается Неопределено.</returns>
        [ContextMethod("ВызватьМетод", "CallMethod")]
        public IValue CallMethod(IRuntimeContextInstance target, string methodName, IRuntimeContextInstance arguments = null)
        {
            ArrayImpl argArray;
            if (arguments != null)
            {
                argArray = arguments as ArrayImpl;
                if (argArray == null)
                    throw RuntimeException.InvalidArgumentType();
            }
            else
            {
                argArray = new ArrayImpl();
            }

            var methodIdx = target.FindMethod(methodName);

            var methInfo = target.GetMethodInfo(methodIdx);
            IValue retValue = ValueFactory.Create();
            if (methInfo.IsFunction)
            {
                target.CallAsFunction(methodIdx, argArray.ToArray(), out retValue);
            }
            else
            {
                target.CallAsProcedure(methodIdx, argArray.ToArray());
            }

            return retValue;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance CreateNew()
        {
            return new ReflectorContext();
        }
    }
}
