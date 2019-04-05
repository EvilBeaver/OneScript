/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.HostedScript;
using ScriptEngine.HostedScript.Library.Binary;

namespace StandaloneRunner
{
    public class InternalTemplate : ITemplate
    {
        public InternalTemplate(byte[] data, TemplateKind kind)
        {
            Kind = kind;
        }

        public void Dispose()
        {
            //throw new System.NotImplementedException();
        }

        public string GetFilename()
        {
            throw new System.NotImplementedException();
        }

        public BinaryDataContext GetBinaryData()
        {
            throw new System.NotImplementedException();
        }

        public TemplateKind Kind { get; }
    }
}