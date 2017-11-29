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

        private ValueTable.ValueTable CreateAnnotationTable(AnnotationDefinition[] annotations)
        {
            var annotationsTable = new ValueTable.ValueTable();
            var annotationNameColumn = annotationsTable.Columns.Add("Имя");
            var annotationParamsColumn = annotationsTable.Columns.Add("Параметры");

            foreach (var annotation in annotations)
            {
                var annotationRow = annotationsTable.Add();
                if (annotation.Name != null)
                {
                    annotationRow.Set(annotationNameColumn, ValueFactory.Create(annotation.Name));
                }
                if (annotation.ParamCount != 0)
                {
                    var parametersTable = new ValueTable.ValueTable();
                    var parameterNameColumn = parametersTable.Columns.Add("Имя");
                    var parameterValueColumn = parametersTable.Columns.Add("Значение");
                            
                    annotationRow.Set(annotationParamsColumn, parametersTable);
                            
                    foreach (var annotationParameter in annotation.Parameters)
                    {
                        var parameterRow = parametersTable.Add();
                        if (annotationParameter.Name != null)
                        {
                            parameterRow.Set(parameterNameColumn, ValueFactory.Create(annotationParameter.Name));
                        }
                        parameterRow.Set(parameterValueColumn, annotationParameter.RuntimeValue);
                    }
                }
            }

            return annotationsTable;
        }

        /// <summary>
        /// Получает таблицу методов для переданного объекта..
        /// </summary>
        /// <param name="target">Объект, из которого получаем таблицу методов.</param>
        /// <returns>Таблица значений колонками:
        /// <list type="bullet">
        ///     <item><term>Имя</term><description> - Строка</description></item>
        ///     <item><term>КоличествоПараметров</term><description> - Число</description></item>
        ///     <item><term>ЭтоФункция</term><description> - Булево</description></item>
        ///     <item><term>Аннотации</term><description> - Неопределено, ТаблицаЗначений:
        ///         <list type="bullet">
        ///             <item><term>Имя</term></item>
        ///             <item><term>Параметры</term></item>
        ///         </list>
        ///     </description></item>
        /// </list></returns>
        [ContextMethod("ПолучитьТаблицуМетодов", "GetMethodsTable")]
        public ValueTable.ValueTable GetMethodsTable(IRuntimeContextInstance target)
        {
            ValueTable.ValueTable Result = new ValueTable.ValueTable();

            var NameColumn = Result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var CountColumn = Result.Columns.Add("КоличествоПараметров", TypeDescription.IntegerType(), "Количество параметров");
            var IsFunctionColumn = Result.Columns.Add("ЭтоФункция", TypeDescription.BooleanType(), "Это функция");
            var AnnotationsColumn = Result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
            var ParamsColumn = Result.Columns.Add("Параметры", new TypeDescription(), "Параметры");

            foreach(var methInfo in target.GetMethods())
            {
                ValueTableRow new_row = Result.Add();
                new_row.Set(NameColumn, ValueFactory.Create(methInfo.Name));
                new_row.Set(CountColumn, ValueFactory.Create(methInfo.ArgCount));
                new_row.Set(IsFunctionColumn, ValueFactory.Create(methInfo.IsFunction));

                if (methInfo.AnnotationsCount != 0)
                {
                    new_row.Set(AnnotationsColumn, CreateAnnotationTable(methInfo.Annotations));
                }
                
                var paramTable = new ValueTable.ValueTable();
                var paramNameColumn = paramTable.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
                var paramByValue = paramTable.Columns.Add("ПоЗначению", TypeDescription.BooleanType(), "По значению");
                var paramHasDefaultValue = paramTable.Columns.Add("ЕстьЗначениеПоУмолчанию", TypeDescription.BooleanType(), "Есть значение по-умолчанию");
                var paramAnnotationsColumn = paramTable.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
                
                new_row.Set(ParamsColumn, paramTable);

                if (methInfo.ArgCount != 0)
                {
                    var index = 0;
                    foreach (var param in methInfo.Params)
                    {
                        var name = string.Format("param{0}", ++index);
                        var paramRow = paramTable.Add();
                        paramRow.Set(paramNameColumn, ValueFactory.Create(name));
                        paramRow.Set(paramByValue, ValueFactory.Create(param.IsByValue));
                        paramRow.Set(paramHasDefaultValue, ValueFactory.Create(param.HasDefaultValue));
                        if (param.AnnotationsCount != 0)
                        {
                            paramRow.Set(paramAnnotationsColumn, CreateAnnotationTable(param.Annotations));
                        }
                    }
                }
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
            var AnnotationsColumn = Result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");

            var SystemVarNames = new string[] { "этотобъект", "thisobject" };

            foreach (var propInfo in target.GetProperties())
            {
                if (SystemVarNames.Contains(propInfo.Identifier.ToLower())) continue;

                ValueTableRow new_row = Result.Add();
                new_row.Set(NameColumn, ValueFactory.Create(propInfo.Identifier));

                if (propInfo.AnnotationsCount != 0)
                {
                    new_row.Set(AnnotationsColumn, CreateAnnotationTable(propInfo.Annotations));
                }
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
