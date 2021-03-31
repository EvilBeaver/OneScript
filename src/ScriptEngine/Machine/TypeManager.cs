/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using OneScript.Core;
using ScriptEngine.Types;

namespace ScriptEngine.Machine
{
    [Obsolete]
    public static class TypeManager
    {
        private static ITypeManager _instance;

        internal static void Initialize(ITypeManager instance)
        {
            _instance = instance;
        }

        public static ITypeManager Instance => _instance;

        public static TypeDescriptor GetTypeByName(string name)
        {
            return _instance.GetTypeByName(name);
        }

        public static TypeDescriptor RegisterType(string name, string alias, Type implementingClass)
        {
            return _instance.RegisterType(name, alias, implementingClass);
        }

        public static bool IsKnownType(Type type)
        {
            return _instance.IsKnownType(type);
        }

        public static bool IsKnownType(string typeName)
        {
            return _instance.IsKnownType(typeName);
        }

        public static TypeDescriptor GetTypeByFrameworkType(Type type)
        {
            return _instance.GetTypeByFrameworkType(type);
        }
    }

}
