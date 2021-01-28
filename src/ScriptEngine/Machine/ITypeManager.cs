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

        bool TryGetType(string name, out TypeDescriptor type);
        bool TryGetType(Type frameworkType, out TypeDescriptor type);
        
        TypeDescriptor RegisterType(string name, string alias, Type implementingClass);
        
        void RegisterType(TypeDescriptor typeDescriptor);
        
        TypeFactory GetFactoryFor(TypeDescriptor type);
        
        bool IsKnownType(Type type);
        bool IsKnownType(string typeName);
    }
}