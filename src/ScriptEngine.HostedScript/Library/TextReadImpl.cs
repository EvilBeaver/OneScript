using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("ЧтениеТекста")]
    class TextReadImpl : AutoContext<TextReadImpl>, IDisposable
    {
        StreamReader _reader;

        [ContextMethod("Открыть")]
        public void Open(string path, string encoding = null)
        {
            if (encoding == null)
            {
                _reader = Environment.FileOpener.OpenReader(path);
            }
            else
            {
                var enc = Encoding.GetEncoding(encoding);
                _reader = Environment.FileOpener.OpenReader(path, enc);
            }
        }

        [ContextMethod("Прочитать")]
        public IValue ReadAll()
        {
            RequireOpen();
            if (_reader.EndOfStream)
                return ValueFactory.Create();

            return ValueFactory.Create(_reader.ReadToEnd());
        }

        [ContextMethod("ПрочитатьСтроку")]
        public IValue ReadLine()
        {
            RequireOpen();
            if (_reader.EndOfStream)
                return ValueFactory.Create();

            return ValueFactory.Create(_reader.ReadLine());
        }

        [ContextMethod("Закрыть")]
        public void Close()
        {
            Dispose();
        }

        private void RequireOpen()
        {
            if (_reader == null)
            {
                throw new RuntimeException("File is not opened");
            }
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue path, IValue encoding)
        {
            var reader = new TextReadImpl();
            reader.Open(path.AsString(), encoding.AsString());
            return reader;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue path)
        {
            var reader = new TextReadImpl();
            reader.Open(path.AsString(), null);
            return reader;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            var reader = new TextReadImpl();
            return reader;
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_reader != null)
            {
                _reader.Dispose();
                _reader = null;
            }
        }

        #endregion
    }
}
