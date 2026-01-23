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
    public class ModuleSerializer
    {
        public byte[] Serialize(StackRuntimeModule module, SymbolTable symbolTable)
        {
            using (var ms = new MemoryStream())
            {
                Serialize(module, symbolTable, ms);
                return ms.ToArray();
            }
        }

        public void Serialize(StackRuntimeModule module, SymbolTable symbolTable, Stream output)
        {
            var image = CreateImage(module, symbolTable);
            MessagePackSerializer.Serialize(output, image);
        }

        private ModuleImage CreateImage(StackRuntimeModule module, SymbolTable symbolTable)
        {
            return new ModuleImage
            {
                FormatVersion = 1,
                Constants = module.Constants.Select(ConvertConstant).ToArray(),
                Identifiers = module.Identifiers.ToArray(),
                Code = module.Code.Select(c => new CommandImage { Code = c.Code, Argument = c.Argument }).ToArray(),
                Methods = module.Methods.Cast<MachineMethodInfo>().Select(ConvertMethod).ToArray(),
                Fields = module.Fields.Cast<BslScriptFieldInfo>().Select(ConvertField).ToArray(),
                Properties = module.Properties.Cast<BslScriptPropertyInfo>().Select(ConvertProperty).ToArray(),
                ModuleAttributes = module.ModuleAttributes.Cast<BslAnnotationAttribute>()
                    .Select(a => ConvertAnnotationDefinition(a.ToMachineDefinition())).ToArray(),
                VariableBindings = module.VariableRefs.Select(b => ConvertBinding(b, symbolTable, false)).ToArray(),
                MethodBindings = module.MethodRefs.Select(b => ConvertBinding(b, symbolTable, true)).ToArray(),
                EntryMethodIndex = module.EntryMethodIndex
            };
        }

        private SymbolicBinding ConvertBinding(ModuleSymbolBinding binding, SymbolTable symbolTable, bool isMethod)
        {
            var result = new SymbolicBinding
            {
                Kind = binding.Kind,
                ScopeIndex = binding.ScopeIndex,
                MemberNumber = binding.MemberNumber
            };

            if (binding.Kind == ScopeBindingKind.Static && binding.Target != null)
            {
                result.ContextTypeName = binding.Target.GetType().FullName;
                
                // For static bindings, get symbol name from the target context
                result.SymbolName = isMethod
                    ? binding.Target.GetMethodInfo(binding.MemberNumber).Name
                    : binding.Target.GetPropertyInfo(binding.MemberNumber).Name;
            }
            else if (binding.Kind == ScopeBindingKind.ThisScope || binding.Kind == ScopeBindingKind.FrameScope)
            {
                // For non-static bindings, get symbol name from SymbolTable
                var scope = symbolTable.GetScope(binding.ScopeIndex);
                result.SymbolName = isMethod
                    ? scope.Methods[binding.MemberNumber].Name
                    : scope.Variables[binding.MemberNumber].Name;
            }

            return result;
        }

        private ConstantImage ConvertConstant(BslPrimitiveValue value)
        {
            var result = new ConstantImage();
            if (value is BslStringValue s)
            {
                result.Kind = ConstantKind.String;
                result.StringValue = (string)s;
            }
            else if (value is BslNumericValue n)
            {
                result.Kind = ConstantKind.Number;
                result.NumericValue = n.AsNumber();
            }
            else if (value is BslBooleanValue b)
            {
                result.Kind = ConstantKind.Boolean;
                result.BooleanValue = b.AsBoolean();
            }
            else if (value is BslDateValue d)
            {
                result.Kind = ConstantKind.Date;
                result.DateTicks = d.AsDate().Ticks;
            }
            else if (value is BslUndefinedValue)
            {
                result.Kind = ConstantKind.Undefined;
            }
            else if (value is BslNullValue)
            {
                result.Kind = ConstantKind.Null;
            }
            else if (value is BslTypeValue t)
            {
                result.Kind = ConstantKind.Type;
                result.TypeName = t.ToString(); // TODO: need better type serialization if needed
            }
            else if (value is BslAnnotationValue)
            {
                result.Kind = ConstantKind.Annotation;
            }
            else if (value is BslSkippedParameterValue)
            {
                result.Kind = ConstantKind.SkippedParameter;
            }

            return result;
        }

        private MethodImage ConvertMethod(MachineMethodInfo info)
        {
            var runtime = info.GetRuntimeMethod();
            return new MethodImage
            {
                Signature = ConvertMethodSignature(runtime.Signature),
                EntryPoint = runtime.EntryPoint,
                LocalVariables = runtime.LocalVariables
            };
        }

        private MethodSignatureImage ConvertMethodSignature(MethodSignature signature)
        {
            return new MethodSignatureImage
            {
                Name = signature.Name,
                Alias = signature.Alias,
                Params = signature.Params?.Select(ConvertParameterDefinition).ToArray(),
                Annotations = signature.Annotations?.Select(ConvertAnnotationDefinition).ToArray(),
                Flags = (int)signature.Flags
            };
        }

        private ParameterDefinitionImage ConvertParameterDefinition(ParameterDefinition p)
        {
            return new ParameterDefinitionImage
            {
                Name = p.Name,
                IsByValue = p.IsByValue,
                HasDefaultValue = p.HasDefaultValue,
                DefaultValueIndex = p.DefaultValueIndex,
                Annotations = p.Annotations?.Select(ConvertAnnotationDefinition).ToArray()
            };
        }

        private AnnotationDefinitionImage ConvertAnnotationDefinition(AnnotationDefinition a)
        {
            return new AnnotationDefinitionImage
            {
                Name = a.Name,
                Parameters = a.Parameters?.Select(p => new AnnotationParameterImage
                {
                    Name = p.Name,
                    Value = ConvertConstant((BslPrimitiveValue)p.RuntimeValue)
                }).ToArray()
            };
        }

        private FieldImage ConvertField(BslScriptFieldInfo f)
        {
            return new FieldImage
            {
                Name = f.Name,
                IsPublic = (f.Attributes & System.Reflection.FieldAttributes.Public) != 0,
                DispatchId = f.DispatchId,
                Annotations = f.GetAnnotations().Cast<BslAnnotationAttribute>()
                    .Select(a => ConvertAnnotationDefinition(a.ToMachineDefinition())).ToArray()
            };
        }

        private PropertyImage ConvertProperty(BslScriptPropertyInfo p)
        {
            return new PropertyImage
            {
                Name = p.Name,
                Alias = p.Alias,
                IsPublic = p.IsExported,
                CanRead = p.CanRead,
                CanWrite = p.CanWrite,
                DispatchId = p.DispatchId,
                Annotations = p.GetAnnotations().Cast<BslAnnotationAttribute>()
                    .Select(a => ConvertAnnotationDefinition(a.ToMachineDefinition())).ToArray()
            };
        }
    }
}
