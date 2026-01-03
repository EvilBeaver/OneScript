/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using MessagePack;

namespace ScriptEngine.Compiler.Packaged
{
    /// <summary>
    /// DTO для сериализации поля модуля (переменной)
    /// </summary>
    [MessagePackObject]
    public class FieldDto
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public bool IsExport { get; set; }

        [Key(2)]
        public int DispatchId { get; set; }

        [Key(3)]
        public List<AnnotationDto> Annotations { get; set; }
    }
}
