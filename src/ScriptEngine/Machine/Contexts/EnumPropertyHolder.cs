/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;

namespace ScriptEngine.Machine.Contexts
{
    public class EnumPropertyHolder /*: IAttachableContext*/
    {
        private readonly List<IValue> _values = new List<IValue>();
        private readonly List<BslPropertyInfo> _definitions = new List<BslPropertyInfo>();
    }
}