/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;

namespace ScriptEngine.Libraries
{
    /// <summary>
    /// Временный класс для откусывания ответственностей от RuntimeEnvironment
    /// </summary>
    public interface ILibraryManager
    {
        IEnumerable<ExternalLibraryDef> GetLibraries();
        void InitExternalLibrary(ScriptingEngine runtime, ExternalLibraryDef library);
    }
}