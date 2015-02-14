using System;
using System.IO;
using System.Text;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("ЧтениеТекста", "TextReader")]
    class TextReadImpl : AutoContext<TextReadImpl>, IDisposable
    {
        StreamReader _reader;

        [ContextMethod("Открыть", "Open")]
        public void Open(string path, IValue encoding = null)
        {
            if (encoding == null)
            {
                _reader = Environment.FileOpener.OpenReader(path);
            }
            else
            {
                var enc = TextEncodingEnum.GetEncoding(encoding);
                _reader = Environment.FileOpener.OpenReader(path, enc);
            }
        }

        [ContextMethod("Прочитать", "Read")]
        public IValue ReadAll()
        {
            RequireOpen();
            if (_reader.EndOfStream)
                return ValueFactory.Create();

            return ValueFactory.Create(_reader.ReadToEnd());
        }

        [ContextMethod("ПрочитатьСтроку", "ReadLine")]
        public IValue ReadLine()
        {
            RequireOpen();
            if (_reader.EndOfStream)
                return ValueFactory.Create();

            return ValueFactory.Create(_reader.ReadLine());
        }

        [ContextMethod("Закрыть", "Close")]
        public void Close()
        {
            Dispose();
        }

        private void RequireOpen()
        {
            if (_reader == null)
            {
                throw new RuntimeException("Файл не открыт");
            }
        }

        [ScriptConstructor(Name="По имени файла и кодировке")]
        public static IRuntimeContextInstance Constructor(IValue path, IValue encoding)
        {
            var reader = new TextReadImpl();
            reader.Open(path.AsString(), encoding);
            return reader;
        }

        [ScriptConstructor(Name = "По имени файла")]
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
