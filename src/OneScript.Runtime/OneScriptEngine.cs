using OneScript.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript.Runtime
{
    public class OneScriptEngine : IScriptEngine
    {
        public OneScriptEngine()
        {
            TypeManager = new TypeManager();
        }

        public TypeManager TypeManager
        {
            get;
            internal set;
        }
    }
}
