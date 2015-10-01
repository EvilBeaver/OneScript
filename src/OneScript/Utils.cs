using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    public static class Utils
    {
        public static bool IsValidIdentifier(string name)
        {
            return Language.Utils.IsValidIdentifier(name);
        }
    }
}
