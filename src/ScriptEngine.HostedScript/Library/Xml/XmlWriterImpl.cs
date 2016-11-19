/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ScriptEngine.HostedScript.Library.Xml
{
    [ContextClass("ЗаписьXML", "XMLWriter")]
    public class XmlWriterImpl : AutoContext<XmlWriterImpl>, IDisposable
    {
        private XmlTextWriter _writer;
        private StringWriter _stringWriter;

        private const int INDENT_SIZE = 4;

        public XmlWriterImpl()
        {
        }

        #region Properties

        [ContextProperty("Отступ","Indent")]
        public bool Indent 
        { 
            get
            {
                return _writer.Formatting.HasFlag(Formatting.Indented);
            }
            set
            {
                if(value)
                {
                    _writer.Formatting = Formatting.Indented;
                }
                else
                {
                    _writer.Formatting = Formatting.None;
                }
            }
        }

        [ContextProperty("КонтекстПространствИмен", "NamespaceContext")]
        public object NamespaceContext
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        [ContextProperty("Параметры", "Settings")]
        public object Settings
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Methods

        [ContextMethod("ЗаписатьАтрибут","WriteAttribute")]
		public void WriteAttribute(string localName, string valueOrNamespace, string value = null)
		{
            if(value == null)
            {
                _writer.WriteAttributeString(localName, valueOrNamespace);
            }
            else
            {
                _writer.WriteAttributeString(localName, valueOrNamespace, value);
            }
        }

        [ContextMethod("ЗаписатьБезОбработки","WriteRaw")]
		public void WriteRaw(string data)
		{
            _writer.WriteRaw(data);
        }

        [ContextMethod("ЗаписатьИнструкциюОбработки","WriteProcessingInstruction")]
		public void WriteProcessingInstruction(string name, string text)
		{
            _writer.WriteProcessingInstruction(name, text);
        }

        [ContextMethod("ЗаписатьКомментарий","WriteComment")]
		public void WriteComment(string text)
		{
            _writer.WriteComment(text);
        }

        [ContextMethod("ЗаписатьКонецАтрибута","WriteEndAttribute")]
		public void WriteEndAttribute()
		{
            _writer.WriteEndAttribute();
        }

        [ContextMethod("ЗаписатьКонецЭлемента","WriteEndElement")]
		public void WriteEndElement()
		{
            _writer.WriteEndElement();
        }

        [ContextMethod("ЗаписатьНачалоАтрибута","WriteStartAttribute")]
		public void WriteStartAttribute(string name, string ns = null)
		{
            if(ns == null)
            {
                _writer.WriteStartAttribute(name);
            }
            else
            {
                _writer.WriteStartAttribute(name, ns);
            }

        }

        [ContextMethod("ЗаписатьНачалоЭлемента","WriteStartElement")]
        public void WriteStartElement(string name, string ns = null)
		{
            if (ns == null)
            {
                _writer.WriteStartElement(name);
            }
            else
            {
                _writer.WriteStartElement(name, ns);
            }
        }

        [ContextMethod("ЗаписатьОбъявлениеXML","WriteXMLDeclaration")]
		public void WriteXMLDeclaration()
		{
            _writer.WriteStartDocument();
        }

        [ContextMethod("ЗаписатьСекциюCDATA","WriteCDATASection")]
		public void WriteCDATASection(string data)
		{
            _writer.WriteCData(data);
        }

        [ContextMethod("ЗаписатьСоответствиеПространстваИмен","WriteNamespaceMapping")]
		public void WriteNamespaceMapping(string prefix, string uri)
		{
            _writer.WriteAttributeString("xmlns", prefix, null, uri);
        }

        [ContextMethod("ЗаписатьСсылкуНаСущность","WriteEntityReference")]
		public void WriteEntityReference(string name)
		{
            _writer.WriteEntityRef(name);
        }

        [ContextMethod("ЗаписатьТекст","WriteText")]
		public void WriteText(string text)
		{
            _writer.WriteString(text);
        }

        [ContextMethod("ЗаписатьТекущий","WriteCurrent")]
		public void WriteCurrent(XmlReaderImpl reader)
		{
            _writer.WriteNode(reader.GetNativeReader(), false);
        }

        [ContextMethod("ЗаписатьТипДокумента","WriteDocumentType")]
		public void WriteDocumentType(string name, string varArg2, string varArg3 = null, string varArg4 = null)
		{
            if(varArg4 != null)
            {
                _writer.WriteDocType(name, varArg2, varArg3, varArg4);
            }
            else if(varArg3 != null)
            {
                _writer.WriteDocType(name, null, varArg2, varArg3);
            }
            else
            {
                _writer.WriteDocType(name, null, null, varArg2);
            }
        }

        [ContextMethod("НайтиПрефикс","LookupPrefix")]
		public IValue LookupPrefix(string uri)
		{
            string prefix = _writer.LookupPrefix(uri);
            if (prefix == null)
                return ValueFactory.Create();
            return ValueFactory.Create(prefix);
        }

        [ContextMethod("Закрыть","Close")]
		public IValue Close()
		{
            if(IsOpenForString())
            {
                _writer.Flush();
                _writer.Close();
                _stringWriter.Close();

                var sb = _stringWriter.GetStringBuilder();
                Dispose();

                return ValueFactory.Create(sb.ToString());
            }
            else
            {
                _writer.Flush();
                _writer.Close();
                Dispose();

                return ValueFactory.Create();
            }

        }

        [ContextMethod("ОткрытьФайл","OpenFile")]
		public void OpenFile(string path, string encoding = null, IValue addBOM = null)
		{
            Encoding enc;
            if (addBOM == null)
                enc = TextEncodingEnum.GetEncodingByName(encoding, true);
            else
                enc = TextEncodingEnum.GetEncodingByName(encoding, addBOM.AsBoolean());

            _writer = new XmlTextWriter(path, enc);
            _stringWriter = null;
            SetDefaultOptions();
        }

        [ContextMethod("УстановитьСтроку","SetString")]
		public void SetString(string encoding = null)
		{
            Encoding enc = TextEncodingEnum.GetEncodingByName(encoding, true);
            _stringWriter = new StringWriterWithEncoding(enc);            
            _writer = new XmlTextWriter(_stringWriter);
            SetDefaultOptions();
        }

        private void SetDefaultOptions()
        {
            _writer.Indentation = INDENT_SIZE;
            this.Indent = true;
        }

        #endregion

        private bool IsOpenForString()
        {
            return _stringWriter != null;
        }

        private sealed class StringWriterWithEncoding : StringWriter
        {
            private readonly Encoding encoding;

            public StringWriterWithEncoding(Encoding encoding)
            {
                this.encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return encoding; }
            }
        }

        public void Dispose()
        {
            if (_writer != null)
                _writer.Close();
            if (_stringWriter != null)
                _stringWriter.Dispose();

            _writer = null;
            _stringWriter = null;
        }

        [ScriptConstructor]
        public static XmlWriterImpl Create()
        {
            return new XmlWriterImpl();
        }

    }
}
