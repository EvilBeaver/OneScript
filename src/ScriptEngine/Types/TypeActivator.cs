/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using ScriptEngine.Machine;

namespace ScriptEngine.Types
{
    public class TypeActivator
    {
        private readonly Dictionary<TypeDescriptor, TypeFactory> _factories = new Dictionary<TypeDescriptor, TypeFactory>(); 

        public TypeFactory GetFactoryFor(TypeDescriptor type)
        {
            if (!_factories.TryGetValue(type, out var factory))
            {
                factory = new TypeFactory(type.ImplementingClass);
                _factories[type] = factory;
            }

            return factory;
        }
    }
}