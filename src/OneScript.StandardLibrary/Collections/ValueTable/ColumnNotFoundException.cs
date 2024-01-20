/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using OneScript.Exceptions;
using OneScript.Localization;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    public class ColumnNotFoundException : RuntimeException
    {
        public ColumnNotFoundException(BilingualString message, Exception innerException) : base(message,
            innerException)
        {
        }

        public ColumnNotFoundException(BilingualString message) : base(message)
        {
        }

        public ColumnNotFoundException(string columnName) : base($"Неверное имя колонки {columnName}")
        {
        }
        
    }
}