/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using MessagePack;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Sources;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Compilation
{
    /// <summary>
    /// Сериализуемая версия IExecutableModule для кэширования
    /// </summary>
    [MessagePackObject]
    public class SerializableModule
    {
        [Key(0)]
        public int FormatVersion { get; set; } = 1;
        
        // Основные компоненты модуля
        [Key(1)]
        public SerializableConstant[] Constants { get; set; }
        [Key(2)]
        public SymbolBinding[] VariableRefs { get; set; }
        [Key(3)]
        public SymbolBinding[] MethodRefs { get; set; }
        [Key(4)]
        public Command[] Code { get; set; }
        
        // Метаданные модуля  
        [Key(5)]
        public int EntryMethodIndex { get; set; } = -1;
        [Key(6)]
        public SerializableSourceCode Source { get; set; }
        
        // Члены модуля
        [Key(7)]
        public SerializableAnnotation[] ModuleAttributes { get; set; }
        [Key(8)]
        public SerializableFieldInfo[] Fields { get; set; }
        [Key(9)]
        public SerializablePropertyInfo[] Properties { get; set; }
        [Key(10)]
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
            var module = new StackRuntimeModule(typeof(IRuntimeContextInstance))
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
    [MessagePackObject]
    public class SerializableConstant
    {
        [Key(0)]
        public string Type { get; set; }
        [Key(1)]
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
                    return BslNumericValue.Create(decimal.TryParse(Value, NumberStyles.Number, NumberFormatInfo.InvariantInfo, out var num) ? num : 0);
                case nameof(BslBooleanValue):
                    // Обработка локализованных строк для булевых значений
                    if (string.IsNullOrEmpty(Value)) return BslBooleanValue.False;
                    var trimmedValue = Value.Trim();
                    var isTrue = StringComparer.OrdinalIgnoreCase.Equals(trimmedValue, "true") ||
                                StringComparer.OrdinalIgnoreCase.Equals(trimmedValue, "да") ||
                                StringComparer.OrdinalIgnoreCase.Equals(trimmedValue, "yes");
                    return (BslPrimitiveValue)BslBooleanValue.Create(isTrue);
                case nameof(BslUndefinedValue):
                    return BslUndefinedValue.Instance;
                case nameof(BslNullValue):
                    return BslNullValue.Instance;
                default:
                    throw new InvalidOperationException($"Неподдерживаемый тип значения для десериализации: {Type}");
            }
        }
    }

    /// <summary>
    /// Сериализуемый исходный код
    /// </summary>
    [MessagePackObject]
    public class SerializableSourceCode
    {
        [Key(0)]
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
            // Для кэширования восстанавливаем исходный код из файла
            if (!string.IsNullOrEmpty(Location) && System.IO.File.Exists(Location))
            {
                return SourceCodeBuilder.Create().FromFile(Location).Build();
            }
            
            return null;
        }
    }

    /// <summary>
    /// Сериализуемая аннотация
    /// </summary>
    [MessagePackObject]
    public class SerializableAnnotation
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
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
    [MessagePackObject]
    public class SerializableAnnotationParameter
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public string Value { get; set; }
        [Key(2)]
        public int ValueIndex { get; set; }

        public static SerializableAnnotationParameter FromParameter(BslAnnotationParameter parameter)
        {
            return new SerializableAnnotationParameter
            {
                Name = parameter.Name,
                Value = parameter.Value?.ToString() ?? "",
                ValueIndex = parameter.ConstantValueIndex
            };
        }

        public static SerializableAnnotationParameter FromAnnotationParameter(AnnotationParameter parameter)
        {
            return new SerializableAnnotationParameter
            {
                Name = parameter.Name,
                ValueIndex = parameter.ValueIndex
            };
        }

        public BslAnnotationParameter ToParameter()
        {
            return new BslAnnotationParameter(Name, BslStringValue.Create(Value ?? ""))
            {
                ConstantValueIndex = ValueIndex
            };
        }

        public AnnotationParameter ToAnnotationParameter()
        {
            return new AnnotationParameter
            {
                Name = Name,
                ValueIndex = ValueIndex
            };
        }
    }

    /// <summary>
    /// Сериализуемая информация о поле
    /// </summary>
    [MessagePackObject]
    public class SerializableFieldInfo
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public string Alias { get; set; }
        [Key(2)]
        public bool IsExport { get; set; }
        [Key(3)]
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
    [MessagePackObject]
    public class SerializablePropertyInfo
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public string Alias { get; set; }
        [Key(2)]
        public bool IsExport { get; set; }
        [Key(3)]
        public int DispatchId { get; set; }
        [Key(4)]
        public bool CanRead { get; set; }
        [Key(5)]
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
    [MessagePackObject]
    public class SerializableMethodInfo
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public string Alias { get; set; }
        [Key(2)]
        public bool IsExport { get; set; }
        [Key(3)]
        public int DispatchId { get; set; }
        [Key(4)]
        public SerializableParameterInfo[] Parameters { get; set; }
        [Key(5)]
        public int EntryPoint { get; set; }
        [Key(6)]
        public string[] LocalVariables { get; set; }
        [Key(7)]
        public SerializableAnnotationDefinition[] Annotations { get; set; }
        [Key(8)]
        public int Flags { get; set; }

        public static SerializableMethodInfo FromMethodInfo(BslScriptMethodInfo method)
        {
            // Для правильной сериализации используем MachineMethodInfo напрямую
            if (method is MachineMethodInfo machineMethod)
            {
                var runtimeMethod = machineMethod.GetRuntimeMethod();
                return new SerializableMethodInfo
                {
                    Name = runtimeMethod.Signature.Name,
                    Alias = runtimeMethod.Signature.Alias,
                    IsExport = runtimeMethod.Signature.IsExport,
                    DispatchId = method.DispatchId,
                    Parameters = runtimeMethod.Signature.Params?.Select(SerializableParameterInfo.FromParameterDefinition).ToArray() ?? new SerializableParameterInfo[0],
                    EntryPoint = runtimeMethod.EntryPoint,
                    LocalVariables = runtimeMethod.LocalVariables ?? new string[0],
                    Annotations = runtimeMethod.Signature.Annotations?.Select(SerializableAnnotationDefinition.FromAnnotationDefinition).ToArray() ?? new SerializableAnnotationDefinition[0],
                    Flags = (int)runtimeMethod.Signature.Flags
                };
            }
            else
            {
                throw new ArgumentException($"Unsupported method type: {method.GetType().Name}. Only MachineMethodInfo is supported for serialization.");
            }
        }

        public BslScriptMethodInfo ToMethodInfo()
        {
            // Создаем MachineMethod напрямую
            var signature = new MethodSignature
            {
                Name = Name,
                Alias = Alias,
                Flags = (MethodFlags)Flags,
                Params = Parameters?.Select(p => p.ToParameterDefinition()).ToArray() ?? new ParameterDefinition[0],
                Annotations = Annotations?.Select(a => a.ToAnnotationDefinition()).ToArray() ?? new AnnotationDefinition[0]
            };
            
            var machineMethod = new MachineMethod
            {
                Signature = signature,
                EntryPoint = EntryPoint,
                LocalVariables = LocalVariables ?? new string[0]
            };
            
            return new MachineMethodInfo(machineMethod);
        }
    }

    /// <summary>
    /// Сериализуемая информация о параметре
    /// </summary>
    [MessagePackObject]
    public class SerializableParameterInfo
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public bool HasDefaultValue { get; set; }
        [Key(2)]
        public string DefaultValue { get; set; }
        [Key(3)]
        public bool IsByRef { get; set; }
        [Key(4)]
        public int DefaultValueIndex { get; set; }
        [Key(5)]
        public SerializableAnnotationDefinition[] Annotations { get; set; }

        public static SerializableParameterInfo FromParameterDefinition(ParameterDefinition param)
        {
            return new SerializableParameterInfo
            {
                Name = param.Name,
                HasDefaultValue = param.HasDefaultValue,
                DefaultValueIndex = param.DefaultValueIndex,
                IsByRef = !param.IsByValue,
                Annotations = param.Annotations?.Select(SerializableAnnotationDefinition.FromAnnotationDefinition).ToArray() ?? new SerializableAnnotationDefinition[0]
            };
        }

        public ParameterDefinition ToParameterDefinition()
        {
            return new ParameterDefinition
            {
                Name = Name,
                HasDefaultValue = HasDefaultValue,
                DefaultValueIndex = DefaultValueIndex,
                IsByValue = !IsByRef,
                Annotations = Annotations?.Select(a => a.ToAnnotationDefinition()).ToArray() ?? new AnnotationDefinition[0]
            };
        }


    }

    /// <summary>
    /// Сериализуемое определение аннотации
    /// </summary>
    [MessagePackObject]
    public class SerializableAnnotationDefinition
    {
        [Key(0)]
        public string Name { get; set; }
        [Key(1)]
        public SerializableAnnotationParameter[] Parameters { get; set; }

        public static SerializableAnnotationDefinition FromAnnotationDefinition(AnnotationDefinition annotation)
        {
            return new SerializableAnnotationDefinition
            {
                Name = annotation.Name,
                Parameters = annotation.Parameters?.Select(SerializableAnnotationParameter.FromAnnotationParameter).ToArray() ?? new SerializableAnnotationParameter[0]
            };
        }

        public AnnotationDefinition ToAnnotationDefinition()
        {
            return new AnnotationDefinition
            {
                Name = Name,
                Parameters = Parameters?.Select(p => p.ToAnnotationParameter()).ToArray() ?? new AnnotationParameter[0]
            };
        }

        public BslAnnotationAttribute ToBslAnnotation()
        {
            var bslParams = Parameters?.Select(p => p.ToParameter()).ToArray() ?? new BslAnnotationParameter[0];
            return new BslAnnotationAttribute(Name, bslParams);
        }
    }
}