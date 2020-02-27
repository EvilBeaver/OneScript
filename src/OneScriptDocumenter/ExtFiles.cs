/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.IO;
using System.Reflection;

namespace OneScriptDocumenter
{
    static class ExtFiles
    {
        static string _root;

        private static string Root
        {
            get
            {
                if (_root == null)
                {
                    string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                    UriBuilder uri = new UriBuilder(codeBase);
                    string path = Uri.UnescapeDataString(uri.Path);
                    _root = Path.GetDirectoryName(path);
                }

                return _root;
            }
        }

        public static string Get(string name)
        {
            return Path.Combine(Root, name);
        }
    }
}
