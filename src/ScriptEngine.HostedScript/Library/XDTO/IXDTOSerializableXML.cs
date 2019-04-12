/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.HostedScript.Library.Xml;
using ScriptEngine.Machine;

namespace ScriptEngine.HostedScript.Library.XDTO
{
    public interface IXDTOSerializable
    {
        void Get(XmlReaderImpl xmlReader);
        void WriteXML(XmlWriterImpl xmlWriter, IRuntimeContextInstance value);
    }
}
