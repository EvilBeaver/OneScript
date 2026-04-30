/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace ScriptEngine
{
    /// <summary>
    /// Лимит объёма данных в памяти для объектов «ДвоичныеДанные» и смежных потоков
    /// до выгрузки во временный файл (байты).
    /// </summary>
    public interface IBinaryDataMemoryLimit
    {
        int MaxBytesInMemory { get; }
    }

    internal sealed class BinaryDataMemoryLimitFromOptions : IBinaryDataMemoryLimit
    {
        public BinaryDataMemoryLimitFromOptions(OneScriptCoreOptions options)
        {
            MaxBytesInMemory = options.BinaryDataInMemoryMaxBytes;
        }

        public int MaxBytesInMemory { get; }
    }
}
