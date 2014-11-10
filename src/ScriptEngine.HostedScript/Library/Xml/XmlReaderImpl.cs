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
    [ContextClass("ЧтениеXML","XMLReader")]
    public class XmlReaderImpl : AutoContext<XmlReaderImpl>, IDisposable
    {
        XmlTextReader _reader;

        [ContextMethod("ОткрытьФайл", "OpenFile")]
        public void OpenFile(string path)
        {
            if (_reader != null)
                throw new RuntimeException("Поток XML уже открыт");
            var textInput = new StreamReader(path);
            InitReader(textInput);
        }

        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(string content)
        {
            if (_reader != null)
                throw new RuntimeException("Поток XML уже открыт");

            var textInput = new StringReader(content);
            InitReader(textInput);
        }

        private void InitReader(TextReader textInput)
        {
            _reader = new XmlTextReader(textInput);
            _reader.WhitespaceHandling = WhitespaceHandling.Significant;
        }

        private void CheckIfOpen()
        {
            if (_reader == null)
                throw new RuntimeException("Файл не открыт");
        }

        #region Свойства
        
        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string NamespaceURI
        {
            get
            {
                return _reader.NamespaceURI;
            }
        }

        [ContextProperty("Автономный", "Standalone")]
        public bool Standalone
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("БазовыйURI", "BaseURI")]
        public string BaseURI
        {
            get
            {
                return _reader.BaseURI;
            }
        }

        [ContextProperty("ВерсияXML", "XMLVersion")]
        public string XMLVersion
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("Значение", "Value")]
        public string Value
        {
            get
            {
                return _reader.Value;
            }
        }

        [ContextProperty("ИмеетЗначение", "HasValue")]
        public bool HasValue
        {
            get
            {
                return _reader.HasValue;
            }
        }

        [ContextProperty("ИмеетИмя", "HasName")]
        public bool HasName
        {
            get
            {
                return _reader.LocalName != String.Empty;
            }
        }

        [ContextProperty("Имя", "Name")]
        public string Name
        {
            get
            {
                return _reader.Name;
            }
        }

        [ContextProperty("ИмяНотации", "NotationName")]
        public string NotationName
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("КодировкаXML", "XMLEncoding")]
        public string XMLEncoding
        {
            get
            {
                return _reader.Encoding.WebName;
            }
        }

        [ContextProperty("КодировкаИсточника", "InputEncoding")]
        public string InputEncoding
        {
            get
            {
                return XMLEncoding;
            }
        }

        [ContextProperty("КонтекстПространствИмен", "NamespaceContext")]
        public object NamespaceContext
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("ЛокальноеИмя", "LocalName")]
        public string LocalName
        {
            get
            {
                return _reader.LocalName;
            }
        }

        [ContextProperty("Префикс", "Prefix")]
        public string Prefix
        {
            get
            {
                return _reader.Prefix;
            }
        }

        [ContextProperty("ПубличныйИдентификатор", "PublicId")]
        public string PublicId
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("СистемныйИдентификатор", "SystemId")]
        public string SystemId
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("ТипУзла", "NodeType")]
        public IValue NodeType
        {
            get
            {
                return XmlNodeTypeEnum.GetInstance().FromNativeValue(_reader.NodeType);
            }
        }

        [ContextProperty("ЭтоАтрибутПоУмолчанию", "IsDefaultAttribute")]
        public bool IsDefaultAttribute
        {
            get
            {
                return _reader.IsDefault;
            }
        }

        [ContextProperty("ЭтоПробельныеСимволы", "IsWhitespace")]
        public bool IsWhitespace
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("Язык", "Lang")]
        public string Lang
        {
            get
            {
                return _reader.XmlLang;
            }
        }

        [ContextProperty("ИгнорироватьПробелы", "IgnoreWhitespace")]
        public bool IgnoreWhitespace
        {
            get
            {
                return _reader.WhitespaceHandling == WhitespaceHandling.None;
            }
            set
            {
                _reader.WhitespaceHandling = value ? WhitespaceHandling.None : WhitespaceHandling.All;
            }
        }

        [ContextProperty("Параметры", "Settings")]
        public object Settings
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        [ContextProperty("ПробельныеСимволы", "Space")]
        public object Space
        {
            get
            {
                throw new NotImplementedException();
                //return _reader.XmlSpace;
            }
        }

        [ContextProperty("ЭтоСимвольныеДанные", "IsCharacters")]
        public bool IsCharacters
        {
            get
            {
                return _reader.NodeType == XmlNodeType.Text || _reader.NodeType == XmlNodeType.CDATA || _reader.NodeType == XmlNodeType.SignificantWhitespace;
            }
        } 
        #endregion

        #region Методы
        [ContextMethod("URIПространстваИменАтрибута", "AttributeNamespaceURI")]
        public string AttributeNamespaceURI(int index)
        {
            throw new NotImplementedException();
        }

        [ContextMethod("ЗначениеАтрибута", "AttributeValue")]
        public string AttributeValue(IValue indexOrName, string URIIfGiven = null)
        {
            if (indexOrName.DataType == DataType.Number)
            {
                return _reader.GetAttribute((int)indexOrName.AsNumber());
            }
            else if (indexOrName.DataType == DataType.String)
            {
                if (URIIfGiven == null)
                    return _reader.GetAttribute(indexOrName.AsString());
                else
                    return _reader.GetAttribute(indexOrName.AsString(), URIIfGiven);
            }
            else
            {
                throw RuntimeException.InvalidArgumentType();
            }
        }

        [ContextMethod("ИмяАтрибута", "AttributeName")]
        public string AttributeName(int index)
        {
            _reader.MoveToAttribute(index);
            var name = _reader.Name;
            _reader.MoveToElement();

            return name;
        }
        [ContextMethod("КоличествоАтрибутов", "AttributeCount")]
        public int AttributeCount()
        {
            return _reader.AttributeCount;
        }

        [ContextMethod("ЛокальноеИмяАтрибута", "AttributeLocalName")]
        public string AttributeLocalName(int index)
        {
            _reader.MoveToAttribute(index);
            var name = _reader.LocalName;
            _reader.MoveToElement();

            return name;
        }

        [ContextMethod("ПервоеОбъявление", "FirstDeclaration")]
        public bool FirstDeclaration()
        {
            throw new NotImplementedException();
        }

        [ContextMethod("ПервыйАтрибут", "FirstAttribute")]
        public bool FirstAttribute()
        {
            return _reader.MoveToFirstAttribute();
        }

        [ContextMethod("ПолучитьАтрибут", "GetAttribute")]
        public string GetAttribute(IValue indexOrName, string URIIfGiven = null)
        {
            return AttributeValue(indexOrName, URIIfGiven);
        }

        [ContextMethod("ПрефиксАтрибута", "AttributePrefix")]
        public string AttributePrefix(int index)
        {
            _reader.MoveToAttribute(index);
            var name = _reader.Prefix;
            _reader.MoveToElement();

            return name;
        }

        [ContextMethod("Пропустить", "Skip")]
        public void Skip()
        {
            _reader.Skip();
        }

        [ContextMethod("Прочитать", "Read")]
        public bool Read()
        {
            return _reader.Read();
        }

        [ContextMethod("ПрочитатьАтрибут", "ReadAttribute")]
        public bool ReadAttribute()
        {
            return _reader.MoveToNextAttribute();
        }

        [ContextMethod("СледующееОбъявление", "NextDeclaration")]
        public void NextDeclaration()
        {
            throw new NotImplementedException();
        }

        [ContextMethod("СледующийАтрибут", "NextAttribute")]
        public bool NextAttribute()
        {
            return _reader.MoveToNextAttribute();
        }

        [ContextMethod("ТипАтрибута", "AttributeType")]
        public void AttributeType()
        {
            throw new NotImplementedException();
        }

        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        [ContextMethod("ПерейтиКСодержимому", "MoveToContent")]
        public IValue MoveToContent()
        {
            return XmlNodeTypeEnum.GetInstance().FromNativeValue(_reader.MoveToContent());
        } 

        #endregion

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Create()
        {
            return new XmlReaderImpl();
        }

    }
}
