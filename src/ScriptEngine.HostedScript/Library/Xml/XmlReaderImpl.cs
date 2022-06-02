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
        bool _ignoreWhitespace = true;
        bool _ignoreWSChanged = false;

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
        public void OpenFile(string path, XmlReaderSettingsImpl settings=null)
        {
            _settings = settings ?? XmlReaderSettingsImpl.Create();
            _txtReader = new XmlTextReader(File.OpenRead(path), XmlNodeType.Document, _settings.Context);

            InitReader();
        }

        [ContextMethod("УстановитьСтроку", "SetString")]
        public void SetString(string content, XmlReaderSettingsImpl settings = null)
        {
            _settings = settings ?? XmlReaderSettingsImpl.Create();
            _txtReader = new XmlTextReader(content, XmlNodeType.Document, _settings.Context);

            InitReader();
        }

        private void InitReader()
        {
            if (_reader != null)
                _reader.Dispose();

            _ignoreWhitespace = _settings.IgnoreWhitespace;
            if (_settings.UseIgnorableWhitespace)
                _settings.Settings.IgnoreWhitespace = false;

            _reader = XmlReader.Create(_txtReader, _settings.Settings);

            _ignoreWSChanged = false;
            _emptyElemReadState = EmptyElemCompabilityState.Off;
            _attributesLoopReset = false;
        }

        #region Свойства
        
        [ContextProperty("Параметры", "Settings")]
        public IValue Settings => _settings;

        [ContextProperty("ПробельныеСимволы", "Space")]
        public IValue Space => _settings.Space;

        [ContextProperty("ВерсияXML", "XMLVersion")]
        public string XMLVersion => _settings.Version;

        [ContextProperty("Язык", "Lang")]
        public string Lang => _settings.Language;

        [ContextProperty("ИгнорироватьПробелы", "IgnoreWhitespace")]
        public bool IgnoreWhitespace
        {
            get { return _ignoreWhitespace; }
            set
            {
                if (value == _ignoreWhitespace)
                    return;

                _ignoreWhitespace = value;

                if (_settings.UseIgnorableWhitespace)
                    return;

                var settings = _settings.Settings.Clone();
                settings.IgnoreWhitespace = _ignoreWhitespace;
                
                _reader = XmlReader.Create(_txtReader, settings);
                _ignoreWSChanged = (_reader.ReadState != _txtReader.ReadState);
            }
        }

        [ContextProperty("КодировкаXML", "XMLEncoding")]
        public string XMLEncoding => _txtReader?.Encoding?.WebName ?? "UTF-8";

        [ContextProperty("КодировкаИсточника", "InputEncoding")]
        public string InputEncoding => XMLEncoding;

        [ContextProperty("Автономный", "Standalone")]
        public bool Standalone => throw new NotSupportedException();

        #endregion

        #region Свойства текущего узла

        [ContextProperty("URIПространстваИмен", "NamespaceURI")]
        public string NamespaceURI => _reader?.NamespaceURI ?? string.Empty;

        [ContextProperty("БазовыйURI", "BaseURI")]
        public string BaseURI => _reader?.BaseURI ?? string.Empty;

        [ContextProperty("ИмеетЗначение", "HasValue")]
        public bool HasValue => _reader?.HasValue ?? false;

        [ContextProperty("Значение", "Value")]
        public string Value => HasValue ? _reader.Value : string.Empty;


        [ContextProperty("ИмеетИмя", "HasName")]
        public bool HasName => _reader != null ? _reader.LocalName != String.Empty : false;

        [ContextProperty("Имя", "Name")]
        public string Name => _reader?.Name ?? string.Empty;

        [ContextProperty("ИмяНотации", "NotationName")]
        public string NotationName => throw new NotSupportedException();

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
        public IValue NamespaceContext
        {
            get
            {
                if (_reader == null)
                    return ValueFactory.Create();

                return new XmlNamespaceContext(Depth, _txtReader.GetNamespacesInScope(XmlNamespaceScope.All));
            }
        }

        [ContextProperty("ЛокальноеИмя", "LocalName")]
        public string LocalName => _reader?.LocalName ?? string.Empty;


        [ContextProperty("Префикс", "Prefix")]
        public string Prefix => _reader?.Prefix ?? string.Empty;

        [ContextProperty("ПубличныйИдентификатор", "PublicId")]
        public string PublicId => throw new NotSupportedException();

        [ContextProperty("СистемныйИдентификатор", "SystemId")]
        public string SystemId => throw new NotSupportedException();

        [ContextProperty("ТипУзла", "NodeType")]
        public IValue NodeType
        {
            get
            {
                XmlNodeType nodeType;
                if (_reader == null)
                {
                    nodeType = XmlNodeType.None;
                }
                else if (_emptyElemReadState == EmptyElemCompabilityState.EmptyElementRead)
                {
                    nodeType = XmlNodeType.EndElement;
                }
                else if (_settings.CDATASectionAsText && _reader.NodeType == XmlNodeType.CDATA)
                {
                    nodeType = XmlNodeType.Text;
                }
                else if (!_settings.UseIgnorableWhitespace && _reader.NodeType == XmlNodeType.Whitespace)
                {
                    nodeType = XmlNodeType.Text;
                }
                else
                {
                    nodeType = _reader.NodeType;
                }

                return GlobalsManager.GetEnum<XmlNodeTypeEnum>().FromNativeValue(nodeType);
            }
        }

        [ContextProperty("ЭтоАтрибутПоУмолчанию", "IsDefaultAttribute")]
        public bool? IsDefaultAttribute
        {
            get
            {
                if (_reader == null || _reader.NodeType != XmlNodeType.Attribute)
                    return null;

                 return _reader.IsDefault;
            }
        }

        [ContextProperty("ЭтоПробельныеСимволы", "IsWhitespace")]
        public bool IsWhitespace
        {
            get
            {
                return _reader != null &&
                    (_reader.NodeType == XmlNodeType.Whitespace ||
                    IsCharacters && string.IsNullOrWhiteSpace(_reader.Value) );
            }
        }

        [ContextProperty("ЭтоСимвольныеДанные", "IsCharacters")]
        public bool IsCharacters
        {
            get
            {
                return _reader != null && 
                    (_reader.NodeType == XmlNodeType.Text || _reader.NodeType == XmlNodeType.CDATA ||
                    _reader.NodeType == XmlNodeType.SignificantWhitespace);
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

            if (_reader == null)
            {
                attributeValue = string.Empty;
            }
            else if (indexOrName.DataType == DataType.Number )
            {
                int index = (int)indexOrName.AsNumber();
                if (index < _reader.AttributeCount)
                    attributeValue = _reader.GetAttribute(index);
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
            if (_reader == null || index + 1 > _reader.AttributeCount)
                return string.Empty;

            _reader.MoveToAttribute(index);
            var name = _reader.Name;
            _reader.MoveToElement();

            return name;
        }
        [ContextMethod("КоличествоАтрибутов", "AttributeCount")]
        public int AttributeCount()
        {
            return _reader?.AttributeCount ?? 0; // несовместимо: 1С возвращает 4294967295 (0xFFFF)
        }

        [ContextMethod("ЛокальноеИмяАтрибута", "AttributeLocalName")]
        public string AttributeLocalName(int index)
        {
            if (_reader == null || index + 1 > _reader.AttributeCount)
                return string.Empty;

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
            return _reader?.MoveToFirstAttribute() ?? false;
        }

        [ContextMethod("ПолучитьАтрибут", "GetAttribute")]
        public IValue GetAttribute(IValue indexOrName, string URIIfGiven = null)
        {
            return AttributeValue(indexOrName, URIIfGiven);
        }

        [ContextMethod("ПрефиксАтрибута", "AttributePrefix")]
        public string AttributePrefix(int index)
        {
            if (_reader == null || index+1 > _reader.AttributeCount)
                return string.Empty;

            _reader.MoveToAttribute(index);
            var name = _reader.Prefix;
            _reader.MoveToElement();

            return name;
        }

        [ContextMethod("Пропустить", "Skip")]
        public void Skip()
        {
            if (_reader == null)
                return;

            if (_emptyElemReadState == EmptyElemCompabilityState.EmptyElementEntered)
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
            if (_reader == null)
                return false;

            if (_emptyElemReadState == EmptyElemCompabilityState.EmptyElementEntered)
            {
                _emptyElemReadState = EmptyElemCompabilityState.EmptyElementRead;
                return true;
            }
            else
            {
                bool readingDone = _ignoreWSChanged ? ReadWhenStateChanged() : _reader.Read();
                CheckEmptyElementEntering();
                return readingDone;
            }
        }

        private bool ReadWhenStateChanged()
        {
            bool readingDone;
            var ln = _txtReader.LineNumber;
            var lp = _txtReader.LinePosition;
            do
            {
                readingDone = _reader.Read();
                if (!readingDone)
                    break;
            }
            while (ln == _txtReader.LineNumber && lp == _txtReader.LinePosition);
            
            return readingDone;
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
            if (_reader == null)
                return false;

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
