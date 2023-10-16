/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Language;
using OneScript.Localization;

namespace OneScript.Exceptions
{
    public class BslCoreException : ScriptException
    {
        public BslCoreException(BilingualString message) : base(message.ToString())
        {
        }
        
        public BslCoreException(BilingualString message, Exception innerException) : base(message.ToString(), innerException)
        {
        }
    }
}