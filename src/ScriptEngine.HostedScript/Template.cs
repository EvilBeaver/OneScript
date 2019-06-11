/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Net;

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript
{
    internal class Template : ITemplate
    {
        public Template(string file, TemplateKind kind)
        {
            Kind = kind;
            Filename = file;
        }

        public TemplateKind Kind { get; }

        private string Filename { get; }

        public string GetFilename()
        {
            return Filename;
        }

        public BinaryDataContext GetBinaryData()
        {
            return new BinaryDataContext(Filename);
        }

        public void Dispose()
        {
            
        }
    }

    
}