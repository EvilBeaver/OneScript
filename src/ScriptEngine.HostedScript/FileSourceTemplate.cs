/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.StandardLibrary.Binary;

namespace ScriptEngine.HostedScript
{
    public class FileSourceTemplate : ITemplate
    {
        public FileSourceTemplate(string file, TemplateKind kind)
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