using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Machine.Library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace oscript
{
    [ContextClass("ВебЗапрос","WebRequest")]
    public class WebRequestContext : AutoContext<WebRequestContext>
    {
        MapImpl _environmentVars = new MapImpl();
        MapImpl _get = new MapImpl();
        MapImpl _post = new MapImpl();

        public WebRequestContext()
        {
            string get = Environment.GetEnvironmentVariable("REQUEST_STRING");
            if(get != null)
            {
                FillGetMap(get);
            }
        }

        private void FillGetMap(string get)
        {
            var pairs = get.Split('&');
            foreach (var pair in pairs)
            {
                var nameVal = pair.Split(new Char[]{'='}, 2);
                if (nameVal.Length == 2)
                {
                    IValue key = ValueFactory.Create(nameVal[0]);
                    IValue val = ValueFactory.Create(Uri.UnescapeDataString(nameVal[1]));
                    _get.Insert(key, val);
                }
                else
                {
                    IValue val = ValueFactory.Create(Uri.UnescapeDataString(nameVal[0]));
                    _get.Insert(val, ValueFactory.Create());
                }
            }
        }

        [ContextProperty("GET")]
        public IValue GET
        {
            get
            {
                return _get;
            }
        }
    }
}
