/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using OneScript.Compilation.Binding;
using OneScript.Contexts;
using OneScript.Contexts.Internal;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// Сериализация и десериализация скомпилированных модулей
    /// </summary>
    public class CompiledModulePackager
    {
        private Dictionary<IAttachableContext, string> _contextSymbols;

        /// <summary>
        /// Устанавливает маппинг контекстов на их символьные имена.
        /// Используется при сборке бандла.
        /// </summary>
        public void SetContextSymbols(Dictionary<IAttachableContext, string> symbols)
        {
            _contextSymbols = symbols;
        }

        /// <summary>
        /// Сериализует модуль в поток
        /// </summary>
        public void Save(Stream stream, StackRuntimeModule module)
        {
            var dto = ConvertToDto(module);
            MessagePackSerializer.Serialize(stream, dto);
        }

        /// <summary>
        /// Сериализует модуль в массив байт
        /// </summary>
        public byte[] Save(StackRuntimeModule module)
        {
            var dto = ConvertToDto(module);
            return MessagePackSerializer.Serialize(dto);
        }

        /// <summary>
        /// Десериализует модуль из потока
        /// </summary>
        public StackRuntimeModule Load(Stream stream, IRuntimeEnvironment environment)
        {
            var dto = MessagePackSerializer.Deserialize<CompiledModuleDto>(stream);
            return ConvertFromDto(dto, environment);
        }

        /// <summary>
        /// Десериализует модуль из массива байт
        /// </summary>
        public StackRuntimeModule Load(byte[] data, IRuntimeEnvironment environment)
        {
            var dto = MessagePackSerializer.Deserialize<CompiledModuleDto>(data);
            return ConvertFromDto(dto, environment);
        }

        /// <summary>
        /// Конвертирует модуль в DTO (используется также BundleBuilder)
        /// </summary>
        public CompiledModuleDto ConvertToDto(StackRuntimeModule module)
        {
            var dto = new CompiledModuleDto
            {
                EntryMethodIndex = module.EntryMethodIndex,
                SourceFileName = module.Source?.Location
            };

            // Constants
            foreach (var constant in module.Constants)
            {
                dto.Constants.Add(ConvertConstant(constant));
            }

            // Identifiers
            dto.Identifiers.AddRange(module.Identifiers);

            // Code
            foreach (var cmd in module.Code)
            {
                dto.Code.Add(new CommandDto { Code = (int)cmd.Code, Argument = cmd.Argument });
            }

            // Methods
            foreach (var method in module.Methods.Cast<MachineMethodInfo>())
            {
                dto.Methods.Add(ConvertMethod(method));
            }

            // Fields
            foreach (var field in module.Fields.Cast<BslScriptFieldInfo>())
            {
                dto.Fields.Add(ConvertField(field));
            }

            // Variable refs
            foreach (var binding in module.VariableRefs)
            {
                dto.VariableRefs.Add(ConvertBinding(binding));
            }

            // Method refs
            foreach (var binding in module.MethodRefs)
            {
                dto.MethodRefs.Add(ConvertBinding(binding));
            }

            // Module attributes
            foreach (var attr in module.ModuleAttributes)
            {
                dto.ModuleAttributes.Add(ConvertAnnotationAttribute(attr));
            }

            return dto;
        }

        private ConstantDto ConvertConstant(BslPrimitiveValue value)
        {
            var dto = new ConstantDto();

            switch (value)
            {
                case BslUndefinedValue _:
                    dto.Type = ConstantType.Undefined;
                    break;
                case BslNullValue _:
                    dto.Type = ConstantType.Null;
                    break;
                case BslStringValue str:
                    dto.Type = ConstantType.String;
                    dto.StringValue = (string)str;
                    break;
                case BslNumericValue num:
                    dto.Type = ConstantType.Number;
                    dto.NumberValue = (decimal)num;
                    break;
                case BslBooleanValue b:
                    dto.Type = ConstantType.Boolean;
                    dto.BoolValue = (bool)b;
                    break;
                case BslDateValue date:
                    dto.Type = ConstantType.Date;
                    dto.DateTicks = ((DateTime)date).Ticks;
                    break;
                default:
                    throw new InvalidOperationException($"Unknown constant type: {value?.GetType().Name}");
            }

            return dto;
        }

        private MethodDto ConvertMethod(MachineMethodInfo method)
        {
            var runtime = method.GetRuntimeMethod();
            var sig = runtime.Signature;

            var dto = new MethodDto
            {
                Name = sig.Name,
                Alias = sig.Alias,
                IsFunction = sig.IsFunction,
                IsExport = sig.IsExport,
                IsAsync = sig.IsAsync,
                IsDeprecated = sig.IsDeprecated,
                ThrowOnUseDeprecated = sig.ThrowOnUseDeprecated,
                EntryPoint = runtime.EntryPoint,
                LocalVariables = runtime.LocalVariables?.ToList() ?? new List<string>()
            };

            if (sig.Params != null)
            {
                foreach (var param in sig.Params)
                {
                    dto.Parameters.Add(ConvertParameter(param));
                }
            }

            if (sig.Annotations != null)
            {
                foreach (var anno in sig.Annotations)
                {
                    dto.Annotations.Add(ConvertAnnotation(anno));
                }
            }

            return dto;
        }

        private ParameterDto ConvertParameter(ParameterDefinition param)
        {
            var dto = new ParameterDto
            {
                Name = param.Name,
                IsByValue = param.IsByValue,
                HasDefaultValue = param.HasDefaultValue,
                DefaultValueIndex = param.DefaultValueIndex
            };

            if (param.Annotations != null)
            {
                dto.Annotations = param.Annotations.Select(ConvertAnnotation).ToList();
            }

            return dto;
        }

        private AnnotationDto ConvertAnnotation(AnnotationDefinition anno)
        {
            var dto = new AnnotationDto { Name = anno.Name };

            if (anno.Parameters != null)
            {
                dto.Parameters = anno.Parameters.Select(p => new AnnotationParameterDto
                {
                    Name = p.Name,
                    Value = p.RuntimeValue?.ToString()
                }).ToList();
            }

            return dto;
        }

        private AnnotationDto ConvertAnnotationAttribute(BslAnnotationAttribute attr)
        {
            var dto = new AnnotationDto { Name = attr.Name };

            if (attr.Parameters != null && attr.Parameters.Any())
            {
                dto.Parameters = attr.Parameters.Select(p => new AnnotationParameterDto
                {
                    Name = p.Name,
                    Value = p.Value?.ToString()
                }).ToList();
            }

            return dto;
        }

        private FieldDto ConvertField(BslScriptFieldInfo field)
        {
            var dto = new FieldDto
            {
                Name = field.Name,
                IsExport = field.IsPublic,
                DispatchId = field.DispatchId
            };

            var annotations = field.GetAnnotations();
            if (annotations != null && annotations.Any())
            {
                dto.Annotations = annotations.Select(ConvertAnnotationAttribute).ToList();
            }

            return dto;
        }

        private SymbolBindingDto ConvertBinding(ModuleSymbolBinding binding)
        {
            var dto = new SymbolBindingDto
            {
                Kind = binding.Kind,
                MemberNumber = binding.MemberNumber,
                ScopeIndex = binding.ScopeIndex
            };

            if (binding.Kind == ScopeBindingKind.Static && binding.Target != null)
            {
                // Сохраняем имя контекста для восстановления при загрузке
                dto.ContextName = GetContextIdentifier(binding.Target);
                
                // Для PropertyBag сохраняем также имя свойства
                if (binding.Target is PropertyBag propertyBag)
                {
                    dto.MemberName = propertyBag.GetPropName(binding.MemberNumber);
                }
            }

            return dto;
        }

        private string GetContextIdentifier(IAttachableContext context)
        {
            // Для пользовательских модулей используем специальный префикс + символьное имя
            if (context is UserScriptContextInstance)
            {
                // Пытаемся найти символьное имя модуля
                if (_contextSymbols != null && _contextSymbols.TryGetValue(context, out var symbol))
                {
                    return "$UserModule:" + symbol;
                }
            }

            // Для системных контекстов используем полное имя типа
            return context.GetType().FullName;
        }

        /// <summary>
        /// Конвертирует DTO обратно в модуль (используется также BundleLoader)
        /// </summary>
        public StackRuntimeModule ConvertFromDto(CompiledModuleDto dto, IRuntimeEnvironment environment)
        {
            if (dto.MagicHeader != CompiledModuleDto.Magic)
            {
                throw new InvalidOperationException("Invalid compiled module format");
            }

            if (dto.Version > CompiledModuleDto.FormatVersion)
            {
                throw new InvalidOperationException($"Unsupported module version: {dto.Version}");
            }

            var module = new StackRuntimeModule(typeof(Machine.Contexts.UserScriptContextInstance))
            {
                EntryMethodIndex = dto.EntryMethodIndex
            };

            // Constants
            foreach (var constDto in dto.Constants)
            {
                module.Constants.Add(ConvertConstantFromDto(constDto));
            }

            // Identifiers
            module.Identifiers.AddRange(dto.Identifiers);

            // Code
            foreach (var cmdDto in dto.Code)
            {
                module.Code.Add(new Command
                {
                    Code = (OperationCode)cmdDto.Code,
                    Argument = cmdDto.Argument
                });
            }

            // Build context lookup for symbol resolution
            var contextLookup = BuildContextLookup(environment);

            // Variable refs
            foreach (var bindingDto in dto.VariableRefs)
            {
                module.VariableRefs.Add(ConvertBindingFromDto(bindingDto, contextLookup));
            }

            // Method refs
            foreach (var bindingDto in dto.MethodRefs)
            {
                module.MethodRefs.Add(ConvertBindingFromDto(bindingDto, contextLookup));
            }

            // Fields
            foreach (var fieldDto in dto.Fields)
            {
                module.Fields.Add(ConvertFieldFromDto(fieldDto, module.ClassType));
            }

            // Methods
            for (int i = 0; i < dto.Methods.Count; i++)
            {
                module.Methods.Add(ConvertMethodFromDto(dto.Methods[i], module.ClassType, i));
            }

            // Module attributes
            foreach (var attrDto in dto.ModuleAttributes)
            {
                module.ModuleAttributes.Add(ConvertAnnotationAttributeFromDto(attrDto));
            }

            return module;
        }

        private BslPrimitiveValue ConvertConstantFromDto(ConstantDto dto)
        {
            return dto.Type switch
            {
                ConstantType.Undefined => BslUndefinedValue.Instance,
                ConstantType.Null => BslNullValue.Instance,
                ConstantType.String => BslStringValue.Create(dto.StringValue ?? string.Empty),
                ConstantType.Number => BslNumericValue.Create(dto.NumberValue ?? 0),
                ConstantType.Boolean => dto.BoolValue == true ? BslBooleanValue.True : BslBooleanValue.False,
                ConstantType.Date => BslDateValue.Create(new DateTime(dto.DateTicks ?? 0)),
                _ => throw new InvalidOperationException($"Unknown constant type: {dto.Type}")
            };
        }

        private Dictionary<string, IAttachableContext> BuildContextLookup(IRuntimeEnvironment environment)
        {
            var lookup = new Dictionary<string, IAttachableContext>(StringComparer.OrdinalIgnoreCase);

            foreach (var context in environment.AttachedContexts)
            {
                // Для системных контекстов — по имени типа
                var typeKey = context.GetType().FullName;
                if (!string.IsNullOrEmpty(typeKey) && !lookup.ContainsKey(typeKey))
                {
                    lookup[typeKey] = context;
                }

                // Для PropertyBag — добавляем пользовательские модули по символьным именам
                if (context is PropertyBag propertyBag)
                {
                    for (int i = 0; i < propertyBag.Count; i++)
                    {
                        var value = propertyBag.GetPropValue(i);
                        if (value is IAttachableContext attachable)
                        {
                            var symbol = propertyBag.GetPropName(i);
                            var userModuleKey = "$UserModule:" + symbol;
                            if (!lookup.ContainsKey(userModuleKey))
                            {
                                lookup[userModuleKey] = attachable;
                            }
                        }
                    }
                }
            }

            return lookup;
        }

        private ModuleSymbolBinding ConvertBindingFromDto(SymbolBindingDto dto, Dictionary<string, IAttachableContext> contextLookup)
        {
            var binding = new ModuleSymbolBinding
            {
                Kind = dto.Kind,
                MemberNumber = dto.MemberNumber,
                ScopeIndex = dto.ScopeIndex
            };

            if (dto.Kind == ScopeBindingKind.Static && !string.IsNullOrEmpty(dto.ContextName))
            {
                if (contextLookup.TryGetValue(dto.ContextName, out var context))
                {
                    binding.Target = context;
                    
                    // Для PropertyBag восстанавливаем MemberNumber по имени
                    if (context is PropertyBag propertyBag && !string.IsNullOrEmpty(dto.MemberName))
                    {
                        binding.MemberNumber = propertyBag.GetPropertyNumber(dto.MemberName);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Cannot resolve context: {dto.ContextName}");
                }
            }

            return binding;
        }

        private BslScriptFieldInfo ConvertFieldFromDto(FieldDto dto, Type ownerType)
        {
            var builder = BslScriptFieldInfo.Create(dto.Name);
            var buildable = (IBuildableMember)builder;

            buildable.SetDispatchIndex(dto.DispatchId);
            buildable.SetExportFlag(dto.IsExport);
            buildable.SetDeclaringType(ownerType);

            if (dto.Annotations != null)
            {
                buildable.SetAnnotations(dto.Annotations.Select(ConvertAnnotationAttributeFromDto));
            }

            return builder;
        }

        private BslScriptMethodInfo ConvertMethodFromDto(MethodDto dto, Type ownerType, int index)
        {
            var builder = MachineMethodInfo.Create();
            var buildable = (IBuildableMember)builder;
            var buildableMethod = (IBuildableMethod)builder;

            buildable.SetName(dto.Name);
            buildable.SetAlias(dto.Alias);
            buildable.SetExportFlag(dto.IsExport);
            buildable.SetDeclaringType(ownerType);
            
            // DispatchId = METHOD_COUNT + index
            // Для UserScriptContextInstance METHOD_COUNT = 1
            buildable.SetDispatchIndex(1 + index);

            if (dto.IsFunction)
            {
                buildable.SetDataType(typeof(OneScript.Values.BslValue));
            }

            // Parameters
            var parameters = dto.Parameters.Select((p, idx) => ConvertParameterFromDto(p, idx, builder)).ToList();
            buildableMethod.SetParameters(parameters);

            // Annotations
            if (dto.Annotations != null)
            {
                buildable.SetAnnotations(dto.Annotations.Select(ConvertAnnotationAttributeFromDto));
            }

            // Runtime parameters
            builder.SetRuntimeParameters(dto.EntryPoint, dto.LocalVariables.ToArray());

            return builder;
        }

        private BslParameterInfo ConvertParameterFromDto(ParameterDto dto, int position, BslScriptMethodInfo method)
        {
            var builder = BslParameterInfo.Create();
            var buildable = (IBuildableMember)builder;

            buildable.SetName(dto.Name);
            builder.SetPosition(position);
            builder.SetByValue(dto.IsByValue);

            if (dto.HasDefaultValue)
            {
                builder.SetDefaultValueIndex(dto.DefaultValueIndex);
            }

            if (dto.Annotations != null)
            {
                buildable.SetAnnotations(dto.Annotations.Select(ConvertAnnotationAttributeFromDto));
            }

            return builder;
        }

        private BslAnnotationAttribute ConvertAnnotationAttributeFromDto(AnnotationDto dto)
        {
            if (dto.Parameters != null && dto.Parameters.Count > 0)
            {
                var parameters = dto.Parameters.Select(p => new BslAnnotationParameter(
                    p.Name,
                    p.Value != null ? BslStringValue.Create(p.Value) : null
                ));
                return new BslAnnotationAttribute(dto.Name, parameters);
            }

            return new BslAnnotationAttribute(dto.Name);
        }
    }
}
