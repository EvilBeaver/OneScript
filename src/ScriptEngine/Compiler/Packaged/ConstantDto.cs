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
    /// DTO для сериализации константы
    /// </summary>
    [MessagePackObject]
    public class ConstantDto
    {
        [Key(0)]
        public ConstantType Type { get; set; }

        [Key(1)]
        public string StringValue { get; set; }

        [Key(2)]
        public decimal? NumberValue { get; set; }

        [Key(3)]
        public long? DateTicks { get; set; }

        [Key(4)]
        public bool? BoolValue { get; set; }
    }

    public enum ConstantType : byte
    {
        Undefined = 0,
        Null = 1,
        String = 2,
        Number = 3,
        Boolean = 4,
        Date = 5
    }
}
