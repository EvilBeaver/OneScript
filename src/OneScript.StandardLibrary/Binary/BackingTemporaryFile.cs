/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.IO;
using Microsoft.Win32.SafeHandles;
using ScriptEngine;

namespace OneScript.StandardLibrary.Binary
{
    /// <summary>
    /// Обертка над временным файлом, хранящим двоичные данные
    /// </summary>
    internal sealed class BackingTemporaryFile : IDisposable
    {
        // Открытый хэндл, удерживающий файл от удаления.
        private SafeFileHandle _holder;
        
        private long _length;
        
        // Стримы чтения не используют хэндл, а открываются по пути,
        // т.к. при закрытии стрим закроет хэндл, кроме того, у хэндла есть позиция
        // и стрим будет начат с этой позиции.
        private string _backingFilePath;

        public BackingTemporaryFile(string path)
        {
            using var source = new FileStream(path, FileMode.Open, FileAccess.Read);

            Init(source);
        }

        public BackingTemporaryFile(Stream source)
        {
            if (!source.CanSeek)
                throw new ArgumentException("Stream must support seek");
            
            var pos = source.Position; 
            Init(source);
            source.Position = pos;
        }

        public void Dispose()
        {
            // Благодаря DeleteOnClose, если других дескрипторов не осталось,
            // файл будет удален силами ОС.
            _holder?.Dispose();
            _holder = null;
            _backingFilePath = null;
        }

        public long Size() => _length;

        public Stream OpenReadStream()
        {
            return new FileStream(_backingFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
        
        private void Init(Stream source)
        {
            _length = source.Length;
            _backingFilePath = Path.GetTempFileName();

            using (var fs = new FileStream(_backingFilePath, FileMode.Create))
            {
                source.CopyTo(fs);
            }

            _holder = File.OpenHandle(_backingFilePath, options: FileOptions.DeleteOnClose | FileOptions.SequentialScan);
        }
    }
}