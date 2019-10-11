﻿/*----------------------------------------------------------
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

using ScriptEngine.HostedScript.Library.Binary;
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
        public BinaryDataContext Base64Value(string data)
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
            var encMethod = GlobalsManager.GetEnum<StringEncodingMethodEnum>();
            Encoding enc;
            if (encoding != null)
                enc = TextEncodingEnum.GetEncoding(encoding);
            else
                enc = Encoding.UTF8;

            if (codeType == encMethod.URLEncoding)
                return EncodeStringImpl(sourceString, enc, false);
            else
                return EncodeStringImpl(sourceString, enc, true);
        }

        // http://en.wikipedia.org/wiki/Percent-encoding
        private string EncodeStringImpl(string sourceString, Encoding enc, bool skipUrlSymbols)
        {
            var bytes = enc.GetBytes(sourceString);
            var builder = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
            {
                byte current = bytes[i];
                if (IsUnreservedSymbol(current))
                    builder.Append((char)current);
                else
                {
                    if(skipUrlSymbols && IsUriSpecialChar(current))
                    {
                        builder.Append((char)current);
                    }
                    else
                    {
                        builder.AppendFormat("%{0:X2}", (int)current);
                    }
                }
            }

            return builder.ToString();
        }

        private bool IsUriSpecialChar(byte symbolByte)
        {
            switch(symbolByte)
            {
                case 47: // /
                case 37: // %
                case 63: // ?
                case 43: // +
                case 61: // =
                case 33: // !
                case 35: // #
                case 36: // $
                case 39: // '
                case 40: // (
                case 41: // )
                case 42: // *
                case 44: // ,
                case 58: // :
                case 59: // ;
                case 64: // @
                case 91: // [
                case 93: // ]
                    return true;
                default:
                    return false;
            }
        }

        private bool IsUnreservedSymbol(byte symbolByte)
        {
            if (symbolByte >= 65 && symbolByte <= 90 // A-Z
                || symbolByte >= 97 && symbolByte <= 122 // a-z
                || symbolByte >= 48 && symbolByte <= 57 // 0-9
                || symbolByte == 45 // -
                || symbolByte == 46 // .
                || symbolByte == 95 // _
                || symbolByte == 126 // ~
                )
                return true;
            else
                return false;
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
            if (encoding != null)
                throw new NotSupportedException("Явное указание кодировки в данной версии не поддерживается (только utf-8 согласно RFC 3986)");

            return Uri.UnescapeDataString(encodedString);
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
