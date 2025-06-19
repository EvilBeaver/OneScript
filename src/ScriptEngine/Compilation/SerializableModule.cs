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
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine.Machine;

namespace ScriptEngine.Compilation
{
    /// <summary>
    /// Сериализуемая версия IExecutableModule для кэширования
    /// </summary>
    [Serializable]
    public class SerializableModule
    {
        public int FormatVersion { get; set; } = 1;
        
        // Основные компоненты модуля
        public SerializableConstant[] Constants { get; set; }
        public SymbolBinding[] VariableRefs { get; set; }
        public SymbolBinding[] MethodRefs { get; set; }
        public Command[] Code { get; set; }
        
        // Метаданные модуля  
        public int EntryMethodIndex { get; set; } = -1;
        public SerializableSourceCode Source { get; set; }
        
        // Члены модуля
        public SerializableAnnotation[] ModuleAttributes { get; set; }
        public SerializableFieldInfo[] Fields { get; set; }
        public SerializablePropertyInfo[] Properties { get; set; }
        public SerializableMethodInfo[] Methods { get; set; }

        /// <summary>
        /// Создать SerializableModule из StackRuntimeModule
        /// </summary>
        public static SerializableModule FromExecutableModule(IExecutableModule module)
        {
            if (!(module is StackRuntimeModule stackModule))
            {
                throw new ArgumentException("Only StackRuntimeModule is supported for serialization", nameof(module));
            }

            var serializable = new SerializableModule
            {
                EntryMethodIndex = stackModule.EntryMethodIndex,
                Constants = stackModule.Constants.Select(SerializableConstant.FromBslValue).ToArray(),
                VariableRefs = stackModule.VariableRefs.ToArray(),
                MethodRefs = stackModule.MethodRefs.ToArray(),
                Code = stackModule.Code.ToArray(),
                Source = SerializableSourceCode.FromSourceCode(stackModule.Source),
                
                ModuleAttributes = stackModule.ModuleAttributes?.Select(SerializableAnnotation.FromAnnotation).ToArray() ?? new SerializableAnnotation[0],
                Fields = stackModule.Fields?.Select(SerializableFieldInfo.FromFieldInfo).ToArray() ?? new SerializableFieldInfo[0],
                Properties = stackModule.Properties?.Select(SerializablePropertyInfo.FromPropertyInfo).ToArray() ?? new SerializablePropertyInfo[0],
                Methods = stackModule.Methods?.Select(SerializableMethodInfo.FromMethodInfo).ToArray() ?? new SerializableMethodInfo[0]
            };

            return serializable;
        }

        /// <summary>
        /// Восстановить StackRuntimeModule из сериализованных данных
        /// </summary>
        public StackRuntimeModule ToExecutableModule()
        {
            var module = new StackRuntimeModule(typeof(object)) // TODO: Determine proper type
            {
                EntryMethodIndex = this.EntryMethodIndex,
                Source = this.Source?.ToSourceCode()
            };

            // Восстанавливаем константы
            foreach (var constant in Constants ?? new SerializableConstant[0])
            {
                module.Constants.Add(constant.ToBslValue());
            }

            // Восстанавливаем ссылки
            foreach (var varRef in VariableRefs ?? new SymbolBinding[0])
            {
                module.VariableRefs.Add(varRef);
            }

            foreach (var methodRef in MethodRefs ?? new SymbolBinding[0])
            {
                module.MethodRefs.Add(methodRef);
            }

            // Восстанавливаем код
            foreach (var command in Code ?? new Command[0])
            {
                module.Code.Add(command);
            }

            // Восстанавливаем атрибуты модуля
            foreach (var attr in ModuleAttributes ?? new SerializableAnnotation[0])
            {
                module.ModuleAttributes.Add(attr.ToAnnotation());
            }

            // Восстанавливаем поля
            foreach (var field in Fields ?? new SerializableFieldInfo[0])
            {
                module.Fields.Add(field.ToFieldInfo());
            }

            // Восстанавливаем свойства
            foreach (var prop in Properties ?? new SerializablePropertyInfo[0])
            {
                module.Properties.Add(prop.ToPropertyInfo());
            }

            // Восстанавливаем методы
            foreach (var method in Methods ?? new SerializableMethodInfo[0])
            {
                module.Methods.Add(method.ToMethodInfo());
            }

            return module;
        }
    }

