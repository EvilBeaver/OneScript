/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Language;

namespace ScriptEngine.Machine
{
    public class ExternalSystemException : ScriptException
    {
        public ExternalSystemException(Exception reason)
            : base(new ErrorPositionInfo(), string.Format("Внешнее исключение ({0}): {1}", reason.GetType().FullName, reason.Message), reason)
        {
        }
    }
}
