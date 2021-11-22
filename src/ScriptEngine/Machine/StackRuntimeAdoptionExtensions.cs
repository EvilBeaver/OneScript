/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Linq;
using System.Reflection;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    public static class StackRuntimeAdoptionExtensions
    {
        public static MethodSignature MakeSignature(this BslMethodInfo method)
        {
            var signature = new MethodSignature
            {
                Name = method.Name,
                Alias = method.Alias,
                IsFunction = method.IsFunction(),
                IsExport = method.IsPublic,
                Annotations = MakeAnnotations(method),
                Params = method.GetParameters().Select(ToMachineDefinition).ToArray()
            };

            return signature;
        }

        public static BslAnnotationAttribute[] GetAnnotations(this ICustomAttributeProvider member)
        {
            return member.GetCustomAttributes(typeof(BslAnnotationAttribute), false)
                .Cast<BslAnnotationAttribute>()
                .ToArray();
        }
        
        public static AnnotationDefinition ToMachineDefinition(this BslAnnotationAttribute attribute)
        {
            return new AnnotationDefinition
            {
                Name = attribute.Name,
                Parameters = attribute.Parameters.Select(ToMachineDefinition).ToArray()
            };
        }

        public static AnnotationParameter ToMachineDefinition(this BslAnnotationParameter parameter)
        {
            return new AnnotationParameter
            {
                Name = parameter.Name,
                ValueIndex = parameter.ConstantValueIndex,
                RuntimeValue = parameter.Value,
            };
        }
        
        public static ParameterDefinition ToMachineDefinition(this ParameterInfo parameter)
        {
            return new ParameterDefinition
            {
                Name = parameter.Name,
                IsByValue = !parameter.IsByRef(),
                DefaultValueIndex = parameter is BslParameterInfo p? p.ConstantValueIndex : -1,
                HasDefaultValue = parameter.HasDefaultValue,
                Annotations = MakeAnnotations(parameter)
            };
        }

        private static AnnotationDefinition[] MakeAnnotations(ICustomAttributeProvider member)
        {
            return member.GetCustomAttributes(typeof(BslAnnotationAttribute), false)
                .Cast<BslAnnotationAttribute>()
                .Select(ToMachineDefinition).ToArray();
        }

        public static BslAnnotationAttribute MakeBslAttribute(this in AnnotationDefinition annotation)
        {
            var attribute = new BslAnnotationAttribute(annotation.Name);
            if (annotation.ParamCount > 0)
            {
                attribute.SetParameters(annotation.Parameters.Select(p => 
                    new BslAnnotationParameter(p.Name, (BslPrimitiveValue) p.RuntimeValue)
                    {
                        ConstantValueIndex = p.ValueIndex
                    }));
            }

            return attribute;
        }
    }
}