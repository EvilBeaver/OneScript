/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Reflection;

namespace OneScript.Contexts
{
    public static class BslMethodBuilder
    {
        public static BslMethodBuilder<BslScriptMethodInfo> Create() => 
            new BslMethodBuilder<BslScriptMethodInfo>(BslScriptMethodInfo.Create());
    }
    
    public class BslMethodBuilder<T> where T : BslScriptMethodInfo
    {
        private readonly IBuildableMethod _member;

        internal BslMethodBuilder(IBuildableMethod member)
        {
            _member = member;
        }

        public BslMethodBuilder<T> SetNames(string methodNameRu, string methodNameEn)
        {
            _member.SetName(methodNameRu);
            _member.SetAlias(methodNameEn);
            return this;
        }

        public T Build()
        {
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
        
        public BslMethodBuilder<T> SetParameters(IEnumerable<BslParameterInfo> parameters)
        {
            throw new NotImplementedException();
        }
    }
}