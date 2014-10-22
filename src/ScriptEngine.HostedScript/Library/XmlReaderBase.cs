using ScriptEngine.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

namespace ScriptEngine.HostedScript.Library
{
    public class XmlReaderBase : IDisposable
    {
        XmlReader _reader;

        public void OpenFile(string path)
        {
            if (_reader != null)
                throw new RuntimeException("Поток XML уже открыт");
            var textInput = new StreamReader(path);
            _reader = XmlReader.Create(textInput);
        }

        public void SetString(string content)
        {
            if (_reader != null)
                throw new RuntimeException("Поток XML уже открыт");

            var textInput = new StringReader(content);
            _reader = XmlReader.Create(textInput);
        }

        private void CheckIfOpen()
        {
            if (_reader == null)
                throw new RuntimeException("Файл не открыт");
        }

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Close();
                _reader = null;
            }
        }
    }
}
