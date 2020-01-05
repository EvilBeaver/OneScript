/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Environment;
using System.Linq;

namespace ScriptEngine.Machine
{
    public class LoadedModule
    {
        public LoadedModule(ModuleImage image)
        {
            Code = image.Code.ToArray();
            EntryMethodIndex = image.EntryMethodIndex;
            MethodRefs = image.MethodRefs.ToArray();
            VariableRefs = image.VariableRefs.ToArray();
            Methods = image.Methods.ToArray();
            Constants = new IValue[image.Constants.Count];
            Variables = new VariablesFrame(image.Variables);
            ExportedProperies = image.ExportedProperties.ToArray();
            ExportedMethods = image.ExportedMethods.ToArray();
            ModuleInfo = image.ModuleInfo;
            LoadAddress = image.LoadAddress;
            for (int i = 0; i < image.Constants.Count; i++)
            {
                var def = image.Constants[i];
                Constants[i] = ValueFactory.Parse(def.Presentation, def.Type);
            }

            ResolveAnnotationConstants();
        }

        private void ResolveAnnotationConstants()
        {
            for (int i = 0; i < Variables.Count; i++)
            {
                EvaluateAnnotationParametersValues(Variables[i].Annotations);
            }

            for (int i = 0; i < Methods.Length; i++)
            {
                EvaluateAnnotationParametersValues(Methods[i].Signature.Annotations);
                for (int j = 0; j < Methods[i].Signature.ArgCount; j++)
                {
                    EvaluateAnnotationParametersValues(Methods[i].Signature.Params[j].Annotations);
                }
            }
        }

        private void EvaluateAnnotationParametersValues(AnnotationDefinition[] annotations)
        {
            for (int i = 0; i < annotations?.Length; i++)
            {
                var parameters = annotations[i].Parameters;
                for (int j = 0; j < parameters?.Length; j++)
                {
                    var pa = parameters[j];
                    if (pa.ValueIndex != AnnotationParameter.UNDEFINED_VALUE_INDEX)
                    {
                        annotations[i].Parameters[j].RuntimeValue = Constants[pa.ValueIndex];
                    }
                }
            }
        }

        public VariablesFrame Variables { get; }
        public int EntryMethodIndex { get; }
        public Command[] Code { get; }
        public SymbolBinding[] VariableRefs { get; }
        public SymbolBinding[] MethodRefs { get; }
        public MethodDescriptor[] Methods { get; }
        public IValue[] Constants { get; }
        public ExportedSymbol[] ExportedProperies { get; }
        public ExportedSymbol[] ExportedMethods { get; }
        public ModuleInformation ModuleInfo { get; }
        public int LoadAddress { get; }
    }

    
}
