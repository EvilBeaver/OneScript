/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Text;

namespace ScriptEngine.Machine
{
    [Serializable]
    public struct AnnotationDefinition
    {
        public string Name;
        public AnnotationParameter[] Parameters;

        public int ParamCount => Parameters?.Length ?? 0;

        public string ToConstValue()
        {
            var builder = new StringBuilder("&");
            builder.Append(Name);
            builder.Append("(");
            builder.Append(string.Join(",", Parameters));
            builder.Append(")");
            
            return builder.ToString();
        }

        /// <summary>
        /// Разбирает объект из сериализованного представления значения аннотации.
        /// </summary>
        /// <param name="constValuePresentation">Сериализованная аннотация. Строка вида `&ИмяАннотации(ИмяПараметра=ЗначениеПараметра,...)`</param>
        /// <returns></returns>
        public static AnnotationDefinition FromConstValue(string constValuePresentation)
        {
            var result = new AnnotationDefinition();

            var nameEnd = constValuePresentation.IndexOf("(", StringComparison.Ordinal);
            const int prefixLength = 1; // Длина строки &
            result.Name = constValuePresentation.Substring(prefixLength, nameEnd - prefixLength);
            
            var paramsEnd = constValuePresentation.LastIndexOf(")", StringComparison.Ordinal);
            var paramsPresentation = constValuePresentation.Substring(nameEnd + 1, paramsEnd - nameEnd - 1);

            var paramsSplitted = paramsPresentation.Split(',');
            result.Parameters = new AnnotationParameter[paramsSplitted.Length];
            for (var i = 0; i < paramsSplitted.Length; i++)
            {
                result.Parameters[i] = AnnotationParameter.FromString(paramsSplitted[i]);
            }
            
            return result;
        }
    }
}