    /// <summary>
    /// Сериализуемая константа
    /// </summary>
    [Serializable]
    public class SerializableConstant
    {
        public string Type { get; set; }
        public string Value { get; set; }

        public static SerializableConstant FromBslValue(BslPrimitiveValue value)
        {
            return new SerializableConstant
            {
                Type = value.GetType().Name,
                Value = value.ToString()
            };
        }

        public BslPrimitiveValue ToBslValue()
        {
            switch (Type)
            {
                case nameof(BslStringValue):
                    return BslStringValue.Create(Value ?? "");
                case nameof(BslNumericValue):
                    return BslNumericValue.Create(decimal.TryParse(Value, out var num) ? num : 0);
                case nameof(BslBooleanValue):
                    return (BslPrimitiveValue)BslBooleanValue.Create(bool.TryParse(Value, out var boolVal) && boolVal);
                case nameof(BslUndefinedValue):
                    return BslUndefinedValue.Instance;
                case nameof(BslNullValue):
                    return BslNullValue.Instance;
                default:
                    // Fallback to string for unknown types
                    return BslStringValue.Create(Value ?? "");
            }
        }
    }

    /// <summary>
    /// Сериализуемый исходный код
    /// </summary>
    [Serializable]
    public class SerializableSourceCode
    {
        public string Location { get; set; }

        public static SerializableSourceCode FromSourceCode(SourceCode source)
        {
            if (source == null) return null;
            
            return new SerializableSourceCode
            {
                Location = source.Location
            };
        }

        public SourceCode ToSourceCode()
        {
            // Для кэширования нам не нужно полностью восстанавливать SourceCode,
            // поскольку восстановленный модуль будет использоваться в контексте,
            // где исходный Source уже не так важен
            return null;
        }
    }

    /// <summary>
    /// Сериализуемая аннотация
    /// </summary>
    [Serializable]
    public class SerializableAnnotation
    {
        public string Name { get; set; }
        public SerializableAnnotationParameter[] Parameters { get; set; }

        public static SerializableAnnotation FromAnnotation(BslAnnotationAttribute annotation)
        {
            return new SerializableAnnotation
            {
                Name = annotation.Name,
                Parameters = annotation.Parameters?.Select(SerializableAnnotationParameter.FromParameter).ToArray() ?? new SerializableAnnotationParameter[0]
            };
        }

        public BslAnnotationAttribute ToAnnotation()
        {
            var parameters = Parameters?.Select(p => p.ToParameter()).ToArray() ?? new BslAnnotationParameter[0];
            return new BslAnnotationAttribute(Name, parameters);
        }
    }

    /// <summary>
    /// Сериализуемый параметр аннотации
    /// </summary>
    [Serializable]
    public class SerializableAnnotationParameter
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public static SerializableAnnotationParameter FromParameter(BslAnnotationParameter parameter)
        {
            return new SerializableAnnotationParameter
            {
                Name = parameter.Name,
                Value = parameter.Value?.ToString() ?? ""
            };
        }

