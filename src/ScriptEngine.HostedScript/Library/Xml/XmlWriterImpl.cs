using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ScriptEngine.HostedScript.Library.Xml
{
    [ContextClass("ЗаписьXML", "XMLWriter")]
    public class XmlWriterImpl : AutoContext<XmlWriterImpl>
    {
        private XmlTextWriter _writer;
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
		public void WriteAttribute()
		{}
        [ContextMethod("ЗаписатьБезОбработки","WriteRaw")]
		public void WriteRaw()
		{}
        [ContextMethod("ЗаписатьИнструкциюОбработки","WriteProcessingInstruction")]
		public void WriteProcessingInstruction()
		{}
        [ContextMethod("ЗаписатьКомментарий","WriteComment")]
		public void WriteComment()
		{}
        [ContextMethod("ЗаписатьКонецАтрибута","WriteEndAttribute")]
		public void WriteEndAttribute()
		{}
        [ContextMethod("ЗаписатьКонецЭлемента","WriteEndElement")]
		public void WriteEndElement()
		{}
        [ContextMethod("ЗаписатьНачалоАтрибута","WriteStartAttribute")]
		public void WriteStartAttribute()
		{}
        [ContextMethod("ЗаписатьНачалоЭлемента","WriteStartElement")]
		public void WriteStartElement()
		{}
        [ContextMethod("ЗаписатьОбъявлениеXML","WriteXMLDeclaration")]
		public void WriteXMLDeclaration()
		{}
        [ContextMethod("ЗаписатьСекциюCDATA","WriteCDATASection")]
		public void WriteCDATASection()
		{}
        [ContextMethod("ЗаписатьСоответствиеПространстваИмен","WriteNamespaceMapping")]
		public void WriteNamespaceMapping()
		{}
        [ContextMethod("ЗаписатьСсылкуНаСущность","WriteEntityReference")]
		public void WriteEntityReference()
		{}
        [ContextMethod("ЗаписатьТекст","WriteText")]
		public void WriteText()
		{}
        [ContextMethod("ЗаписатьТекущий","WriteCurrent")]
		public void WriteCurrent()
		{}
        [ContextMethod("ЗаписатьТипДокумента","WriteDocumentType")]
		public void WriteDocumentType()
		{}
        [ContextMethod("НайтиПрефикс","LookupPrefix")]
		public void LookupPrefix()
		{}
        [ContextMethod("Закрыть","Close")]
		public void Close()
		{}
        [ContextMethod("ОткрытьФайл","OpenFile")]
		public void OpenFile()
		{}
        [ContextMethod("УстановитьСтроку","SetString")]
		public void SetString()
		{}

        #endregion

        [ScriptConstructor]
        public static XmlWriterImpl Create()
        {
            return new XmlWriterImpl();
        }
    }
}
