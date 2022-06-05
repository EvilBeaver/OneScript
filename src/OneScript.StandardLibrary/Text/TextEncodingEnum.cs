/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Text;
using OneScript.Commons;
using OneScript.Contexts.Enums;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Text
{
    [SystemEnum("КодировкаТекста", "TextEncoding")]
    public class TextEncodingEnum : EnumerationContext
    {
        private enum TextEncodingValues
        {
            System,
            ANSI,
            OEM,
            UTF16,
            UTF8,
            UTF8NoBOM
        }

        private TextEncodingEnum(TypeDescriptor typeRepresentation, TypeDescriptor valuesType)
            : base(typeRepresentation, valuesType)
        {
            System = this.WrapClrValue("Системная", "System", TextEncodingValues.System);
            Ansi = this.WrapClrValue("ANSI", default, TextEncodingValues.ANSI);
            Oem = this.WrapClrValue("OEM", default, TextEncodingValues.OEM);
            Utf16 = this.WrapClrValue("UTF16", default, TextEncodingValues.UTF16);
            Utf8 = this.WrapClrValue("UTF8", default, TextEncodingValues.UTF8);
            Utf8NoBOM = this.WrapClrValue("UTF8БезBOM", "UTF8NoBOM", TextEncodingValues.UTF8NoBOM);
        }

        public EnumerationValue Ansi { get; }

        public EnumerationValue Oem { get; }

        public EnumerationValue Utf16 { get; }

        public EnumerationValue Utf8 { get; }

        public EnumerationValue Utf8NoBOM { get; }

        public EnumerationValue System { get; }

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
            var instance = EnumContextHelper.CreateClrEnumInstance<TextEncodingEnum, System.IO.DriveType>(
                typeManager,
                (t, v) => new TextEncodingEnum(t, v));

            return instance;
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
                if (!(encoding.GetRawValue() is ClrEnumValueWrapper<TextEncodingValues> encValue))
                    throw RuntimeException.InvalidArgumentType();

                var encodingEnum = (TextEncodingEnum)encValue.Owner;

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
