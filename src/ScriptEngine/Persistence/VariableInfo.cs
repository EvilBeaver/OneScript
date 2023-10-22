using System;
using ScriptEngine.Machine;

namespace ScriptEngine.Persistence
{
    [Serializable]
    public struct VariableInfo
    {
        public int Index;
        public string Identifier;

        public AnnotationDefinition[] Annotations;

        public int AnnotationsCount => Annotations?.Length ?? 0;

        public override string ToString()
        {
            return $"{Index}:{Identifier}";
        }
    }
}