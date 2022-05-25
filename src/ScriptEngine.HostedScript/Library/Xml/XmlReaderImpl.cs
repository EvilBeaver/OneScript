/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.IO;
using System.Xml;

namespace ScriptEngine.HostedScript.Library.Xml
{
    [ContextClass("ЧтениеXML","XMLReader")]
    public class XmlReaderImpl : AutoContext<XmlReaderImpl>, IDisposable
    {
        XmlReader _reader;
        XmlTextReader _txtReader;
        XmlReaderSettingsImpl _settings = XmlReaderSettingsImpl.Constructor();

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
            var fs = File.OpenRead(path);

            InitReader(fs);
        }

        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(string content)
        {
            var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));

            InitReader(ms);
        }

        private void InitReader(Stream textInput)
        {
            if (_reader != null)
            {
                _reader.Dispose();
            }

            var settings = _settings.Settings;
            settings.IgnoreComments = true;

            _settings = new XmlReaderSettingsImpl(_settings.Version, _settings.Context, settings);

            _txtReader = new XmlTextReader(textInput, XmlNodeType.Document, _settings.Context);
            _reader = XmlReader.Create(_txtReader, _settings.Settings);
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
        public string XMLVersion => _settings.Version;

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
        public string XMLEncoding => _txtReader?.Encoding?.WebName ?? "UTF-8";

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
                return new XmlNamespaceContext(Depth, _txtReader.GetNamespacesInScope(XmlNamespaceScope.All));
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
                if (_emptyElemReadState == EmptyElemCompabilityState.EmptyElementRead)
                {
                    return GlobalsManager.GetEnum<XmlNodeTypeEnum>().FromNativeValue(XmlNodeType.EndElement);
                }
                else
                {
                    return GlobalsManager.GetEnum<XmlNodeTypeEnum>().FromNativeValue(_reader.NodeType);
                }
            }
        }

        [ContextProperty("ЭтоАтрибутПоУмолчанию", "IsDefaultAttribute")]
        public bool? IsDefaultAttribute
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
        public string Lang => _settings.Language;

        [ContextProperty("ИгнорироватьПробелы", "IgnoreWhitespace")]
        public bool IgnoreWhitespace
        {
            get { return _settings.IgnoreWhitespace; }
            set
            {
                _settings.SetIgnoringWhitespace(value);
                if (_reader != null)
                    _reader.Settings.IgnoreWhitespace = value;
            }
        }

        [ContextProperty("Параметры", "Settings")]
        public IValue Settings => _settings;

        [ContextProperty("ПробельныеСимволы", "Space")]
        public IValue Space => _settings.Space;

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
            var isEnd = (NodeType == GlobalsManager.GetEnum<XmlNodeTypeEnum>().FromNativeValue(XmlNodeType.EndElement));
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
            return GlobalsManager.GetEnum<XmlNodeTypeEnum>().FromNativeValue(nodeType);        
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
