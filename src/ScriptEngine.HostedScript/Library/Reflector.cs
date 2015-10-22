/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System.Linq;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.ValueTable;

namespace ScriptEngine.HostedScript.Library
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
        public IValue CallMethod(IRuntimeContextInstance target, string methodName, ArrayImpl arguments = null)
        {
            if (arguments == null)
            {
                arguments = new ArrayImpl();
            }

            var methodIdx = target.FindMethod(methodName);

            var methInfo = target.GetMethodInfo(methodIdx);

            if (methInfo.ArgCount < arguments.Count())
                throw RuntimeException.TooManyArgumentsPassed();

            if (methInfo.ArgCount > arguments.Count())
                throw RuntimeException.TooLittleArgumentsPassed();

            IValue retValue = ValueFactory.Create();
            if (methInfo.IsFunction)
            {
                target.CallAsFunction(methodIdx, arguments.ToArray(), out retValue);
            }
            else
            {
                target.CallAsProcedure(methodIdx, arguments.ToArray());
            }

            return retValue;
        }

        /// <summary>
        /// Проверяет существование указанного метода у переданного объекта..
        /// </summary>
        /// <param name="target">Объект, из которого получаем таблицу методов.</param>
        /// <param name="methodName">Имя метода для вызова</param>
        /// <returns>Истину, если метод существует, и Ложь в обратном случае. </returns>
        [ContextMethod("МетодСуществует", "MethodExists")]
        public bool MethodExists(IRuntimeContextInstance target, string methodName)
        {
            try
            {
                var idx = target.FindMethod(methodName);
                return idx >= 0;
            }
            catch (RuntimeException)
            {
                return false;
            }
        }


        /// <summary>
        /// Получает таблицу методов для переданного объекта..
        /// </summary>
        /// <param name="target">Объект, из которого получаем таблицу методов.</param>
        /// <returns>Таблица значений с 3 колонками - Имя, КоличествоПараметров, ЭтоФункция. </returns>
        [ContextMethod("ПолучитьТаблицуМетодов", "GetMethodsTable")]
        public ValueTable.ValueTable GetMethodsTable(IRuntimeContextInstance target)
        {
            ValueTable.ValueTable Result = new ValueTable.ValueTable();
            
            var NameColumn = Result.Columns.Add("Имя", ValueFactory.Create(""), "Имя"); // TODO: Доработать после увеличения предела количества параметров
            var CountColumn = Result.Columns.Add("КоличествоПараметров", ValueFactory.Create(""), "Количество параметров"); // TODO: Доработать после увеличения предела количества параметров
            var IsFunctionColumn = Result.Columns.Add("ЭтоФункция", ValueFactory.Create(""), "Это функция"); // TODO: Доработать после увеличения предела количества параметров

            foreach(var methInfo in target.GetMethods())
            {
                ValueTableRow new_row = Result.Add();
                new_row.Set(NameColumn, ValueFactory.Create(methInfo.Name));
                new_row.Set(CountColumn, ValueFactory.Create(methInfo.ArgCount));
                new_row.Set(IsFunctionColumn, ValueFactory.Create(methInfo.IsFunction));
            }

            return Result;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance CreateNew()
        {
            return new ReflectorContext();
        }
    }
}
