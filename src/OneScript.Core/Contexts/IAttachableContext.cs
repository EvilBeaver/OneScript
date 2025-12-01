/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Execution;

namespace OneScript.Contexts
{
    /// <summary>
    /// Присоединяемый контекст. Методы и свойства этого контекста становятся глобальными методами и свойствами.
    /// Каждый скрипт также является присоединяемым, за счет чего его методы и свойства доступны в модуле, как глобальные.
    /// </summary>
    public interface IAttachableContext : IRuntimeContextInstance
    {
        [Obsolete]
        void OnAttach(out IVariable[] variables,
                      out BslMethodInfo[] methods);

        IVariable GetVariable(int index);
        BslMethodInfo GetMethod(int index);
        
        int VariablesCount { get; }
        int MethodsCount { get; }
    }

    /// <summary>
    /// Выполняемое. Имеет bsl-модуль и может присоединяться к машине
    /// </summary>
    public interface IRunnable : IAttachableContext
    {
        IExecutableModule Module { get; }
    }
}
