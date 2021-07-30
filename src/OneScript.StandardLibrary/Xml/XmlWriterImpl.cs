/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [ContextClass("ЗаписьXML", "XMLWriter")]
    public class XmlWriterImpl : AutoContext<XmlWriterImpl>, IDisposable
    {
        private TextWriterWithSettings _internalTextWriter;
        private XmlTextWriter _writer;
        private XmlWriterSettingsImpl _settings = (XmlWriterSettingsImpl)XmlWriterSettingsImpl.Constructor(); 
        private int _depth;
        private Stack<Dictionary<string, string>> _nsmap = new Stack<Dictionary<string, string>>();
        private StringBuilder _stringBuffer;

        private const string DEFAULT_INDENT_STRING = "    ";

        public XmlWriterImpl()
        {
            Indent = true;
            _nsmap.Push(new Dictionary<string, string>());
        }

        private void EnterScope()
        {
            ++_depth;
            var newMap = _nsmap.Peek().ToDictionary((kv) => kv.Key, (kv) => kv.Value);
            _nsmap.Push(newMap);
        }

        private void ExitScope()
        {
            _nsmap.Pop();
            --_depth;
        }

        #region Properties

        [ContextProperty("Отступ","Indent")]
        public bool Indent { get; set; }

        [ContextProperty("КонтекстПространствИмен", "NamespaceContext")]
        public XmlNamespaceContext NamespaceContext
        {
            get
            {
                return new XmlNamespaceContext(_depth, _nsmap.Peek());
            }
        }

        [ContextProperty("Параметры", "Settings")]
        public XmlWriterSettingsImpl Settings
        {
            get { return _settings; }
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
            _internalTextWriter.TrimEndSlashes = true;
            _writer.WriteEndElement();
            _internalTextWriter.TrimEndSlashes = false;
            ExitScope();
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
            EnterScope();
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
            _nsmap.Peek()[prefix] = uri;
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

                Dispose();

                var result = _stringBuffer.ToString();
                _stringBuffer = null;
                return ValueFactory.Create(result);
            }
            else
            {
                _writer.Flush();
                _writer.Close();
                Dispose();

                return ValueFactory.Create();
            }

        }

        private void ApplySettings(IValue encodingOrSettings)
        {
            var rawEncoding = encodingOrSettings?.GetRawValue();
            if (rawEncoding is XmlWriterSettingsImpl)
            {
                _settings = rawEncoding as XmlWriterSettingsImpl;
            }
            else if ((encodingOrSettings?.SystemType ?? BasicTypes.String) == BasicTypes.String)
            {
                _settings = (XmlWriterSettingsImpl) XmlWriterSettingsImpl.Constructor(encodingOrSettings, null,
                    ValueFactory.Create(Indent), null, ValueFactory.Create(DEFAULT_INDENT_STRING));
            }
            else
            {
                throw RuntimeException.InvalidArgumentType(nameof(encodingOrSettings));
            }
        }

        [ContextMethod("ОткрытьФайл","OpenFile")]
        public void OpenFile(string path, IValue encodingOrSettings = null, IValue addBOM = null)
        {
            ApplySettings(encodingOrSettings);
            var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var clrSettings = _settings.GetClrSettings(addBOM?.AsBoolean() ?? true);
            _internalTextWriter = new TextWriterWithSettings(fs, clrSettings);
            _writer = new XmlTextWriter(_internalTextWriter);
            SetDefaultOptions();
        }

        [ContextMethod("УстановитьСтроку","SetString")]
        public void SetString(IValue encodingOrSettings = null)
        {
            ApplySettings(encodingOrSettings);
            _stringBuffer = new StringBuilder();
            _internalTextWriter = new TextWriterWithSettings(_stringBuffer, _settings.GetClrSettings());
            _writer = new XmlTextWriter(_internalTextWriter);
            SetDefaultOptions();
        }

        private void SetDefaultOptions()
        {
            if (Settings.Indent)
            {
                _writer.IndentChar = '\xfeff';
                _writer.Formatting = Formatting.Indented;
                _writer.Indentation = 1;
            }
        }

        #endregion

        private bool IsOpenForString()
        {
            return _stringBuffer != null;
        }

        private sealed class TextWriterWithSettings : TextWriter
        {
            private readonly TextWriter _baseObject;
            private readonly XmlWriterSettings _settings;

            public TextWriterWithSettings(Stream stream, XmlWriterSettings settings)
            {
                _baseObject = new StreamWriter(stream, settings.Encoding);
                _settings = settings;
            }
            
            public TextWriterWithSettings(StringBuilder buffer, XmlWriterSettings settings)
            {
                _baseObject = new StringWriter(buffer);
                _settings = settings;
            }
            
            /// <summary>
            /// Признак необходимости заменять строку ` /` на `/`.
            /// См. https://github.com/EvilBeaver/OneScript/issues/768#issuecomment-397848410
            /// </summary>
            public bool TrimEndSlashes { get; set; }
            
            public override Encoding Encoding => _settings.Encoding;
            
            public override void Write(char value)
            {
                if (value == '\xfeff')
                {
                    _baseObject.Write(_settings.IndentChars);
                }
                else
                {
                    _baseObject.Write(value);
                }
            }

            public override void Write(string value)
            {
                if (value == " /" && TrimEndSlashes)
                {
                    base.Write("/");
                    return;
                }
                base.Write(value);
            }

            // netcore3.1 использует эту перегрузку вместо старой со string
            public override void Write(char[] buffer)
            {
                if (TrimEndSlashes && 
                    buffer != default 
                    && buffer.Length == 3 
                    && buffer[0] == ' '
                    && buffer[1] == '/'
                    && buffer[2] == '>')
                {
                    Write(buffer, 1, 2);
                }
                else
                {
                    base.Write(buffer);
                }
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _baseObject.Close();
                }

                base.Dispose(disposing);
            }
        }

        public void Dispose()
        {
            if (_writer != null)
                _writer.Close();

            _writer = null;
        }

        [ScriptConstructor]
        public static XmlWriterImpl Create()
        {
            return new XmlWriterImpl();
        }

        public XmlWriter GetNativeWriter() => _writer;
    }
}
