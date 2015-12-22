/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine
{
    public static class SystemLogger
    {
        static SystemLogger()
        {
            SetWriter(new NullWriter());
        }

        public static void SetWriter(ISystemLogWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException();

            _writer = writer;
        }

        public static void Write(string text)
        {
            _writer.Write(text);
        }

        private class NullWriter : ISystemLogWriter
        {
            public void Write(string text)
            {
            }
        }

        private static ISystemLogWriter _writer;
    }

    public interface ISystemLogWriter
    {
        void Write(string text);
    }
}
