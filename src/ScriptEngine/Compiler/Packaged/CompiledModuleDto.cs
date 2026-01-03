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
    /// DTO для сериализации скомпилированного модуля
    /// </summary>
    [MessagePackObject]
    public class CompiledModuleDto
    {
        public const int FormatVersion = 1;
        public const string Magic = "OSC1";

        [Key(0)]
        public string MagicHeader { get; set; } = Magic;

        [Key(1)]
        public int Version { get; set; } = FormatVersion;

        [Key(2)]
        public List<ConstantDto> Constants { get; set; } = new List<ConstantDto>();

        [Key(3)]
        public List<string> Identifiers { get; set; } = new List<string>();

        [Key(4)]
        public List<CommandDto> Code { get; set; } = new List<CommandDto>();

        [Key(5)]
        public List<MethodDto> Methods { get; set; } = new List<MethodDto>();

        [Key(6)]
        public List<FieldDto> Fields { get; set; } = new List<FieldDto>();

        [Key(7)]
        public List<SymbolBindingDto> VariableRefs { get; set; } = new List<SymbolBindingDto>();

        [Key(8)]
        public List<SymbolBindingDto> MethodRefs { get; set; } = new List<SymbolBindingDto>();

        [Key(9)]
        public List<AnnotationDto> ModuleAttributes { get; set; } = new List<AnnotationDto>();

        [Key(10)]
        public int EntryMethodIndex { get; set; } = -1;

        [Key(11)]
        public string SourceFileName { get; set; }
    }
}
