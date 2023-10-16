﻿/*----------------------------------------------------------
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
using OneScript.Contexts.Internal;

namespace OneScript.Contexts
{
    /// <summary>
    /// Информация о методе, который объявлен в пользовательском скриптовом коде
    /// </summary>
    public class BslScriptMethodInfo : BslMethodInfo, IBuildableMethod
    {
        private Type _declaringType;
        private Type _returnType = typeof(void);
        private string _name;
        private bool _isPrivate = true;
        protected BslParameterInfo[] _parameters = Array.Empty<BslParameterInfo>();

        protected BslScriptMethodInfo()
        {
        }

        internal static BslScriptMethodInfo Create()
        {
            return new BslScriptMethodInfo();
        }
        
        void IBuildableMember.SetDispatchIndex(int index)
        {
            DispatchId = index;
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
            _parameters = parameters.ToArray();
        }

        public override Type ReturnType => _returnType;

        public override Type DeclaringType => _declaringType;
        
        public override string Name => _name;

        public override string Alias => default;

        public override Type ReflectedType => _declaringType;

        public int DispatchId { get; private set; } = -1;

        public override MethodImplAttributes GetMethodImplementationFlags() => MethodImplAttributes.Managed;

        // ReSharper disable once CoVariantArrayConversion
        public override ParameterInfo[] GetParameters() => CopyParams();
        
        public BslParameterInfo[] GetBslParameters() => CopyParams();

        private BslParameterInfo[] CopyParams()
        {
            if (_parameters.Length == 0)
                return _parameters;

            var dest = new BslParameterInfo[_parameters.Length];
            _parameters.CopyTo(dest, 0);
            return dest;
        }

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