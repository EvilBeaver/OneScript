using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Scripting.Compiler
{
    public static class CompilerFactory<T> where T : IModuleBuilder, new()
    {
        public static Compiler Create()
        {
            var builder = new T();
            return new Compiler(builder);
        }
    }
}
