/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

namespace OneScript.Commons
{
    public interface IObjectWrapper
    {
        object UnderlyingObject { get; }
    }
    
    public static class WrapperHelper 
    {
        public static T GetUnderlying<T>(this IObjectWrapper wrapper)
        {
            return (T) wrapper.UnderlyingObject;
        }
    }
}
