/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text;
using OneScript.Commons;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Text
{
    [SystemEnum("КодировкаТекста", "TextEncoding")]
    public class TextEncodingEnum : EnumerationContext
    {
        private const string ENCODING_ANSI = "ANSI";
        private const string ENCODING_OEM = "OEM";
        private const string ENCODING_UTF16 = "UTF16";
        private const string ENCODING_UTF8 = "UTF8";
        private const string ENCODING_UTF8NoBOM = "UTF8NoBOM";
        private const string ENCODING_SYSTEM = "Системная";

        private TextEncodingEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
        }

        [EnumValue(ENCODING_ANSI)]
        public EnumerationValue Ansi
        {
            get
            {
                return this[ENCODING_ANSI];
            }
        }

        [EnumValue(ENCODING_OEM)]
        public EnumerationValue Oem
        {
            get
            {
                return this[ENCODING_OEM];
            }
        }

        [EnumValue(ENCODING_UTF16)]
        public EnumerationValue Utf16
        {
            get
            {
                return this[ENCODING_UTF16];
            }
        }

        [EnumValue(ENCODING_UTF8)]
        public EnumerationValue Utf8
        {
            get
            {
                return this[ENCODING_UTF8];
            }
        }

        [EnumValue(ENCODING_UTF8NoBOM)]
        public EnumerationValue Utf8NoBOM
        {
            get
            {
                return this[ENCODING_UTF8NoBOM];
            }
        }

        [EnumValue(ENCODING_SYSTEM, "System")]
        public EnumerationValue System
        {
            get
            {
                return this[ENCODING_SYSTEM];
            }
        }

        public EnumerationValue GetValue(Encoding encoding)
        {
            if (encoding.Equals(Encoding.GetEncoding(866)))
                return Oem;

            if (encoding.Equals(Encoding.GetEncoding(1251)))
                return Ansi;

            if (encoding.Equals(new UnicodeEncoding(false, true)))
                return Utf16;

            if (encoding.Equals(new UTF8Encoding(true)))
                return Utf8;

            if (encoding.Equals(new UTF8Encoding(false)))
                return Utf8NoBOM;

            if (encoding.Equals(Encoding.Default))
                return System;

            throw RuntimeException.InvalidArgumentValue();
        }

        public static TextEncodingEnum CreateInstance(ITypeManager typeManager)
        {
            return EnumContextHelper.CreateSelfAwareEnumInstance(typeManager, 
                (t,v)=>new TextEncodingEnum(t,v));
        }

        public static Encoding GetEncodingByName(string encoding, bool addBOM = true)
        {
            Encoding enc;
            if (string.IsNullOrEmpty(encoding))
                enc = new UTF8Encoding(addBOM);
            else
            {
                switch (encoding.Trim().ToUpper())
                {
                    case "UTF-8":
                        enc = new UTF8Encoding(addBOM);
                        break;
                    case "UTF-16":
                    case "UTF-16LE":
                    // предположительно, варианты UTF16_PlatformEndian\UTF16_OppositeEndian
                    // зависят от платформы x86\m68k\SPARC. Пока нет понимания как корректно это обработать.
                    // Сейчас сделано исходя из предположения что PlatformEndian должен быть LE поскольку 
                    // платформа x86 более широко распространена
                    case "UTF16_PLATFORMENDIAN":
                        enc = new UnicodeEncoding(false, addBOM);
                        break;
                    case "UTF-16BE":
                    case "UTF16_OPPOSITEENDIAN":
                        enc = new UnicodeEncoding(true, addBOM);
                        break;
                    case "UTF-32":
                    case "UTF-32LE":
                    case "UTF32_PLATFORMENDIAN":
                        enc = new UTF32Encoding(false, addBOM);
                        break;
                    case "UTF-32BE":
                    case "UTF32_OPPOSITEENDIAN":
                        enc = new UTF32Encoding(true, addBOM);
                        break;
                    default:
                        try
                        {
                            enc = Encoding.GetEncoding(encoding);
                            break;
                        }
                        catch
                        {
                            throw RuntimeException.InvalidEncoding(encoding);
                        }
                }
            }

            return enc;
        }

        public static Encoding GetEncoding(IValue encoding, bool addBOM = true)
        {
            if (encoding.SystemType == BasicTypes.String)
                return GetEncodingByName(encoding.AsString(), addBOM);
            else
            {
                if (!(encoding.GetRawValue() is SelfAwareEnumValue<TextEncodingEnum> encValue))
                    throw RuntimeException.InvalidArgumentType();

                var encodingEnum = GlobalsHelper.GetEnum<TextEncodingEnum>();

                Encoding enc;
                if (encValue == encodingEnum.Ansi)
                    enc = Encoding.GetEncoding(1251);
                else if (encValue == encodingEnum.Oem)
                    enc = Encoding.GetEncoding(866);
                else if (encValue == encodingEnum.Utf16)
                    enc = new UnicodeEncoding(false, addBOM);
                else if (encValue == encodingEnum.Utf8)
                    enc = new UTF8Encoding(addBOM);
                else if (encValue == encodingEnum.Utf8NoBOM)
                    enc = new UTF8Encoding(false);
                else if (encValue == encodingEnum.System)
                    enc = Encoding.Default;
                else
                    throw RuntimeException.InvalidArgumentValue();

                return enc;
            }
        }
    }
}
