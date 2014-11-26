using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [ContextClass("ДвоичныеДанные", "BinaryData")]
    public class BinaryDataContext : AutoContext<BinaryDataContext>
    {
        byte[] _buffer;

        public BinaryDataContext(string filename)
        {
            using(var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                _buffer = new byte[fs.Length];
                fs.Read(_buffer, 0, _buffer.Length);
            }
        }
        
        [ContextMethod("Размер","Size")]
        public int Size()
        {
            return _buffer.Length;
        }

        [ContextMethod("Записать","Write")]
        public void Write(string filename)
        {
            using(var fs = new FileStream(filename, FileMode.Create, FileAccess.Write))
            {
                fs.Write(_buffer, 0, _buffer.Length);
            }
        }

        [ScriptConstructor(Name="На основании файла")]
        public static BinaryDataContext Constructor(IValue filename)
        {
            return new BinaryDataContext(filename.AsString());
        }

    }
}
