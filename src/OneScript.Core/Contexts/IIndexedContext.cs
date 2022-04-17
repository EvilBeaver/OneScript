/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Values;

namespace OneScript.Contexts
{   
    public interface IIndexedContext<in TIndex, TValue>
    {
        TValue GetIndexedValue(TIndex index);
        
        void SetIndexedValue(TIndex index, TValue value);
    }
    
    public interface IIndexedContext : IIndexedContext<BslValue, BslValue>
    {
    }
}