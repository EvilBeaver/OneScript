/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
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
        /// <param name="arguments">Массив аргументов, передаваемых методу. Следует учесть, что все параметры нужно передавать явно, в том числе необязательные.</param>
        /// <returns>Если вызывается функция, то возвращается ее результат. В противном случае возвращается Неопределено.</returns>
        [ContextMethod("ВызватьМетод", "CallMethod")]
        public IValue CallMethod(IRuntimeContextInstance target, string methodName, ArrayImpl arguments = null)
        {
            var methodIdx = target.FindMethod(methodName);
            var methInfo = target.GetMethodInfo(methodIdx);

            var argsToPass = GetArgsToPass(arguments, methInfo);

            IValue retValue = ValueFactory.Create();
            if (methInfo.IsFunction)
            {
                target.CallAsFunction(methodIdx, argsToPass, out retValue);
            }
            else
            {
                target.CallAsProcedure(methodIdx, argsToPass);
            }

            if (arguments != null)
            {
                for (int i = 0; i < argsToPass.Length; i++)
                {
                    if (i < arguments.Count())
                    {
                        arguments.Set(i, argsToPass[i]?.GetRawValue());
                    }
                }
            }

            return retValue;
        }

        private static IValue[] GetArgsToPass(ArrayImpl arguments, MethodInfo methInfo)
        {
            var argsToPass = new List<IValue>();
            if (arguments != null)
            {
                argsToPass.AddRange(arguments);
            }

            if (methInfo.ArgCount < argsToPass.Count)
                throw RuntimeException.TooManyArgumentsPassed();

            for (int i = 0; i < argsToPass.Count; i++)
            {
                if (!methInfo.Params[i].IsByValue)
                    argsToPass[i] = Variable.Create(argsToPass[i], $"reflectorArg{i}");
            }
            while (argsToPass.Count < methInfo.ArgCount)
            {
                if (!methInfo.Params[argsToPass.Count].HasDefaultValue)
                {
                    throw RuntimeException.TooLittleArgumentsPassed();
                }
                argsToPass.Add(null);
            }

            return argsToPass.ToArray();
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
            
            var NameColumn = Result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var CountColumn = Result.Columns.Add("КоличествоПараметров", TypeDescription.IntegerType(), "Количество параметров");
            var IsFunctionColumn = Result.Columns.Add("ЭтоФункция", TypeDescription.BooleanType(), "Это функция");
            var DocumentationColumn = Result.Columns.Add("Документация", TypeDescription.StringType(), "Документация");

            foreach(var methInfo in target.GetMethods())
            {
                ValueTableRow new_row = Result.Add();
                new_row.Set(NameColumn, ValueFactory.Create(methInfo.Name));
                new_row.Set(CountColumn, ValueFactory.Create(methInfo.ArgCount));
                new_row.Set(IsFunctionColumn, ValueFactory.Create(methInfo.IsFunction));
                new_row.Set(DocumentationColumn, ValueFactory.Create(methInfo.Documentation ?? ""));
            }

            return Result;
        }

        /// <summary>
        /// Получает таблицу свойств для переданного объекта..
        /// </summary>
        /// <param name="target">Объект, из которого получаем таблицу свойств.</param>
        /// <returns>Таблица значений с 1 колонкой - Имя</returns>
        [ContextMethod("ПолучитьТаблицуСвойств", "GetPropertiesTable")]
        public ValueTable.ValueTable GetPropertiesTable(IRuntimeContextInstance target)
        {
            ValueTable.ValueTable Result = new ValueTable.ValueTable();

            var NameColumn = Result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var DocumentationColumn = Result.Columns.Add("Документация", TypeDescription.StringType(), "Документация");

            var SystemVarNames = new string[] { "этотобъект", "thisobject" };

            foreach (var propInfo in target.GetProperties())
            {
                if (SystemVarNames.Contains(propInfo.Identifier.ToLower())) continue;

                ValueTableRow new_row = Result.Add();
                new_row.Set(NameColumn, ValueFactory.Create(propInfo.Identifier));
                new_row.Set(DocumentationColumn, ValueFactory.Create(propInfo.Documentation ?? ""));
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
