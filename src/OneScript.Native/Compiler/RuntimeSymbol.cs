/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Reflection;
using OneScript.Runtime.Binding;

namespace OneScript.Native.Compiler
{
    public class RuntimeSymbol : IBoundSymbol
    {
        public string Name { get; set; }
        
        public string Alias { get; set; }
        
        public object Target { get; set; }
    }
    
    public class VariableSymbol : RuntimeSymbol, IVariableSymbol
    {
        public Type Type { get; set; }
    }
    
    public class PropertySymbol : RuntimeSymbol, IPropertySymbol
    {
        public Type Type => Property.PropertyType;
        
        public PropertyInfo Property { get; set; }
    }
    
    public class MethodSymbol : RuntimeSymbol, IMethodSymbol
    {
        public MethodInfo Method { get; set; }
    }
}