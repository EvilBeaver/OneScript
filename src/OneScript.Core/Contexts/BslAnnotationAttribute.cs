/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Values;

namespace OneScript.Contexts
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Parameter)]
    public class BslAnnotationAttribute : Attribute
    {
        public BslAnnotationAttribute(string name)
        {
            Name = name;
        }
        
        public string Name { get; }

        public IEnumerable<BslAnnotationParameter> Parameters { get; private set; }

        public void SetParameters(IEnumerable<BslAnnotationParameter> parameters)
        {
            Parameters = new List<BslAnnotationParameter>(parameters);
        }
    }

    public class BslAnnotationParameter
    {
        public BslAnnotationParameter(string name, BslPrimitiveValue value)
        {
            Name = name;
            Value = value;
        }
        
        public BslAnnotationParameter(string name, int constantValueIndex)
        {
            Name = name;
            ConstantValueIndex = constantValueIndex;
        }
        
        public string Name { get; }
        
        public BslPrimitiveValue Value { get; }

        public int ConstantValueIndex { get; set; } = -1;
    }
}