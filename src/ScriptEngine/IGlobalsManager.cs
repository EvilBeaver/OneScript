/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace ScriptEngine
{
    public interface IGlobalsManager : IEnumerable<KeyValuePair<Type, object>>, IDisposable
    {
        void RegisterInstance(object instance);
        void RegisterInstance(Type type, object instance);
        object GetInstance(Type type);
        T GetInstance<T>();
    }
}