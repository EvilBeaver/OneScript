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
        private readonly int _inMemLimit;

        public FileSourceTemplate(int inMemLimit, string file, TemplateKind kind)
        {
            _inMemLimit = inMemLimit;
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
            return new BinaryDataContext(Filename, _inMemLimit);
        }

        public void Dispose()
        {
            
        }
    }

    
}