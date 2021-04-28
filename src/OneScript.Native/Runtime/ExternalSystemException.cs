/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language;

namespace OneScript.Native.Runtime
{
    public class ExternalSystemException : ScriptException
    {
        public ExternalSystemException(Exception reason)
            : base(new ErrorPositionInfo(), $"Внешнее исключение ({reason.GetType().FullName}): {reason.Message}", reason)
        {
        }
    }
}