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
using OneScript.StandardLibrary.Collections;
using OneScript.StandardLibrary.Collections.ValueTable;
using OneScript.StandardLibrary.TypeDescriptions;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Reflection;

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

        private static IValue[] GetArgsToPass(ArrayImpl arguments, MethodSignature signature)
        {
            var argsToPass = new List<IValue>();
            if (arguments != null)
            {
                argsToPass.AddRange(arguments);
            }

            if (signature.ArgCount < argsToPass.Count)
                throw RuntimeException.TooManyArgumentsPassed();

            for (int i = 0; i < argsToPass.Count; i++)
            {
                if (!signature.Params[i].IsByValue)
                    argsToPass[i] = Variable.Create(argsToPass[i], $"reflectorArg{i}");
            }
            while (argsToPass.Count < signature.ArgCount)
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
                var idx = target.FindMethod(methodName);
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

        private static ValueTable CreateAnnotationTable(AnnotationDefinition[] annotations)
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
                if (annotation.ParamCount != 0)
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
                        parameterRow.Set(parameterValueColumn, annotationParameter.RuntimeValue);
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
            FillMethodsTable(result, ConvertToOsMethods(clrMethods));
        }

        private static IEnumerable<MethodSignature> ConvertToOsMethods(IEnumerable<System.Reflection.MethodInfo> source)
        {
            var dest = new List<MethodSignature>();
            foreach (var methodInfo in source)
            {
                var osMethod = new MethodSignature();
                osMethod.Name = methodInfo.Name;
                osMethod.Alias = null;
                osMethod.IsExport = methodInfo.IsPublic;
                osMethod.IsFunction = methodInfo.ReturnType != typeof(void);
                osMethod.Annotations = GetAnnotations(methodInfo.GetCustomAttributes<UserAnnotationAttribute>());

                var methodParameters = methodInfo.GetParameters();
                var osParams = new ParameterDefinition[methodParameters.Length];
                osMethod.Params = osParams;
                for (int i = 0; i < osParams.Length; i++)
                {
                    var parameterInfo = methodParameters[i];
                    var osParam = new ParameterDefinition();
                    osParam.Name = parameterInfo.Name;
                    osParam.IsByValue = parameterInfo.GetCustomAttribute<ByRefAttribute>() != null;
                    osParam.HasDefaultValue = parameterInfo.HasDefaultValue;
                    osParam.DefaultValueIndex = -1;

                    // On Mono 5.20 we can't use GetCustomAttributes<T> because it fails with InvalidCast.
                    // Here's a workaround with home-made attribute Type filter.
                    var attributes = parameterInfo.GetCustomAttributes()
                        .OfType<UserAnnotationAttribute>();
                    
                    osParam.Annotations = GetAnnotations(attributes);
                    osParams[i] = osParam;
                }
                dest.Add(osMethod);
            }

            return dest;
        }

        private static AnnotationDefinition[] GetAnnotations(IEnumerable<UserAnnotationAttribute> attributes)
        {
            return attributes.Select(x => x.Annotation).ToArray();
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
                                     .Where(x=>x.PropDef != null);

            int indices = 0;
            var infos = new List<VariableInfo>();
            foreach(var prop in nativeProps)
            {
                var info = new VariableInfo();
                info.Type = SymbolType.ContextProperty;
                info.Index = indices++;
                info.Identifier = prop.PropDef.GetName();
                info.Annotations = GetAnnotations(prop.Prop.GetCustomAttributes<UserAnnotationAttribute>());
                infos.Add(info);
            }

            if (clrType.BaseType == typeof(ScriptDrivenObject))
            {
                var nativeFields = clrType.GetFields();
                foreach(var field in nativeFields)
                {
                    var info = new VariableInfo();
                    info.Type = SymbolType.ContextProperty;
                    info.Index = indices++;
                    info.Identifier = field.Name;
                    info.Annotations = GetAnnotations(field.GetCustomAttributes<UserAnnotationAttribute>());
                    infos.Add(info);
                }
            }

            FillPropertiesTable(result, infos);

        }

        private static void FillMethodsTable(ValueTable result, IEnumerable<MethodSignature> methods)
        {
            var nameColumn = result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var countColumn = result.Columns.Add("КоличествоПараметров", TypeDescription.IntegerType(), "Количество параметров");
            var isFunctionColumn = result.Columns.Add("ЭтоФункция", TypeDescription.BooleanType(), "Это функция");
            var annotationsColumn = result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
            var paramsColumn = result.Columns.Add("Параметры", new TypeDescription(), "Параметры");
            var isExportlColumn = result.Columns.Add("Экспорт", new TypeDescription(), "Экспорт");

            foreach (var methInfo in methods)
            {
                
                ValueTableRow new_row = result.Add();
                new_row.Set(nameColumn, ValueFactory.Create(methInfo.Name));
                new_row.Set(countColumn, ValueFactory.Create(methInfo.ArgCount));
                new_row.Set(isFunctionColumn, ValueFactory.Create(methInfo.IsFunction));
                new_row.Set(isExportlColumn, ValueFactory.Create(methInfo.IsExport));

                new_row.Set(annotationsColumn, methInfo.AnnotationsCount != 0 ? CreateAnnotationTable(methInfo.Annotations) : EmptyAnnotationsTable());

                var paramTable = new ValueTable();
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
                        paramRow.Set(paramAnnotationsColumn, param.AnnotationsCount != 0 ? CreateAnnotationTable(param.Annotations) : EmptyAnnotationsTable());
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
                FillPropertiesTable(result, target.AsObject().GetProperties());
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
                propIdx = target.FindProperty(prop);
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
                propIdx = target.FindProperty(prop);
            target.SetPropValue(propIdx, value);
        }

        private static void FillPropertiesTable(ValueTable result, IEnumerable<VariableInfo> properties)
        {
            var nameColumn = result.Columns.Add("Имя", TypeDescription.StringType(), "Имя");
            var annotationsColumn = result.Columns.Add("Аннотации", new TypeDescription(), "Аннотации");
            var systemVarNames = new string[] { "этотобъект", "thisobject" };

            foreach (var propInfo in properties)
            {
                if (systemVarNames.Contains(propInfo.Identifier.ToLower())) continue;

                ValueTableRow new_row = result.Add();
                new_row.Set(nameColumn, ValueFactory.Create(propInfo.Identifier));

                new_row.Set(annotationsColumn, propInfo.AnnotationsCount != 0 ? CreateAnnotationTable(propInfo.Annotations) : EmptyAnnotationsTable());
            }
        }

        public static Type ReflectUserType(string typeName)
        {
            LoadedModule module;
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
