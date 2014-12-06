using ScriptEngine.Machine.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.HostedScript.Library
{
    [GlobalContext(Category="Прочие функции")]
    public class MiscGlobalFunctions : GlobalContextBase<MiscGlobalFunctions>
    {
        [ContextMethod("Base64Строка", "Base64String")]
        public string Base64String(BinaryDataContext data)
        {
            return Convert.ToBase64String(data.Buffer);
        }

        [ContextMethod("Base64Значение", "Base64Value")]
        public BinaryDataContext Base64String(string data)
        {
            byte[] bytes = Convert.FromBase64String(data);
            return new BinaryDataContext(bytes);
        }

        public static MiscGlobalFunctions CreateInstance()
        {
            return new MiscGlobalFunctions();
        }
    }
}
