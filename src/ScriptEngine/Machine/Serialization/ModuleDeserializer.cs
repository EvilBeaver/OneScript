using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MessagePack;
using OneScript.Values;
using ScriptEngine.Machine;
using OneScript.Execution;
using OneScript.Contexts;
using OneScript.Compilation.Binding;

namespace ScriptEngine.Machine.Serialization
{
    public class ModuleDeserializer
    {
        public StackRuntimeModule Deserialize(byte[] data, IRuntimeEnvironment environment)
        {
            using (var ms = new MemoryStream(data))
            {
                return Deserialize(ms, environment);
            }
        }

        public StackRuntimeModule Deserialize(Stream input, IRuntimeEnvironment environment)
        {
            var image = MessagePackSerializer.Deserialize<ModuleImage>(input);
            return CreateModule(image, environment);
        }

        private StackRuntimeModule CreateModule(ModuleImage image, IRuntimeEnvironment environment)
        {
            // TODO: ClassType might need better handling if we want to support user classes serialization
            var module = new StackRuntimeModule(null);

            foreach (var identifier in image.Identifiers)
            {
                module.Identifiers.Add(identifier);
            }

            foreach (var constant in image.Constants)
            {
                module.Constants.Add(RestoreConstant(constant));
            }

            foreach (var cmd in image.Code)
            {
                module.Code.Add(new Command { Code = cmd.Code, Argument = cmd.Argument });
            }

            for (int i = 0; i < image.Methods.Length; i++)
            {
                module.Methods.Add(RestoreMethod(image.Methods[i], module, i));
            }

            foreach (var fieldImage in image.Fields)
            {
                module.Fields.Add(RestoreField(fieldImage));
            }

            foreach (var propImage in image.Properties)
            {
                module.Properties.Add(RestoreProperty(propImage));
            }

            foreach (var attrDefImage in image.ModuleAttributes)
            {
                module.ModuleAttributes.Add(RestoreAnnotationDefinition(attrDefImage).MakeBslAttribute());
            }

            foreach (var binding in image.MethodBindings)
            {
                module.MethodRefs.Add(RebindSymbol(binding, environment, true));
            }

            foreach (var binding in image.VariableBindings)
            {
                module.VariableRefs.Add(RebindSymbol(binding, environment, false));
            }

            module.EntryMethodIndex = image.EntryMethodIndex;

            return module;
        }

        private BslPrimitiveValue RestoreConstant(ConstantImage image)
        {
            switch (image.Kind)
            {
                case ConstantKind.String:
                    return BslStringValue.Create(image.StringValue);
                case ConstantKind.Number:
                    return BslNumericValue.Create(image.NumericValue);
                case ConstantKind.Boolean:
                    return (BslPrimitiveValue)BslBooleanValue.Create(image.BooleanValue);
                case ConstantKind.Date:
                    return BslDateValue.Create(new DateTime(image.DateTicks));
                case ConstantKind.Undefined:
                    return BslUndefinedValue.Instance;
                case ConstantKind.Null:
                    return BslNullValue.Instance;
                case ConstantKind.Type:
                    // TODO: how to restore Type value?
                    return BslUndefinedValue.Instance;
                case ConstantKind.Annotation:
                    return BslUndefinedValue.Instance; // Should not happen for primitive values
                case ConstantKind.SkippedParameter:
                    return BslSkippedParameterValue.Instance;
                default:
                    return BslUndefinedValue.Instance;
            }
        }

        private MachineMethodInfo RestoreMethod(MethodImage image, StackRuntimeModule module, int dispatchId)
        {
            var factory = new BslMethodInfoFactory<MachineMethodInfo>(() => new MachineMethodInfo());
            var builder = factory.NewMethod();
            
            // Set basic method info
            builder.Name(image.Signature.Name);
            if (!string.IsNullOrEmpty(image.Signature.Alias))
            {
                builder.Alias(image.Signature.Alias);
            }
            
            var flags = (MethodFlags)image.Signature.Flags;
            builder.IsExported((flags & MethodFlags.IsExported) != 0);
            builder.DeclaringType(module.ClassType);
            builder.SetDispatchingIndex(dispatchId);
            
            // Add parameters if any
            if (image.Signature.Params != null)
            {
                foreach (var paramImage in image.Signature.Params)
                {
                    var paramBuilder = builder.NewParameter();
                    paramBuilder.Name(paramImage.Name);
                    paramBuilder.ByValue(paramImage.IsByValue);
                    if (paramImage.HasDefaultValue)
                    {
                        paramBuilder.DefaultValue(module.Constants[paramImage.DefaultValueIndex]);
                    }
                    if (paramImage.Annotations != null)
                    {
                        var annotations = paramImage.Annotations
                            .Select(a => RestoreAnnotationDefinition(a).MakeBslAttribute());
                        paramBuilder.SetAnnotations(annotations);
                    }
                }
            }
            
            // Add method annotations if any
            if (image.Signature.Annotations != null)
            {
                var annotations = image.Signature.Annotations
                    .Select(a => RestoreAnnotationDefinition(a).MakeBslAttribute());
                builder.SetAnnotations(annotations);
            }
            
            var methodInfo = builder.Build();
            
            // Set runtime parameters (entry point and local variables)
            methodInfo.SetRuntimeParameters(image.EntryPoint, image.LocalVariables);
            
            return methodInfo;
        }

