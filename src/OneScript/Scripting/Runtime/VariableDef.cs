using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Runtime
{
    public struct VariableDef
    {
        public string Name { get; set; }
        public VariableFlags Flags { get; set; }

        public bool IsExported
        {
            get { return Flags.HasFlag(VariableFlags.Exported); }
        }

        public bool IsLocal
        {
            get { return Flags.HasFlag(VariableFlags.Local); }
        }

        public static VariableDef CreateGlobal(string name)
        {
            return CreateInternal(name, false);
        }

        public static VariableDef CreateExported(string name)
        {
            return CreateInternal(name, true);
        }

        public static VariableDef CreateLocal(string name)
        {
            return new VariableDef()
            {
                Name = name
            };
        }

        private static VariableDef CreateInternal(string name, bool isExported)
        {
            var v = new VariableDef();
            v.Name = name;
            if (isExported)
                v.Flags |= VariableFlags.Exported;
            else
                v.Flags |= VariableFlags.ModuleLevel;

            return v;
        }

    }

    [Flags]
    public enum VariableFlags
    {
        Local = 0,
        ModuleLevel = 1,
        Exported = 2
    }
}
