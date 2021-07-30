/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OneScript.Types;
using OneScript.Values;

namespace OneScript.Contexts
{
    public delegate BslValue InstanceConstructor(TypeActivationContext context, BslValue[] arguments);
    
    public class BslLambdaConstructorInfo : BslConstructorInfo, IBuildableMethod
    {
        private readonly InstanceConstructor _implementation;
        private Type _declaringType;
        
        internal BslLambdaConstructorInfo(InstanceConstructor implementation)
        {
            _implementation = implementation;
        }
        
        internal List<BslParameterInfo> Parameters { get; } = new List<BslParameterInfo>();

        public override Type DeclaringType => _declaringType;
        public override string Name => ConstructorName;
        public override Type ReflectedType => _declaringType;
        
        public override ParameterInfo[] GetParameters()
        {
            return Parameters.Cast<ParameterInfo>().ToArray();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        void IBuildableMember.SetDeclaringType(Type type)
        {
            _declaringType = type;
        }

        void IBuildableMember.SetName(string name)
        {
        }

        void IBuildableMember.SetAlias(string alias)
        {
        }

        void IBuildableMember.SetExportFlag(bool isExport)
        {
        }

        void IBuildableMember.SetDataType(Type type)
        {
        }

        void IBuildableMember.SetAnnotations(IEnumerable<object> annotations)
        {
        }

        void IBuildableMember.SetDispatchIndex(int index)
        {
        }

        void IBuildableMethod.SetParameters(IEnumerable<BslParameterInfo> parameters)
        {
        }
    }
}