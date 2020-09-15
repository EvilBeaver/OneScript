/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;

namespace ScriptEngine
{
    public static class Utils
    {
        public static bool IsValidIdentifier(string name)
        {
            if (name == null || name.Length == 0)
                return false;

            if (!(Char.IsLetter(name[0]) || name[0] == '_'))
                return false;

            for (int i = 1; i < name.Length; i++)
            {
                if (!(Char.IsLetterOrDigit(name[i]) || name[i] == '_'))
                    return false;
            }

            return true;
        }

        public static void ForEach<T>(this IEnumerable<T> input, Action<T> action)
        {
            foreach (var data in input)
            {
                action(data);
            }
        }
        
        public static bool IsNetCore => System.Runtime.InteropServices
            .RuntimeInformation
            .FrameworkDescription
            .StartsWith(".NET Core", StringComparison.OrdinalIgnoreCase);
        
        public static bool IsNetFramework => System.Runtime.InteropServices
            .RuntimeInformation
            .FrameworkDescription
            .StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);

        public static bool IsMonoRuntime => Type.GetType("Mono.Runtime") != null;

    }
}
