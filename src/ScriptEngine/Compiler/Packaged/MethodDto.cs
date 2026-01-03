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
    /// DTO для сериализации метода
    /// </summary>
    [MessagePackObject]
    public class MethodDto
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public string Alias { get; set; }

        [Key(2)]
        public bool IsFunction { get; set; }

        [Key(3)]
        public bool IsExport { get; set; }

        [Key(4)]
        public bool IsAsync { get; set; }

        [Key(5)]
        public bool IsDeprecated { get; set; }

        [Key(6)]
        public bool ThrowOnUseDeprecated { get; set; }

        [Key(7)]
        public int EntryPoint { get; set; }

        [Key(8)]
        public List<string> LocalVariables { get; set; } = new List<string>();

        [Key(9)]
        public List<ParameterDto> Parameters { get; set; } = new List<ParameterDto>();

        [Key(10)]
        public List<AnnotationDto> Annotations { get; set; } = new List<AnnotationDto>();
    }

    /// <summary>
    /// DTO для сериализации параметра метода
    /// </summary>
    [MessagePackObject]
    public class ParameterDto
    {
        [Key(0)]
        public string Name { get; set; }

        [Key(1)]
        public bool IsByValue { get; set; }

        [Key(2)]
        public bool HasDefaultValue { get; set; }

        [Key(3)]
        public int DefaultValueIndex { get; set; }

        [Key(4)]
        public List<AnnotationDto> Annotations { get; set; }
    }
}
