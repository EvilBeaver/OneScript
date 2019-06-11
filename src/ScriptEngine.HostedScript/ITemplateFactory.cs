/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

using ScriptEngine.HostedScript.Library.Binary;

namespace ScriptEngine.HostedScript
{
    public interface ITemplateFactory
    {
        ITemplate CreateTemplate(string file, TemplateKind kind);
    }
    
    public interface ITemplate : IDisposable
    {
        string GetFilename();

        BinaryDataContext GetBinaryData();

        TemplateKind Kind { get; }
    }
}