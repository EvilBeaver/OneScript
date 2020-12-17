/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.Zip
{
    [EnumerationType("МетодСжатияZIP", "ZIPCompressionMethod")]
    public enum ZipCompressionMethod
    {
        [EnumItem("Сжатие", "Deflate")]
        Deflate,
        [EnumItem("Копирование", "Copy")]
        Copy
    }
}
