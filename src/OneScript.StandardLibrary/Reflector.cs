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
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary
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
            var methodIdx = target.GetMethodNumber(methodName);
            var methInfo = target.GetMethodInfo(methodIdx);

            var argsToPass = GetArgsToPass(arguments, methInfo.GetParameters());

            IValue retValue = ValueFactory.Create();
            if (methInfo.IsFunction())
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

        private static IValue[] GetArgsToPass(ArrayImpl arguments, ParameterInfo[] parameters)
        {
            var argsToPass = new List<IValue>();
            if (arguments != null)
            {
                argsToPass.AddRange(arguments);
            }

            if (parameters.Length < argsToPass.Count)
                throw RuntimeException.TooManyArgumentsPassed();

            for (int i = 0; i < argsToPass.Count; i++)
            {
                if (parameters[i].IsByRef())
                    argsToPass[i] = Variable.Create(argsToPass[i], $"reflectorArg{i}");
            }
            while (argsToPass.Count < parameters.Length)
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
            if(target.GetRawValue() is BslObjectValue)
                return MethodExistsForObject(target.AsObject(), methodName);

            if (target.SystemType == BasicTypes.Type)
                return MethodExistsForType(target.GetRawValue() as BslTypeValue, methodName);

            throw RuntimeException.InvalidArgumentType("target");
        }

        private static bool MethodExistsForObject(IRuntimeContextInstance target, string methodName)
        {
            try
            {
                var idx = target.GetMethodNumber(methodName);
                return idx >= 0;
            }
            catch (RuntimeException)
            {
                return false;
            }
        }

        private static ValueTable EmptyAnnotationsTable()
        {
            var annotationsTable = new ValueTable();
            annotationsTable.Columns.Add("Имя");
            annotationsTable.Columns.Add("Параметры");

            return annotationsTable;
        }

        private static ValueTable CreateAnnotationTable(BslAnnotationAttribute[] annotations)
        {
            var annotationsTable = EmptyAnnotationsTable();
            var annotationNameColumn = annotationsTable.Columns.FindColumnByName("Имя");
            var annotationParamsColumn = annotationsTable.Columns.FindColumnByName("Параметры");

            foreach (var annotation in annotations)
            {
                var annotationRow = annotationsTable.Add();
                if (annotation.Name != null)
                {
                    annotationRow.Set(annotationNameColumn, ValueFactory.Create(annotation.Name));
                }
                if (annotation.Parameters.Any())
                {
                    var parametersTable = new ValueTable();
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
                        parameterRow.Set(parameterValueColumn, annotationParameter.Value);
                    }
                }
            }

            return annotationsTable;
        }

        private static bool MethodExistsForType(BslTypeValue type, string methodName)
        {
            var clrType = GetReflectableClrType(type);
            return clrType.GetMethod(methodName) != null;
        }

        private static Type GetReflectableClrType(BslTypeValue type)
        {
            var clrType = type.TypeValue.ImplementingClass;
            if(clrType != typeof(AttachedScriptsFactory) && !typeof(IRuntimeContextInstance).IsAssignableFrom(clrType))
            {
                throw NonReflectableType();
            }

            Type reflectableType;
            if (clrType == typeof(AttachedScriptsFactory))
                reflectableType = ReflectUserType(type.TypeValue.Name);
            else
                reflectableType = ReflectContext(clrType);

            return reflectableType;
        }

        private static RuntimeException NonReflectableType()
        {
            return RuntimeException.InvalidArgumentValue("Тип не может быть отражен.");
        }

        /// <summary>
        /// Получает таблицу методов для переданного объекта..
        /// </summary>
        /// <param name="target">Объект, из которого получаем таблицу методов.</param>
        /// <returns>Таблица значений колонками: Имя, Количество, ЭтоФункция, Аннотации</returns>
        [ContextMethod("ПолучитьТаблицуМетодов", "GetMethodsTable")]
        public ValueTable GetMethodsTable(IValue target)
        {
            var result = new ValueTable();
            if(target.GetRawValue() is BslObjectValue)
                FillMethodsTableForObject(target.AsObject(), result);
            else if (target.SystemType == BasicTypes.Type)
                FillMethodsTableForType(target.GetRawValue() as BslTypeValue, result);
            else
                throw RuntimeException.InvalidArgumentType();

            return result;
        }

        private static void FillMethodsTableForObject(IRuntimeContextInstance target, ValueTable result)
        {
            FillMethodsTable(result, target.GetMethods());
        }

        private static void FillMethodsTableForType(BslTypeValue type, ValueTable result)
        {
            var clrType = GetReflectableClrType(type);
            var clrMethods = clrType.GetMethods(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
            FillMethodsTable(result, clrMethods.Select(x=>new Contexts.ContextMethodInfo(x)));
        }

        private void FillPropertiesTableForObject(ValueTable result, IValue target)
        {
            if (target is ScriptDrivenObject scriptObject)
            {
                var fields = scriptObject.Module.Fields
                    .Cast<BslScriptFieldInfo>()
                    .Select(field => BslPropertyBuilder.Create()
                        .Name(field.Name)
                        .IsExported(field.IsPublic)
                        .SetAnnotations(field.GetAnnotations())
                        .SetDispatchingIndex(field.DispatchId)
                        .Build()
                    ).OrderBy(p => p.DispatchId)
                    .ToArray();

                var fieldNames = fields.Select(x => x.Name)
                    .ToHashSet();
                
                var properties = scriptObject.GetProperties()
                    .Where(prop => !fieldNames.Contains(prop.Name));
                
                FillPropertiesTable(result, properties.Concat(fields));
            }
            else
            {
                var objectProperties = target.AsObject().GetProperties();
                FillPropertiesTable(result, objectProperties);
            }
        }
        
        private static void FillPropertiesTableForType(BslTypeValue type, ValueTable result)
        {
            var clrType = GetReflectableClrType(type);
            var nativeProps = clrType.GetProperties()
                                     .Select(x => new
                                     {
                                         PropDef = x.GetCustomAttribute<ContextPropertyAttribute>(),
                                         Prop = x
                                     })
                                     .Where(x=>x.PropDef != null)
                                     .Select(x => new ContextPropertyInfo(x.Prop));

            var infos = new List<BslPropertyInfo>();

            infos.AddRange(nativeProps);
            int indices = infos.Count;

            if (typeof(ScriptDrivenObject).IsAssignableFrom(clrType.BaseType))
            {
                var nativeFields = clrType.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
                foreach(var field in nativeFields)
                {
                    var prop = BslPropertyBuilder.Create()
                        .Name(field.Name)
                        .IsExported(field.IsPublic)
                        .SetDispatchingIndex(indices++)
                        .Build();
                    
                    infos.Add(prop);
                }
            }

            FillPropertiesTable(result, infos);

        }

        private static void FillMethodsTable(ValueTable result, IEnumerable<BslMethodInfo> methods)
        {
            var nameColumn = result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var countColumn = result.Columns.Add("КоличествоПараметров", TypeDescription.IntegerType(), "Количество параметров");
            var isFunctionColumn = result.Columns.Add("ЭтоФункция", TypeDescription.BooleanType(), "Это функция");
            var annotationsColumn = result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
            var paramsColumn = result.Columns.Add("Параметры", new TypeDescription(), "Параметры");
            var isExportlColumn = result.Columns.Add("Экспорт", new TypeDescription(), "Экспорт");

            foreach (var methInfo in methods)
            {
                var annotations = methInfo.GetAnnotations();
                var parameters = methInfo.GetParameters();
                
                ValueTableRow new_row = result.Add();
                new_row.Set(nameColumn, ValueFactory.Create(methInfo.Name));
                new_row.Set(countColumn, ValueFactory.Create(parameters.Length));
                new_row.Set(isFunctionColumn, ValueFactory.Create(methInfo.IsFunction()));
                new_row.Set(isExportlColumn, ValueFactory.Create(methInfo.IsPublic));

                new_row.Set(annotationsColumn, CreateAnnotationTable(annotations));

                var paramTable = new ValueTable();
                var paramNameColumn = paramTable.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
                var paramByValue = paramTable.Columns.Add("ПоЗначению", TypeDescription.BooleanType(), "По значению");
                var paramHasDefaultValue = paramTable.Columns.Add("ЕстьЗначениеПоУмолчанию", TypeDescription.BooleanType(), "Есть значение по-умолчанию");
                var paramDefaultValue = paramTable.Columns.Add("ЗначениеПоУмолчанию", new TypeDescription(), "Значение по умолчанию");
                var paramAnnotationsColumn = paramTable.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");

                new_row.Set(paramsColumn, paramTable);

                if (parameters.Length != 0)
                {
                    var index = 0;
                    foreach (var param in parameters)
                    {
                        var name = param.Name ?? $"param{++index}";
                        var paramRow = paramTable.Add();
                        var defaultValue = param.DefaultValue;
                        paramRow.Set(paramNameColumn, ValueFactory.Create(name));
                        paramRow.Set(paramByValue, ValueFactory.Create(!param.IsByRef()));
                        paramRow.Set(paramHasDefaultValue, ValueFactory.Create(param.HasDefaultValue));
                        paramRow.Set(paramDefaultValue, ValueFactory.Create(param.DefaultValue.ToString()));
                        paramRow.Set(paramAnnotationsColumn, CreateAnnotationTable(param.GetAnnotations()));
                    }
                }
            }
        }

        /// <summary>
        /// Получает таблицу свойств для переданного объекта..
        /// </summary>
        /// <param name="target">Объект, из которого получаем таблицу свойств.</param>
        /// <returns>Таблица значений с колонками - Имя, Аннотации</returns>
        [ContextMethod("ПолучитьТаблицуСвойств", "GetPropertiesTable")]
        public ValueTable GetPropertiesTable(IValue target)
        {
            var result = new ValueTable();

            if(target.GetRawValue() is BslObjectValue)
                FillPropertiesTableForObject(result, target);
            else if (target.SystemType == BasicTypes.Type)
            {
                var type = target.GetRawValue() as BslTypeValue;
                FillPropertiesTableForType(type, result);
            }
            else
                throw RuntimeException.InvalidArgumentType();

            return result;
        }

        /// <summary>
        /// Получает свойство по его имени.
        /// </summary>
        /// <param name="target">Объект, свойство которого необходимо установить.</param>
        /// <param name="prop">Имя свойства</param>
        /// <returns>Значение свойства</returns>
        [ContextMethod("ПолучитьСвойство", "GetProperty")]
        public IValue GetProperty(IRuntimeContextInstance target, string prop)
        {
            int propIdx;
            if (target is ScriptDrivenObject script)
                propIdx = script.FindAnyProperty(prop);
            else
                propIdx = target.GetPropertyNumber(prop);
            return target.GetPropValue(propIdx);
        }

        /// <summary>
        /// Устанавливает свойство по его имени.
        /// </summary>
        /// <param name="target">Объект, свойство которого необходимо установить.</param>
        /// <param name="prop">Имя свойства</param>
        /// <param name="value">Значение свойства.</param>
        [ContextMethod("УстановитьСвойство", "SetProperty")]
        public void SetProperty(IRuntimeContextInstance target, string prop, IValue value)
        {
            int propIdx;
            if (target is ScriptDrivenObject script)
                propIdx = script.FindAnyProperty(prop);
            else
                propIdx = target.GetPropertyNumber(prop);

            if (target.IsPropWritable(propIdx))
                target.SetPropValue(propIdx, value);
            else
                throw PropertyAccessException.PropIsNotWritableException(prop);
        }

        private static void FillPropertiesTable(ValueTable result, IEnumerable<BslPropertyInfo> properties)
        {
            var nameColumn = result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var annotationsColumn = result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
            var isExportedColumn = result.Columns.Add("Экспорт", TypeDescription.BooleanType(), "Экспорт");
            
            var systemVarNames = new string[] { "этотобъект", "thisobject" };

            foreach (var propInfo in properties)
            {
                if (systemVarNames.Contains(propInfo.Name.ToLower())) continue;

                ValueTableRow new_row = result.Add();
                new_row.Set(nameColumn, ValueFactory.Create(propInfo.Name));

                var annotations = propInfo.GetAnnotations();
                new_row.Set(annotationsColumn, annotations.Length != 0 ? CreateAnnotationTable(annotations) : EmptyAnnotationsTable());

                if (propInfo is BslScriptPropertyInfo scriptProp)
                {
                    new_row.Set(isExportedColumn, BslBooleanValue.Create(scriptProp.IsExported));
                }
                else
                {
                    new_row.Set(isExportedColumn, BslBooleanValue.Create(propInfo.CanRead));
                }
            }
        }

        public static Type ReflectUserType(string typeName)
        {
            IExecutableModule module;
            try
            {
                module = AttachedScriptsFactory.GetModuleOfType(typeName);
            }
            catch (KeyNotFoundException)
            {
                throw NonReflectableType();
            }

            var builder = new ClassBuilder(typeof(UserScriptContextInstance));

            return builder
                   .SetTypeName(typeName)
                   .SetModule(module)
                   .ExportDefaults()
                   .Build();
        }

        public static Type ReflectContext(Type clrType)
        {
            var attrib = clrType.GetCustomAttribute<ContextClassAttribute>();
            if (attrib == null || !typeof(ContextIValueImpl).IsAssignableFrom(clrType))
                throw NonReflectableType();

            var builder = new ClassBuilder(clrType);

            return builder.SetTypeName(attrib.GetName())
                   .ExportDefaults()
                   .Build();
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance CreateNew()
        {
            return new ReflectorContext();
        }
    }
}
