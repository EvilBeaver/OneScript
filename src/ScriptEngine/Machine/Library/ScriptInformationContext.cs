using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Library
{
    [ContextClass("ИнформацияОСценарии")]
    class ScriptInformationContext : ContextBase<ScriptInformationContext>
    {
        private ICodeSource _info;

        public ScriptInformationContext(ICodeSource info)
        {
            _info = info;
        }

        [ContextProperty("Источник")]
        public string Source
        {
            get
            {
                return _info.SourceDescription;
            }
        }
    }
}
