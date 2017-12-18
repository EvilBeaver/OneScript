/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.HostedScript.Library.ValueTable;

using MethodInfo = ScriptEngine.Machine.MethodInfo;

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
        public bool MethodExists(IValue target, string methodName)
        {
            if(target.DataType == DataType.Object)
                return MethodExistsForObject(target.AsObject(), methodName);

            if (target.DataType == DataType.Type)
                return MethodExistsForType(target.GetRawValue() as TypeTypeValue, methodName);

            throw RuntimeException.InvalidArgumentType("target");
        }

        private static bool MethodExistsForObject(IRuntimeContextInstance target, string methodName)
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

        private static ValueTable.ValueTable CreateAnnotationTable(AnnotationDefinition[] annotations)
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

        private static bool MethodExistsForType(TypeTypeValue type, string methodName)
        {
            var clrType = GetReflectableClrType(type);
            var mapper = CreateMethodsMapper(clrType);

            var actualType = mapper.GetType();
            int result = (int)actualType.InvokeMember("FindMethod", 
                BindingFlags.InvokeMethod,
                null,
                mapper,
                new object[]{methodName});
            return result >= 0;
        }

        private static object CreateMethodsMapper(Type clrType)
        {
            var mapperType = typeof(ContextMethodsMapper<>).MakeGenericType(clrType);
            var instance = Activator.CreateInstance(mapperType);
            return instance;
        }

        private static dynamic CreatePropertiesMapper(Type clrType)
        {
            var mapperType = typeof(ContextPropertyMapper<>).MakeGenericType(clrType);
            var instance = Activator.CreateInstance(mapperType);
            dynamic magicCaller = instance; // зачем строить ExpressionTree, когда есть dynamic
            return magicCaller;
        }

        private static Type GetReflectableClrType(TypeTypeValue type)
        {
            Type clrType;
            try
            {
                clrType = TypeManager.GetImplementingClass(type.Value.ID);
            }
            catch (InvalidOperationException)
            {
                throw RuntimeException.InvalidArgumentValue("Тип не может быть отражен.");
            }

            var attrs = clrType.GetCustomAttributes(typeof(ContextClassAttribute), false).ToArray();
            if (attrs.Length == 0)
                throw RuntimeException.InvalidArgumentValue("Тип не может быть отражен.");

            return clrType;
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
        public ValueTable.ValueTable GetMethodsTable(IValue target)
        {
            var result = new ValueTable.ValueTable();
            if(target.DataType == DataType.Object)
                FillMethodsTableForObject(target.AsObject(), result);
            else if (target.DataType == DataType.Type)
                FillMethodsTableForType(target.GetRawValue() as TypeTypeValue, result);
            else
                throw RuntimeException.InvalidArgumentType();

            return result;
        }

        private static void FillMethodsTableForObject(IRuntimeContextInstance target, ValueTable.ValueTable result)
        {
            FillMethodsTable(result, target.GetMethods());
        }

        private static void FillMethodsTableForType(TypeTypeValue type, ValueTable.ValueTable result)
        {
            var clrType = GetReflectableClrType(type);
            var mapper = CreateMethodsMapper(clrType);
            var actualType = mapper.GetType();
            var infos = (IEnumerable<MethodInfo>)actualType.InvokeMember("GetMethods",
                                                      BindingFlags.InvokeMethod,
                                                      null,
                                                      mapper,
                                                      new object[0]);
            FillMethodsTable(result, infos);
        }
        
        private static void FillMethodsTable(ValueTable.ValueTable result, IEnumerable<MethodInfo> methods)
        {
            var nameColumn = result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var countColumn = result.Columns.Add("КоличествоПараметров", TypeDescription.IntegerType(), "Количество параметров");
            var isFunctionColumn = result.Columns.Add("ЭтоФункция", TypeDescription.BooleanType(), "Это функция");
            var annotationsColumn = result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
            var paramsColumn = result.Columns.Add("Параметры", new TypeDescription(), "Параметры");

            foreach (var methInfo in methods)
            {
                ValueTableRow new_row = result.Add();
                new_row.Set(nameColumn, ValueFactory.Create(methInfo.Name));
                new_row.Set(countColumn, ValueFactory.Create(methInfo.ArgCount));
                new_row.Set(isFunctionColumn, ValueFactory.Create(methInfo.IsFunction));

                if (methInfo.AnnotationsCount != 0)
                {
                    new_row.Set(annotationsColumn, CreateAnnotationTable(methInfo.Annotations));
                }
                
                var paramTable = new ValueTable.ValueTable();
                var paramNameColumn = paramTable.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
                var paramByValue = paramTable.Columns.Add("ПоЗначению", TypeDescription.BooleanType(), "По значению");
                var paramHasDefaultValue = paramTable.Columns.Add("ЕстьЗначениеПоУмолчанию", TypeDescription.BooleanType(), "Есть значение по-умолчанию");
                var paramAnnotationsColumn = paramTable.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
                
                new_row.Set(paramsColumn, paramTable);

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
        }

        /// <summary>
        /// Получает таблицу свойств для переданного объекта..
        /// </summary>
        /// <param name="target">Объект, из которого получаем таблицу свойств.</param>
        /// <returns>Таблица значений с 1 колонкой - Имя</returns>
        [ContextMethod("ПолучитьТаблицуСвойств", "GetPropertiesTable")]
        public ValueTable.ValueTable GetPropertiesTable(IValue target)
        {
            ValueTable.ValueTable result = new ValueTable.ValueTable();

            if(target.DataType == DataType.Object)
                FillPropertiesTable(result, target.AsObject().GetProperties());
            else if (target.DataType == DataType.Type)
            {
                var type = target.GetRawValue() as TypeTypeValue;
                var clrType = GetReflectableClrType(type);
                var magicCaller = CreatePropertiesMapper(clrType);
                FillPropertiesTable(result, magicCaller.GetProperties());
            }
            else
                throw RuntimeException.InvalidArgumentType();

            return result;
        }

        private void FillPropertiesTable(ValueTable.ValueTable result, IEnumerable<VariableInfo> properties)
        {
            var nameColumn = result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var annotationsColumn = result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
            var systemVarNames = new string[] { "этотобъект", "thisobject" };

            foreach (var propInfo in properties)
            {
                if (systemVarNames.Contains(propInfo.Identifier.ToLower())) continue;

                ValueTableRow new_row = result.Add();
                new_row.Set(nameColumn, ValueFactory.Create(propInfo.Identifier));

                if (propInfo.AnnotationsCount != 0)
                {
                    new_row.Set(annotationsColumn, CreateAnnotationTable(propInfo.Annotations));
                }
            }
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance CreateNew()
        {
            return new ReflectorContext();
        }
    }
}
