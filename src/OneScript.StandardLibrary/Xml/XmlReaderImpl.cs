/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using System.Xml;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Xml
{
    [ContextClass("ЧтениеXML","XMLReader")]
    public class XmlReaderImpl : AutoContext<XmlReaderImpl>, IDisposable
    {
        XmlTextReader _reader;
        EmptyElemCompabilityState _emptyElemReadState = EmptyElemCompabilityState.Off;
        bool _attributesLoopReset = false;

        private enum EmptyElemCompabilityState
        {
            Off,
            EmptyElementEntered,
            EmptyElementRead
        }

        public XmlReader GetNativeReader()
        {
            return _reader;
        }

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
                return "1.0";
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

        private int Depth
        {
            get
            {
                if (_reader.NodeType == XmlNodeType.EndElement)
                    return _reader.Depth;

                if (_emptyElemReadState == EmptyElemCompabilityState.EmptyElementRead)
                    return _reader.Depth;
                
                return _reader.Depth + 1;
            }
        }

        [ContextProperty("КонтекстПространствИмен", "NamespaceContext")]
        public XmlNamespaceContext NamespaceContext
        {
            get
            {
                return new XmlNamespaceContext(Depth, _reader.GetNamespacesInScope(XmlNamespaceScope.All));
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
        public ClrEnumValueWrapper<XmlNodeType> NodeType
        {
            get
            {
                var enumInstance = XmlNodeTypeEnum.Instance;
                if (_emptyElemReadState == EmptyElemCompabilityState.EmptyElementRead)
                {
                    return enumInstance.FromNativeValue(XmlNodeType.EndElement);
                }
                else
                {
                    return enumInstance.FromNativeValue(_reader.NodeType);
                }
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
        public IValue AttributeValue(IValue indexOrName, string URIIfGiven = null)
        {
            string attributeValue = null;

            if (indexOrName.DataType == DataType.Number)
            {
                attributeValue = _reader.GetAttribute((int)indexOrName.AsNumber());
            }
            else if (indexOrName.DataType == DataType.String)
            {
                if (URIIfGiven == null)
                    attributeValue = _reader.GetAttribute(indexOrName.AsString());
                else
                    attributeValue = _reader.GetAttribute(indexOrName.AsString(), URIIfGiven);
            }
            else
            {
                throw RuntimeException.InvalidArgumentType();
            }

            if (attributeValue != null)
                return ValueFactory.Create(attributeValue);
            else
                return ValueFactory.Create();

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
        public IValue GetAttribute(IValue indexOrName, string URIIfGiven = null)
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
            if(_emptyElemReadState == EmptyElemCompabilityState.EmptyElementEntered)
            {
                _emptyElemReadState = EmptyElemCompabilityState.EmptyElementRead;
                return;
            }

            V8CompatibleSkip();
            CheckEmptyElementEntering();
        }

        private void V8CompatibleSkip()
        {
            if (_reader.NodeType == XmlNodeType.Element)
            {
                int initialDepth = _reader.Depth;
                while (_reader.Read() && _reader.Depth > initialDepth) ;
                System.Diagnostics.Debug.Assert(_reader.NodeType == XmlNodeType.EndElement);
            }
            else
            {
                _reader.Skip();
            }
        }

        [ContextMethod("Прочитать", "Read")]
        public bool Read()
        {
            if (_emptyElemReadState == EmptyElemCompabilityState.EmptyElementEntered)
            {
                _emptyElemReadState = EmptyElemCompabilityState.EmptyElementRead;
                return true;
            }
            else
            {
                bool readingDone = _reader.Read();
                CheckEmptyElementEntering();
                return readingDone;
            }
        }

        private void CheckEmptyElementEntering()
        {
            _attributesLoopReset = false;
            if (_reader.IsEmptyElement)
                _emptyElemReadState = EmptyElemCompabilityState.EmptyElementEntered;
            else
                _emptyElemReadState = EmptyElemCompabilityState.Off;
        }

        private bool IsEndElement()
        {
            var isEnd = (NodeType == XmlNodeTypeEnum.Instance.FromNativeValue(XmlNodeType.EndElement));
            return isEnd;
        }

        private bool ReadAttributeInternal()
        {
            if (IsEndElement() && !_attributesLoopReset)
            {
                _attributesLoopReset = true;
                return _reader.MoveToFirstAttribute();
            }

            return _reader.MoveToNextAttribute();
        }

        [ContextMethod("ПрочитатьАтрибут", "ReadAttribute")]
        public bool ReadAttribute()
        {
            return ReadAttributeInternal();
        }

        [ContextMethod("СледующееОбъявление", "NextDeclaration")]
        public void NextDeclaration()
        {
            throw new NotImplementedException();
        }

        [ContextMethod("СледующийАтрибут", "NextAttribute")]
        public bool NextAttribute()
        {
            return ReadAttributeInternal();
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
            var nodeType = _reader.MoveToContent();
            CheckEmptyElementEntering();
            return GlobalsHelper.GetEnum<XmlNodeTypeEnum>().FromNativeValue(nodeType);        
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
        public static XmlReaderImpl Create()
        {
            return new XmlReaderImpl();
        }

    }
}
