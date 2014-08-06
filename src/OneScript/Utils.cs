using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OneScript
{
    static class Utils
    {
        public static bool IsValidIdentifier(string name)
        {
            if (name == null || name.Length == 0)
                return false;

            if (!(Char.IsLetter(name[0]) || name[0] == '_'))
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!Char.IsLetterOrDigit(name[i]))
                    return false;
            }

            return true;
        }
    }
}
