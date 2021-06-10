// /*----------------------------------------------------------
// This Source Code Form is subject to the terms of the
// Mozilla Public License, v.2.0. If a copy of the MPL
// was not distributed with this file, You can obtain one
// at http://mozilla.org/MPL/2.0/.
// ----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace OneScript.Contexts
{
    public class BslScriptMethodInfo : BslMethodInfo, IBuildableMethod
    {
        private Type _declaringType;
        private Type _returnType;
        private string _name;
        private string _alias;
        private bool _isPrivate;
        private int _dispId = -1;

        protected BslScriptMethodInfo()
        {
        }

        internal static BslScriptMethodInfo Create()
        {
            return new BslScriptMethodInfo();
        }
        
        void IBuildableMember.SetDispatchIndex(int index)
        {
            _dispId = index;
        }
        
        void IBuildableMember.SetDeclaringType(Type type)
        {
            _declaringType = type;
        }

        void IBuildableMember.SetName(string name)
        {
            _name = name;
        }

        void IBuildableMember.SetAlias(string alias)
        {
            _alias = alias;
        }

        void IBuildableMember.SetExportFlag(bool isExport)
        {
            _isPrivate = !isExport;
        }

        void IBuildableMember.SetDataType(Type type)
        {
            _returnType = type;
        }

        void IBuildableMember.SetAnnotations(IEnumerable<object> annotations)
        {
            SetAnnotations(new AnnotationHolder(annotations.ToArray()));
        }

        void IBuildableMethod.SetParameters(IEnumerable<BslParameterInfo> parameters)
        {
            Parameters.Clear();
            Parameters.AddRange(parameters);
        }
        
        internal List<BslParameterInfo> Parameters { get; } = new List<BslParameterInfo>();
        
        public override Type ReturnType => _returnType;

        public override Type DeclaringType => _declaringType;
        
        public override string Name => _name;

        public override string Alias => default;

        public override Type ReflectedType => _declaringType;
        
        public override MethodImplAttributes GetMethodImplementationFlags() => MethodImplAttributes.Managed;

        public override ParameterInfo[] GetParameters() => Parameters.ToArray();
        
        public BslParameterInfo[] GetBslParameters() => Parameters.ToArray();

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override MethodAttributes Attributes => _isPrivate ? MethodAttributes.Private : MethodAttributes.Public;

        public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();
        
        public override MethodInfo GetBaseDefinition()
        {
            return this;
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();
    }
}