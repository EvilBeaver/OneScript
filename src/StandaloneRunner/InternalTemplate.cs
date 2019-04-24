/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.IO;
using System.Net;

using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library.Binary;

namespace StandaloneRunner
{
    public class InternalTemplate : ITemplate
    {
        private string _tempFileName;

        private BinaryDataContext _data;
        
        public InternalTemplate(byte[] data, TemplateKind kind)
        {
            Kind = kind;
            _data = new BinaryDataContext(data);
        }

        public void Dispose()
        {
            if (_tempFileName != null)
            {
                try
                {
                    File.Delete(_tempFileName);
                }
                catch
                {// now errors on exit
                }
            }
        }

        public string GetFilename()
        {
            if (_tempFileName == null)
            {
                _tempFileName = Path.GetTempFileName();
                using (var fs = new FileStream(_tempFileName, FileMode.OpenOrCreate))
                {
                    fs.Write(_data.Buffer,0,_data.Buffer.Length);
                }
            }

            return _tempFileName;
        }

        public BinaryDataContext GetBinaryData()
        {
            return _data;
        }

        public TemplateKind Kind { get; }
    }
}