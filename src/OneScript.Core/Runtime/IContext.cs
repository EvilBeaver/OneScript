/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Values;

namespace OneScript.Runtime
{
    /// <summary>
    /// Протокол динамического общения с классами Bsl
    /// Замена для IRuntimeContextInstance
    /// </summary>
    public interface IContext
    {
        IReadOnlyList<BslPropertyInfo> Properties { get; }
        
        IReadOnlyList<BslMethodInfo> Methods { get; }
        
        bool DynamicMethodSignatures { get; }

        void SetPropValue(int index, BslValue value);
        
        BslValue GetPropValue(int index);
        
        void CallAsProcedure(int index, IReadOnlyList<BslValue> arguments);
        
        BslValue CallAsFunction(int methodNumber, IReadOnlyList<BslValue> arguments);
    }
}