/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

namespace OneScriptDocumenter
{
    public static class ConsoleLogger
    {
        public static void Info(string content)
        {
            Console.WriteLine(content);
        }
        
        public static void Warning(string content)
        {
            using var yellow = new ColorContext(ConsoleColor.Yellow);
            Console.Error.WriteLine(content);
        }
        
        public static void Error(string content)
        {
            using var yellow = new ColorContext(ConsoleColor.Red);
            Console.Error.WriteLine(content);
        }
        
        private class ColorContext : IDisposable
        {
            private readonly ConsoleColor _oldColor;
            
            public ColorContext(ConsoleColor newColor)
            {
                _oldColor = Console.ForegroundColor;
                Console.ForegroundColor = newColor;
            }

            public void Dispose()
            {
                Console.ForegroundColor = _oldColor;
            }
        }
    }
}