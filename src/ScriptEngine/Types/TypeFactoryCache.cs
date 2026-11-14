/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Concurrent;
using OneScript.Types;
using ScriptEngine.Machine;

namespace ScriptEngine.Types
{
    public class TypeFactoryCache
    {
        // Объекты создаются из всех потоков, где исполняется код, в том числе из фоновых заданий
        private readonly ConcurrentDictionary<TypeDescriptor, TypeFactory> _factories = new ConcurrentDictionary<TypeDescriptor, TypeFactory>();

        public TypeFactory GetFactoryFor(TypeDescriptor type)
        {
            return _factories.GetOrAdd(type, t => new TypeFactory(t));
        }
    }
}