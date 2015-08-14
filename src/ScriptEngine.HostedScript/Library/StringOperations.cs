/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category = "Операции с строками")]
    public class StringOperations : GlobalContextBase<StringOperations>
    {
        readonly int STRTEMPLATE_ID;
        const string STRTEMPLATE_NAME_RU = "СтрШаблон";
        const string STRTEMPLATE_NAME_EN = "StrTemplate";

        public StringOperations()
        {
            STRTEMPLATE_ID = this.Methods.Count;
        }

        /// <summary>
        /// Определяет, что строка начинается с указанной подстроки.
        /// </summary>
        /// <param name="inputString">Строка, начало которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое начало строки. В случае если переданное значение является пустой строкой генерируется исключительная ситуация.</param>
        [ContextMethod("СтрНачинаетсяС", "StrStartWith")]
        public bool StrStartWith(string inputString, string searchString)
        {
            bool result = false;

            if(!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.StartsWith(searchString);
                }
                else throw new RuntimeException("Ошибка при вызове метода контекста (СтрНачинаетсяС): Недопустимое значение параметра (параметр номер '2')"); 
            }

            return result;
        }

        /// <summary>
        /// Определяет, заканчивается ли строка указанной подстрокой.
        /// </summary>
        /// <param name="inputString">Строка, окончание которой проверяется на совпадение с подстрокой поиска.</param>
        /// <param name="searchString">Строка, содержащая предполагаемое окончание строки. В случае если переданное значение является пустой строкой генерируется исключительная ситуация.</param>
        [ContextMethod("СтрЗаканчиваетсяНа", "StrEndsWith")]
        public bool StrEndsWith(string inputString, string searchString)
        {
            bool result = false;

            if(!string.IsNullOrEmpty(inputString))
            {
                if (!string.IsNullOrEmpty(searchString))
                {
                    result = inputString.EndsWith(searchString);
                }
                else throw new RuntimeException("Ошибка при вызове метода контекста (СтрЗаканчиваетсяНа): Недопустимое значение параметра (параметр номер '2')"); 
            }

            return result;
        }

        /// <summary>
        /// Разделяет строку на части по указанным символам-разделителям.
        /// </summary>
        /// <param name="inputString">Разделяемая строка.</param>
        /// <param name="stringDelimiter">Строка символов, каждый из которых является индивидуальным разделителем.</param>
        /// <param name="includeEmpty">Указывает необходимость включать в результат пустые строки, которые могут образоваться в результате разделения исходной строки. Значение по умолчанию: Истина. </param>
        [ContextMethod("СтрРазделить", "StrSplit")]
        public ArrayImpl StrSplit(string inputString, string stringDelimiter, bool includeEmpty = true)
        {
            ArrayImpl arrResult = new ArrayImpl();
            string[] arrParsed;
            if(!string.IsNullOrEmpty(inputString))
            {
                if(!string.IsNullOrEmpty(stringDelimiter))
                {
                    arrParsed = inputString.Split(new string[] { stringDelimiter }, includeEmpty ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    arrParsed = new string[] { inputString };
                }
            } else
            {
                arrParsed = new string[] { string.Empty };
            }
            arrResult = new ArrayImpl(arrParsed.Select(x => ValueFactory.Create(x)));
            return arrResult;
        }

        public override int FindMethod(string name)
        {
            if (string.Compare(name, STRTEMPLATE_NAME_RU, true) == 0 || string.Compare(name, STRTEMPLATE_NAME_EN, true) == 0)
                return STRTEMPLATE_ID;
            else
                return base.FindMethod(name);
        }

        public override IEnumerable<MethodInfo> GetMethods()
        {
            var fullList = new List<MethodInfo>(base.GetMethods());
            var strTemplateMethodInfo = CreateStrTemplateMethodInfo();

            fullList.Add(strTemplateMethodInfo);
            return fullList;

        }

        private static MethodInfo CreateStrTemplateMethodInfo()
        {
            var strTemplateMethodInfo = new MethodInfo();
            strTemplateMethodInfo.IsFunction = true;
            strTemplateMethodInfo.Name = STRTEMPLATE_NAME_RU;
            strTemplateMethodInfo.Alias = STRTEMPLATE_NAME_EN;
            strTemplateMethodInfo.Params = new ParameterDefinition[11];

            strTemplateMethodInfo.Params[0] = new ParameterDefinition()
            {
                IsByValue = true
            };

            for (int i = 1; i < strTemplateMethodInfo.Params.Length; i++)
            {
                strTemplateMethodInfo.Params[i] = new ParameterDefinition()
                {
                    IsByValue = true,
                    HasDefaultValue = true
                };
            }
            return strTemplateMethodInfo;
        }

        public override MethodInfo GetMethodInfo(int methodNumber)
        {
            if (methodNumber == STRTEMPLATE_ID)
                return CreateStrTemplateMethodInfo();
            else
                return base.GetMethodInfo(methodNumber);
        }

        public override void CallAsProcedure(int methodNumber, IValue[] arguments)
        {
            if (methodNumber == STRTEMPLATE_ID)
                CallStrTemplate(arguments);
            else
                base.CallAsProcedure(methodNumber, arguments);
        }

        public override void CallAsFunction(int methodNumber, IValue[] arguments, out IValue retValue)
        {
            if (methodNumber == STRTEMPLATE_ID)
                retValue = CallStrTemplate(arguments);
            else
                base.CallAsFunction(methodNumber, arguments, out retValue);
        }

        private IValue CallStrTemplate(IValue[] arguments)
        {
            var srcFormat = arguments[0].AsString();
            if (srcFormat == String.Empty)
                return ValueFactory.Create("");

            var re = new System.Text.RegularExpressions.Regex(@"(%%)|(%\d+)|(%\D)");
            int matchCount = 0;
            int passedArgsCount = arguments.Skip(1).Where(x => x != null).Count();
            var result = re.Replace(srcFormat, (m) =>
            {
                if (m.Groups[1].Success)
                    return "%";
                
                if(m.Groups[2].Success)
                {
                    matchCount++;
                    var number = int.Parse(m.Groups[2].Value.Substring(1));
                    if (number < 1 || number > 11)
                        throw new RuntimeException("Ошибка при вызове метода контекста (СтрШаблон): Ошибка синтаксиса шаблона в позиции " + (m.Index + 1));

                    if (arguments[number] != null)
                        return arguments[number].AsString();
                    else
                        return "";
                }

                throw new RuntimeException("Ошибка при вызове метода контекста (СтрШаблон): Ошибка синтаксиса шаблона в позиции " + (m.Index + 1));

            });

            if (passedArgsCount > matchCount)
                throw RuntimeException.TooManyArgumentsPassed();

            return ValueFactory.Create(result);
        }

        public static IAttachableContext CreateInstance()
        {
            return new StringOperations();
        }

    }
}
