using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Language
{
    public static class CompilerFactory<T> where T : IASTBuilder, new()
    {
        public static CompilerEngine Create()
        {
            var builder = new T();
            return new CompilerEngine(builder);
        }
    }
}
