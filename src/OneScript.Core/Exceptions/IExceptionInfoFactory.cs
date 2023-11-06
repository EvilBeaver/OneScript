/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Values;

namespace OneScript.Exceptions
{
    /// <summary>
    /// Фабрика объекта ИнформацияОбОшибке по исключению.
    /// </summary>
    public interface IExceptionInfoFactory
    {
        BslObjectValue GetExceptionInfo(Exception exception);
        
        string GetExceptionDescription(IRuntimeContextInstance exceptionInfo);

        Exception Raise(object raiseValue);
    }
}