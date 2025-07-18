/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Runtime.CompilerServices;
using System.Linq;
using OneScript.Contexts;
using OneScript.Values;

namespace ScriptEngine.Machine
{
    internal class MachineMethodInfo : BslScriptMethodInfo
    {
        private MachineMethod _method;
        
        // Внутренний конструктор для десериализации из кэша
        internal MachineMethodInfo(MachineMethod method) : base(
            method.Signature.Name,
            method.Signature.Alias,
            null, // declaringType - будет установлен позже
            method.Signature.IsFunction ? typeof(BslValue) : typeof(void),
            method.Signature.IsExport,
            -1, // dispatchId - будет установлен позже  
            ConvertParametersForCache(method.Signature.Params),
            ConvertAnnotations(method.Signature.Annotations))
        {
            _method = method;
        }
        
        public MachineMethodInfo() : base()
        {
            // Конструктор по умолчанию для обычного создания
        }
        
        internal void SetRuntimeParameters(int entryPoint, string[] locals)
        {
            _method = new MachineMethod
            {
                EntryPoint = entryPoint,
                LocalVariables = locals,
                Signature = this.MakeSignature()
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal MachineMethod GetRuntimeMethod() => _method;

        /// <summary>
        /// Конвертирует ParameterDefinition[] в BslParameterInfo[] для десериализации из кэша
        /// Пропускает индексы значений по умолчанию для избежания IndexOutOfRangeException
        /// </summary>
        private static BslParameterInfo[] ConvertParametersForCache(ParameterDefinition[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return new BslParameterInfo[0];

            var result = new BslParameterInfo[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var builder = new BslParameterBuilder()
                    .Name(param.Name)
                    .ByValue(!param.IsByValue)
                    .ParameterType(typeof(BslValue));
                
                // Пропускаем значения по умолчанию при загрузке из кэша
                // так как они будут восстановлены из сериализованных данных позже
                
                if (param.Annotations != null && param.Annotations.Length > 0)
                {
                    var annotations = param.Annotations.Select(a => a.MakeBslAttribute()).ToArray();
                    builder.SetAnnotations(annotations);
                }
                
                result[i] = builder.Build();
            }
            return result;
        }

        /// <summary>
        /// Конвертирует ParameterDefinition[] в BslParameterInfo[]
        /// </summary>
        private static BslParameterInfo[] ConvertParameters(ParameterDefinition[] parameters)
        {
            if (parameters == null || parameters.Length == 0)
                return new BslParameterInfo[0];

            var result = new BslParameterInfo[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var builder = new BslParameterBuilder()
                    .Name(param.Name)
                    .ByValue(!param.IsByValue)
                    .ParameterType(typeof(BslValue));
                
                if (param.HasDefaultValue)
                {
                    builder.CompileTimeBslConstant(param.DefaultValueIndex);
                }
                
                if (param.Annotations != null && param.Annotations.Length > 0)
                {
                    var annotations = param.Annotations.Select(a => a.MakeBslAttribute()).ToArray();
                    builder.SetAnnotations(annotations);
                }
                
                result[i] = builder.Build();
            }
            return result;
        }

        /// <summary>
        /// Конвертирует AnnotationDefinition[] в BslAnnotationAttribute[]
        /// </summary>
        private static BslAnnotationAttribute[] ConvertAnnotations(AnnotationDefinition[] annotations)
        {
            if (annotations == null || annotations.Length == 0)
                return new BslAnnotationAttribute[0];

            return annotations.Select(a => a.MakeBslAttribute()).ToArray();
        }
    }
}