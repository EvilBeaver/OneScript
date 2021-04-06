/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Types;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.StandardLibrary.Collections.ValueTable
{
    [ContextClass("ИндексКоллекции", "CollectionIndex", TypeUUID = "48D150D4-A0DA-47CA-AEA3-D4078A731C11")]
    public class CollectionIndex : ContextIValueImpl
    {
        private static readonly TypeDescriptor _objectType = typeof(ValueTableColumnCollection).GetTypeFromClassMarkup();
        
        public CollectionIndex() : base(_objectType)
        {
        }
        
    }
}
