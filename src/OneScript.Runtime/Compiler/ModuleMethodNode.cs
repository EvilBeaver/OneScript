using OneScript.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime.Compiler
{
    class ModuleMethodNode : IASTMethodDefinitionNode
    {
        private ModuleMethodDefinition _method = new ModuleMethodDefinition();

        public ModuleMethodNode()
        {
            Parameters = new ASTMethodParameter[0];
        }

        public string Identifier
        {
            get { return _method.Name; }
            set { _method.Name = value; }
        }

        public bool IsFunction
        {
            get { return _method.IsFunction; }
            set { _method.IsFunction = value; }
        }

        public bool IsExported
        {
            get { return _method.IsExported; }
            set { _method.IsExported = value; }
        }

        public ASTMethodParameter[] Parameters
        {
            get;
            set;
        }

        public int EntryPoint { get; set; }

        public ModuleMethodDefinition GetMethodForModule(CompiledModule module)
        {
            var parameters = new ModuleMethodParameter[Parameters.Length];
            for (int i = 0; i < Parameters.Length; i++)
            {
                var src = Parameters[i];
                parameters[i] = new ModuleMethodParameter()
                {
                    Identifier = src.Name,
                    IsByValue = src.ByValue,
                    IsOptional = src.IsOptional,
                    DefaultValueIndex = src.IsOptional ? module.Constants.GetIndex(src.DefaultValueLiteral) : CompiledModule.InvalidEntityIndex
                };
            }

            _method.Parameters = parameters;
            _method.EntryPoint = EntryPoint;
            return _method;
        }

    }

    
}
