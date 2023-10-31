/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace ScriptEngine.Machine
{
    [Serializable]
    public struct AnnotationParameter
    {
        public string Name;
        public int ValueIndex;

        [NonSerialized]
        public IValue RuntimeValue;
        
        public const int UNDEFINED_VALUE_INDEX = -1;

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return $"[{ValueIndex}]";
            }
            if (ValueIndex == UNDEFINED_VALUE_INDEX)
            {
                return Name;
            }
            return $"{Name}=[{ValueIndex}]";
        }

        public static AnnotationParameter FromString(string presentation)
        {
            var result = new AnnotationParameter();
            var parts = presentation.Split('=');
            if (parts.Length == 1)
            {
                if (parts[0].StartsWith("["))
                {
                    result.ValueIndex = AnnotationValueConstIndex(parts[0]);
                }
                else
                {
                    result.Name = parts[0];
                }
            }
            else
            {
                result.Name = parts[0];
                result.ValueIndex = AnnotationValueConstIndex(parts[1]);
            }

            return result;
        }

        private static int AnnotationValueConstIndex(string valuePresentation)
        {
            // убираем [] по краям
            var intPresentation = valuePresentation.Substring(1, valuePresentation.Length - 2);
            return int.Parse(intPresentation);
        }
    }
}