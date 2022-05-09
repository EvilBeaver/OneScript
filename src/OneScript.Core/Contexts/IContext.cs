/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Values;

namespace OneScript.Contexts
{
    /// <summary>
    /// Протокол динамического общения с классами Bsl
    /// Замена для IRuntimeContextInstance
    /// </summary>
    public interface IContext
    {
        IReadOnlyList<BslPropertyInfo> GetProperties();

        IReadOnlyList<BslMethodInfo> GetMethods();

        BslPropertyInfo FindProperty(string name);
        
        BslMethodInfo FindMethod(string name);
        
        bool DynamicMethodSignatures { get; }

        void SetPropValue(BslPropertyInfo property, BslValue value);
        
        BslValue GetPropValue(BslPropertyInfo property);
        
        void CallAsProcedure(BslMethodInfo method, IReadOnlyList<BslValue> arguments);
        
        BslValue CallAsFunction(BslMethodInfo method, IReadOnlyList<BslValue> arguments);
    }
}