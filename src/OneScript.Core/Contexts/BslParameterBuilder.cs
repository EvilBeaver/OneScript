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
    public class BslParameterBuilder
    {
        private readonly BslParameterInfo _parameter;

        internal BslParameterBuilder(BslParameterInfo info)
        {
            _parameter = info;
        }

        public BslParameterBuilder Name(string name)
        {
            ((IBuildableMember)_parameter).SetName(name);
            return this;
        }
        
        public BslParameterBuilder ByValue(bool isByValue)
        {
            _parameter.ByValue(isByValue);
            return this;
        }
        
        public BslParameterBuilder DefaultValue(BslPrimitiveValue value)
        {
            _parameter.SetDefaultValue(value);
            return this;
        }

        public BslParameterBuilder SetAnnotations(IEnumerable<object> annotations)
        {
            ((IBuildableMember)_parameter).SetAnnotations(annotations);
            return this;
        }

        public BslParameterBuilder ParameterType(Type type)
        {
            ((IBuildableMember)_parameter).SetDataType(type);
            return this;
        }
        
        internal BslParameterInfo Build()
        {
            return _parameter;
        }
    }
}