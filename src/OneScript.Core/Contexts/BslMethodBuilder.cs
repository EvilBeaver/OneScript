/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using OneScript.Contexts.Internal;

namespace OneScript.Contexts
{
    public static class BslMethodBuilder
    {
        public static BslMethodBuilder<BslScriptMethodInfo> Create() => 
            new BslMethodBuilder<BslScriptMethodInfo>(BslScriptMethodInfo.Create(), () => new BslParameterInfo());
    }
    
    public class BslMethodBuilder<T> where T : BslScriptMethodInfo
    {
        private readonly IBuildableMethod _member;
        private readonly Func<BslParameterInfo> _parameterFactory;

        private readonly List<BslParameterBuilder> _parametersToBuild = new List<BslParameterBuilder>();

        internal BslMethodBuilder(IBuildableMethod member, Func<BslParameterInfo> parameterFactory)
        {
            _member = member;
            _parameterFactory = parameterFactory;
        }

        public BslMethodBuilder<T> SetNames(string methodNameRu, string methodNameEn)
        {
            _member.SetName(methodNameRu);
            _member.SetAlias(methodNameEn);
            return this;
        }

        public T Build()
        {
            var parameters = _parametersToBuild.Select(x => x.Build());
            _member.SetParameters(parameters);
            return (T)_member;
        }
        
        public BslMethodBuilder<T> Name(string name)
        {
            _member.SetName(name);
            return this;
        }
        
        public BslMethodBuilder<T> Alias(string name)
        {
            _member.SetAlias(name);
            return this;
        }

        public BslMethodBuilder<T> DeclaringType(Type type)
        {
            _member.SetDeclaringType(type);
            return this;
        }
        
        public BslMethodBuilder<T> ReturnType(Type type)
        {
            _member.SetDataType(type);
            return this;
        }
        
        public BslMethodBuilder<T> IsExported(bool exportFlag)
        {
            _member.SetExportFlag(exportFlag);
            return this;
        }

        public BslMethodBuilder<T> SetAnnotations(IEnumerable<object> annotations)
        {
            _member.SetAnnotations(annotations);
            return this;
        }
        
        public BslMethodBuilder<T> SetDispatchingIndex(int dispId)
        {
            _member.SetDispatchIndex(dispId);
            return this;
        }
        
        public BslParameterBuilder NewParameter()
        {
            var parameter = _parameterFactory();
            parameter.SetOwner((MemberInfo)_member);
            var builder = new BslParameterBuilder(parameter);
            _parametersToBuild.Add(builder);
            return builder;
        }
    }
}