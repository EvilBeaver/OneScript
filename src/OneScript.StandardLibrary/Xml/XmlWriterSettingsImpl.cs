/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Xml;
using OneScript.StandardLibrary.Text;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    /// <summary>
    /// Параметры, используемые для формирования XML
    /// </summary>
    [ContextClass("ПараметрыЗаписиXML", "XMLWriterSettings")]
    public class XmlWriterSettingsImpl : AutoContext<XmlWriterSettingsImpl>
    {

        public XmlWriterSettingsImpl(string encoding, string version, bool indent, bool indentAttributes,
            string indentChars)
        {
            Version = version;
            Encoding = encoding;
            Indent = indent;
            IndentAttributes = indentAttributes;
            IndentChars = indentChars;
        }

        /// <summary>
        /// Версия XML
        /// </summary>
        [ContextProperty("Версия", "Version")]
        public string Version { get; }
        
        /// <summary>
        /// Кодировка
        /// </summary>
        [ContextProperty("Кодировка", "Encoding")]
        public string Encoding { get; }
        
        /// <summary>
        /// Использование отступа для вложенных элементов
        /// </summary>
        [ContextProperty("Отступ", "Indent")]
        public bool Indent { get; }
        
        /// <summary>
        /// Использование отступов для атрибутов
        /// </summary>
        [ContextProperty("ОтступАтрибутов", "IndentAttributes")]
        public bool IndentAttributes { get; }
        
        /// <summary>
        /// Символы, используемые для формирования отступа
        /// </summary>
        [ContextProperty("СимволыОтступа", "IndentChars")]
        public string IndentChars { get; }

        public XmlWriterSettings GetClrSettings(bool addBom = false)
        {
            var result = new XmlWriterSettings
            {
                Indent = Indent,
                IndentChars = IndentChars,
                NewLineOnAttributes = IndentAttributes,
                Encoding = TextEncodingEnum.GetEncodingByName(Encoding, addBom),
                NewLineChars = System.Environment.NewLine,
                NamespaceHandling = NamespaceHandling.OmitDuplicates
            };
            return result;
        }

        [ScriptConstructor]
        public static XmlWriterSettingsImpl Constructor(IValue encoding = null,
            IValue version = null, IValue indent = null, IValue indentAttributes = null,
            IValue indentChars = null)
        {
            var _indent = ContextValuesMarshaller.ConvertParam<bool>(indent, true);
            var _indentAttributes = ContextValuesMarshaller.ConvertParam<bool>(indentAttributes);

            return new XmlWriterSettingsImpl(encoding?.AsString() ?? "UTF-8",
                version?.AsString() ?? "1.0",
                _indent,
                _indentAttributes,
                indentChars?.AsString() ?? "\t");
        }
    }
}