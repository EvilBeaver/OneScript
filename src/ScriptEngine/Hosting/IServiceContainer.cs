/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ScriptEngine.Hosting
{
    public interface IServiceContainer : IDisposable
    {
        object Resolve(Type type);

        T Resolve<T>();
        
        object TryResolve(Type type);
        
        T TryResolve<T>();

        IEnumerable<T> ResolverEnumerable<T>();

        IServiceContainer CreateScope();
    }
}