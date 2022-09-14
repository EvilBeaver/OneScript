/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Execution;

namespace OneScript.Contexts
{
    public interface IAttachableContext : IRuntimeContextInstance
    {
        void OnAttach(out IVariable[] variables,
                      out BslMethodInfo[] methods);
    }

    public interface IRunnable : IAttachableContext
    {
        IExecutableModule Module { get; }
    }
}
