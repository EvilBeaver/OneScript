/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using ScriptEngine.Types;

namespace ScriptEngine.Machine
{
    public interface ITypeManager
    {
        TypeDescriptor GetTypeByName(string name);
        TypeDescriptor GetTypeByFrameworkType(Type type);
        TypeDescriptor RegisterType(string name, string alias, Type implementingClass);
        
        TypeFactory GetFactoryFor(TypeDescriptor type);
        
        bool IsKnownType(Type type);
        bool IsKnownType(string typeName);
    }
}