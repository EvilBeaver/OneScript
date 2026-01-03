/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using MessagePack;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// DTO для сериализации команды байт-кода
    /// </summary>
    [MessagePackObject]
    public class CommandDto
    {
        [Key(0)]
        public int Code { get; set; }

        [Key(1)]
        public int Argument { get; set; }
    }
}
