/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library.Binary
{
    [ContextClass("ДвоичныеДанные", "BinaryData")]
    public class BinaryDataContext : AutoContext<BinaryDataContext>, IDisposable
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

        public BinaryDataContext(byte[] buffer)
        {
            _buffer = buffer;
        }

        public void Dispose()
        {
            _buffer = null;
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

        public byte[] Buffer
        {
            get
            {
                return _buffer;
            }
        }

        /// <summary>
        /// 
        /// Открывает поток для чтения двоичных данных.
        /// </summary>
        ///
        ///
        /// <returns name="Stream">
        /// Представляет собой поток данных, который можно последовательно читать и/или в который можно последовательно писать. 
        /// Экземпляры объектов данного типа можно получить с помощью различных методов других объектов.</returns>
        ///
        [ContextMethod("ОткрытьПотокДляЧтения", "OpenStreamForRead")]
        public GenericStream OpenStreamForRead()
        {
            var stream = new MemoryStream(_buffer);
            return new GenericStream(stream);
        }

        [ScriptConstructor(Name="На основании файла")]
        public static BinaryDataContext Constructor(IValue filename)
        {
            return new BinaryDataContext(filename.AsString());
        }

    }
}
