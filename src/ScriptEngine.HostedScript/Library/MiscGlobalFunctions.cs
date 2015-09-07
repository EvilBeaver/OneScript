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
    [GlobalContext(Category="Прочие функции")]
    public class MiscGlobalFunctions : GlobalContextBase<MiscGlobalFunctions>
    {
        [ContextMethod("Base64Строка", "Base64String")]
        public string Base64String(BinaryDataContext data)
        {
            return Convert.ToBase64String(data.Buffer);
        }

        [ContextMethod("Base64Значение", "Base64Value")]
        public BinaryDataContext Base64String(string data)
        {
            byte[] bytes = Convert.FromBase64String(data);
            return new BinaryDataContext(bytes);
        }

        /// <summary>
        /// Кодирует строку для передачи в URL (urlencode)
        /// </summary>
        /// <param name="sourceString"></param>
        /// <param name="codeType"></param>
        /// <param name="encoding"></param>
        [ContextMethod("КодироватьСтроку", "EncodeString")]
        public string EncodeString(string sourceString, SelfAwareEnumValue<StringEncodingMethodEnum> codeType, IValue encoding = null)
        {
            if(encoding != null)
                throw new NotSupportedException("Явное указание кодировки в данной версии не поддерживается");

            var encMethod = GlobalsManager.GetEnum<StringEncodingMethodEnum>();
            if (codeType == encMethod.URLEncoding)
                return Uri.EscapeDataString(sourceString);
            else
                return Uri.EscapeUriString(sourceString);
        }

        /// <summary>
        /// Раскодирует строку из URL формата.
        /// </summary>
        /// <param name="encodedString"></param>
        /// <param name="codeType"></param>
        /// <param name="encoding"></param>
        /// <returns></returns>
        [ContextMethod("РаскодироватьСтроку", "DecodeString")]
        public string DecodeString(string encodedString, SelfAwareEnumValue<StringEncodingMethodEnum> codeType, IValue encoding = null)
        {
            throw new NotImplementedException();
        }


        public static MiscGlobalFunctions CreateInstance()
        {
            return new MiscGlobalFunctions();
        }
    }

    [SystemEnum("СпособКодированияСтроки", "StringEncodingMethod")]
    public class StringEncodingMethodEnum : EnumerationContext
    {
        private const string EV_SIMPLE = "КодировкаURL";
        private const string EV_URL = "URLВКодировкеURL";

        public StringEncodingMethodEnum(TypeDescriptor enumType, TypeDescriptor valuesType) : base(enumType, valuesType)
        {

        }

        [EnumValue(EV_SIMPLE, "URLEncoding")]
        public EnumerationValue URLEncoding
        {
            get
            {
                return this[EV_SIMPLE];
            }
        }

        [EnumValue(EV_URL, "URLInURLEncoding")]
        public EnumerationValue URLInURLEncoding
        {
            get
            {
                return this[EV_URL];
            }
        }

        public static StringEncodingMethodEnum CreateInstance()
        {
            return EnumContextHelper.CreateEnumInstance<StringEncodingMethodEnum>((t, v) => new StringEncodingMethodEnum(t, v));
        }
    }
}
