/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using OneScript.Contexts.Internal;

namespace OneScript.Contexts
{
    public static class BslPropertyBuilder
    {
        public static BslPropertyBuilder<BslScriptPropertyInfo> Create() => 
            new BslPropertyBuilder<BslScriptPropertyInfo>(BslScriptPropertyInfo.Create());
    }
    
    public class BslPropertyBuilder<T> where T : BslScriptPropertyInfo
    {
        private readonly IBuildableProperty _member;

        internal BslPropertyBuilder(IBuildableProperty member)
        {
            _member = member;
        }
        
        public BslPropertyBuilder<T> SetNames(string methodNameRu, string methodNameEn)
        {
            _member.SetName(methodNameRu);
            _member.SetAlias(methodNameEn);
            return this;
        }

        public T Build()
        {
            return (T)_member;
        }
        
        public BslPropertyBuilder<T> Name(string name)
        {
            _member.SetName(name);
            return this;
        }
        
        public BslPropertyBuilder<T> Alias(string name)
        {
            _member.SetAlias(name);
            return this;
        }

        public BslPropertyBuilder<T> DeclaringType(Type type)
        {
            _member.SetDeclaringType(type);
            return this;
        }
        
        public BslPropertyBuilder<T> ReturnType(Type type)
        {
            _member.SetDataType(type);
            return this;
        }
        
        public BslPropertyBuilder<T> IsExported(bool exportFlag)
        {
            _member.SetExportFlag(exportFlag);
            return this;
        }

        public BslPropertyBuilder<T> SetAnnotations(IEnumerable<object> annotations)
        {
            _member.SetAnnotations(annotations);
            return this;
        }
        
        public BslPropertyBuilder<T> SetDispatchingIndex(int index)
        {
            _member.SetDispatchIndex(index);
            return this;
        }

        public BslPropertyBuilder<T> CanRead(bool canRead)
        {
            _member.CanRead(canRead);
            return this;
        }
        
        public BslPropertyBuilder<T> CanWrite(bool canWrite)
        {
            _member.CanWrite(canWrite);
            return this;
        }
    }
}