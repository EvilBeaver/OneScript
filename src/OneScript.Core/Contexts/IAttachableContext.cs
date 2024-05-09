/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Execution;

namespace OneScript.Contexts
{
    /// <summary>
    /// Присоединяемый контекст. Методы и свойства этого контекста становятся глобальными методами и свойствами.
    /// Каждый модуль также является присоединяемым, за счет чего его методы и свойства доступны в модуле, как глобальные.
    /// </summary>
    public interface IAttachableContext : IRuntimeContextInstance
    {
        void OnAttach(out IVariable[] variables,
                      out BslMethodInfo[] methods);
    }

    /// <summary>
    /// Выполняемое. Имеет bsl-модуль и может присоединяться к машине
    /// </summary>
    public interface IRunnable : IAttachableContext
    {
        IExecutableModule Module { get; }
    }
}