        public BslAnnotationParameter ToParameter()
        {
            return new BslAnnotationParameter(Name, BslStringValue.Create(Value ?? ""));
        }
    }

    /// <summary>
    /// Сериализуемая информация о поле
    /// </summary>
    [Serializable]
    public class SerializableFieldInfo
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsExport { get; set; }
        public int DispatchId { get; set; }

        public static SerializableFieldInfo FromFieldInfo(BslScriptFieldInfo field)
        {
            return new SerializableFieldInfo
            {
                Name = field.Name,
                Alias = field.Alias,
                IsExport = (field.Attributes & FieldAttributes.Public) == FieldAttributes.Public,
                DispatchId = field.DispatchId
            };
        }

        public BslScriptFieldInfo ToFieldInfo()
        {
            return BslFieldBuilder.Create()
                .Name(Name)
                .Alias(Alias)
                .IsExported(IsExport)
                .SetDispatchingIndex(DispatchId)
                .Build();
        }
    }

    /// <summary>
    /// Сериализуемая информация о свойстве
    /// </summary>
    [Serializable]
    public class SerializablePropertyInfo
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsExport { get; set; }
        public int DispatchId { get; set; }
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }

        public static SerializablePropertyInfo FromPropertyInfo(BslScriptPropertyInfo prop)
        {
            return new SerializablePropertyInfo
            {
                Name = prop.Name,
                Alias = prop.Alias,
                IsExport = prop.IsExported,
                DispatchId = prop.DispatchId,
                CanRead = prop.CanRead,
                CanWrite = prop.CanWrite
            };
        }

        public BslScriptPropertyInfo ToPropertyInfo()
        {
            return BslPropertyBuilder.Create()
                .Name(Name)
                .Alias(Alias)
                .IsExported(IsExport)
                .SetDispatchingIndex(DispatchId)
                .CanRead(CanRead)
                .CanWrite(CanWrite)
                .Build();
        }
    }

    /// <summary>
    /// Сериализуемая информация о методе
    /// </summary>
    [Serializable]
    public class SerializableMethodInfo
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public bool IsExport { get; set; }
        public int DispatchId { get; set; }
        public SerializableParameterInfo[] Parameters { get; set; }

        public static SerializableMethodInfo FromMethodInfo(BslScriptMethodInfo method)
        {
            return new SerializableMethodInfo
            {
                Name = method.Name,
                Alias = method.Alias,
                IsExport = (method.Attributes & MethodAttributes.Public) == MethodAttributes.Public,
                DispatchId = method.DispatchId,
                Parameters = method.GetBslParameters()?.Select(SerializableParameterInfo.FromParameterInfo).ToArray() ?? new SerializableParameterInfo[0]
            };
        }

        public BslScriptMethodInfo ToMethodInfo()
        {
            var builder = BslMethodBuilder.Create()
                .Name(Name)
                .Alias(Alias)
                .IsExported(IsExport)
                .SetDispatchingIndex(DispatchId);
            
            // Добавляем параметры
            foreach (var param in Parameters ?? new SerializableParameterInfo[0])
            {
                var paramBuilder = builder.NewParameter()
                    .Name(param.Name)
                    .ByValue(!param.IsByRef);
                
                if (param.HasDefaultValue)
                {
                    paramBuilder.DefaultValue(BslStringValue.Create(param.DefaultValue ?? ""));
                }
            }
            
            return builder.Build();
        }
    }

    /// <summary>
    /// Сериализуемая информация о параметре
    /// </summary>
    [Serializable]
    public class SerializableParameterInfo
    {
        public string Name { get; set; }
        public bool HasDefaultValue { get; set; }
        public string DefaultValue { get; set; }
        public bool IsByRef { get; set; }

        public static SerializableParameterInfo FromParameterInfo(BslParameterInfo param)
        {
            return new SerializableParameterInfo
            {
                Name = param.Name,
                HasDefaultValue = param.HasDefaultValue,
                DefaultValue = param.DefaultValue?.ToString(),
                IsByRef = !param.ExplicitByVal // ExplicitByVal означает по значению, поэтому IsByRef противоположен
            };
        }

        public BslParameterInfo ToParameterInfo()
        {
            var builder = new BslParameterBuilder()
                .Name(Name)
                .ByValue(!IsByRef);
                
            if (HasDefaultValue)
            {
                builder.DefaultValue(BslStringValue.Create(DefaultValue ?? ""));
            }
            
            return builder.Build();
        }
    }
}