        private ParameterDefinition RestoreParameterDefinition(ParameterDefinitionImage image)
        {
            return new ParameterDefinition
            {
                Name = image.Name,
                IsByValue = image.IsByValue,
                HasDefaultValue = image.HasDefaultValue,
                DefaultValueIndex = image.DefaultValueIndex,
                Annotations = image.Annotations?.Select(RestoreAnnotationDefinition).ToArray()
            };
        }

        private AnnotationDefinition RestoreAnnotationDefinition(AnnotationDefinitionImage image)
        {
            return new AnnotationDefinition
            {
                Name = image.Name,
                Parameters = image.Parameters?.Select(p => new AnnotationParameter
                {
                    Name = p.Name,
                    RuntimeValue = RestoreConstant(p.Value)
                }).ToArray()
            };
        }

        private BslScriptFieldInfo RestoreField(FieldImage image)
        {
            var annotations = image.Annotations?.Select(a => RestoreAnnotationDefinition(a).MakeBslAttribute()).ToArray() 
                ?? Array.Empty<BslAnnotationAttribute>();
            
            return BslFieldBuilder.Create()
                .Name(image.Name)
                .IsExported(image.IsPublic)
                .SetDispatchingIndex(image.DispatchId)
                .SetAnnotations(annotations)
                .Build();
        }

        private BslScriptPropertyInfo RestoreProperty(PropertyImage image)
        {
            var annotations = image.Annotations?.Select(a => RestoreAnnotationDefinition(a).MakeBslAttribute()).ToArray() 
                ?? Array.Empty<BslAnnotationAttribute>();
            
            return BslPropertyBuilder.Create()
                .Name(image.Name)
                .Alias(image.Alias)
                .IsExported(image.IsPublic)
                .CanRead(image.CanRead)
                .CanWrite(image.CanWrite)
                .SetDispatchingIndex(image.DispatchId)
                .SetAnnotations(annotations)
                .Build();
        }

        private ModuleSymbolBinding RebindSymbol(SymbolicBinding symbolic, IRuntimeEnvironment environment, bool isMethod)
        {
            switch (symbolic.Kind)
            {
                case ScopeBindingKind.ThisScope:
                    return new ModuleSymbolBinding
                    {
                        Kind = ScopeBindingKind.ThisScope,
                        MemberNumber = symbolic.MemberNumber
                    };
                case ScopeBindingKind.FrameScope:
                    return new ModuleSymbolBinding
                    {
                        Kind = ScopeBindingKind.FrameScope,
                        ScopeIndex = symbolic.ScopeIndex,
                        MemberNumber = symbolic.MemberNumber
                    };
                case ScopeBindingKind.Static:
                    return RebindStaticSymbol(symbolic, environment, isMethod);
                default:
                    throw new InvalidOperationException("Unknown binding kind");
            }
        }

        private ModuleSymbolBinding RebindStaticSymbol(SymbolicBinding symbolic, IRuntimeEnvironment environment, bool isMethod)
        {
            var context = environment.AttachedContexts
                .FirstOrDefault(c => c.GetType().FullName == symbolic.ContextTypeName);

            if (context == null)
            {
                // Try to find by name if FullName is not matched (might happen with different assembly versions)
                context = environment.AttachedContexts
                    .FirstOrDefault(c => c.GetType().Name == symbolic.ContextTypeName.Split('.').Last());
            }

            if (context == null)
                throw new SymbolDeserializationException($"Context '{symbolic.ContextTypeName}' not found");

            int memberNumber = isMethod
                ? context.GetMethodNumber(symbolic.SymbolName)
                : context.GetPropertyNumber(symbolic.SymbolName);

            if (memberNumber < 0)
                throw new SymbolDeserializationException(
                    $"Symbol '{symbolic.SymbolName}' not found in '{symbolic.ContextTypeName}'");

            return new ModuleSymbolBinding
            {
                Kind = ScopeBindingKind.Static,
                Target = context,
                MemberNumber = memberNumber
            };
        }
    }
}
