using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime.Compiler
{
    public class ModuleMethodDefinition
    {
        public string Name { get; set; }

        public bool IsFunction { get; set; }

        public bool IsExported { get; set; }

        public ModuleMethodParameter[] Parameters { get; set; }

        public int EntryPoint { get; set; }

        ModuleMethodParameter ParameterFromASTNode(ASTMethodParameter source)
        {
            var param = new ModuleMethodParameter()
            {
                Identifier = source.Name,
                IsByValue = source.ByValue,
                IsOptional = source.IsOptional
            };

            return param;

        }
    }

    public struct ModuleMethodParameter
    {
        public string Identifier;
        public bool IsByValue;
        public bool IsOptional;
        public int DefaultValueIndex;
    }
}
