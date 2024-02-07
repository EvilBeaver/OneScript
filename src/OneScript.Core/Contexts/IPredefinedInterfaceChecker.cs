/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Execution;

namespace OneScript.Contexts
{
    /// <summary>
    /// Постобработчик построения модуля, регистрирующий специальные аннотации, позволяющий во время компиляции проверить
    /// что модуль соответствует тому или иному интерфейсу, содержит корректные методы и пр.
    /// </summary>
    public interface IPredefinedInterfaceChecker
    {
        IEnumerable<PredefinedInterfaceRegistration> GetRegistrations();

        public void Validate(IExecutableModule module);
    }
}