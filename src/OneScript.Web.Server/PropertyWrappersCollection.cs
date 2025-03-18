/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;

namespace OneScript.Web.Server;

/// <summary>
/// Класс, обеспечивающий создание оберток свойств запроса один только раз.
/// </summary>
public class PropertyWrappersCollection
{
    private readonly Dictionary<string, object> _objects = new();
    
    public T Get<T>(string propName, Func<T> factory)
    {
        if (_objects.TryGetValue(propName, out var value))
            return (T)value;
        
        value = factory();
        _objects.Add(propName, value);

        return (T)value;
    }
}