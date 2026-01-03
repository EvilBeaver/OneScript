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
    /// DTO для сериализации аннотации
    /// </summary>
    [MessagePackObject]
    public class AnnotationDto
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public List<AnnotationParameterDto> Parameters { get; set; }
    }

    /// <summary>
    /// DTO для сериализации параметра аннотации
    /// </summary>
    [MessagePackObject]
    public class AnnotationParameterDto
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public string Value { get; set; }
    }
}
