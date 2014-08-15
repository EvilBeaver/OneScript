using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("ЗаписьТекста", "TextWriter")]
    class TextWriteImpl : AutoContext<TextWriteImpl>, IDisposable
    {
        StreamWriter _writer;

        public TextWriteImpl()
        {

        }

        public TextWriteImpl(string path, string encoding)
        {
            Open(path, encoding);
        }

        public TextWriteImpl(string path, string encoding, bool append)
        {
            Open(path, encoding, append);
        }

        [ContextMethod("Открыть", "Open")]
        public void Open(string path, string encoding = null, bool append = false)
        {
            Encoding enc;
            if (encoding == null)
            {
                enc = new UTF8Encoding(true);
            }
            else
            {
                enc = Encoding.GetEncoding(encoding);
            }

            _writer = new StreamWriter(path, append, enc);
        }

        [ContextMethod("Закрыть","Close")]
        public void Close()
        {
            Dispose();
        }

        [ContextMethod("Записать", "Write")]
        public void Write(string what)
        {
            _writer.Write(what);
        }

        [ContextMethod("ЗаписатьСтроку", "WriteLine")]
        public void WriteLine(string what)
        {
            _writer.WriteLine(what);
        }

        public void Dispose()
        {
            if (_writer != null)
            {
                _writer.Dispose();
                _writer = null;
            }
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue path, IValue encoding)
        {
            return new TextWriteImpl(path.AsString(), encoding.AsString());
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue path, IValue encoding, IValue append)
        {
            return new TextWriteImpl(path.AsString(), encoding.AsString(), append.AsBoolean());
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor(IValue path)
        {
            var obj = new TextWriteImpl();
            obj.Open(path.AsString());
            return obj;
        }

        [ScriptConstructor]
        public static IRuntimeContextInstance Constructor()
        {
            return new TextWriteImpl();
        }

    }
}
