/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using OneScript.Contexts.Internal;

namespace OneScript.Contexts
{
    public class BslFieldBuilder
    {
        private IBuildableMember _member;
        
        private BslFieldBuilder()
        {
            _member = new BslScriptFieldInfo(null);
        }

        public BslFieldBuilder Name(string name)
        {
            _member.SetName(name);
            return this;
        }
        
        public BslFieldBuilder Alias(string name)
        {
            _member.SetAlias(name);
            return this;
        }

        public BslFieldBuilder DeclaringType(Type type)
        {
            _member.SetDeclaringType(type);
            return this;
        }
        
        public BslFieldBuilder ValueType(Type type)
        {
            _member.SetDataType(type);
            return this;
        }
        
        public BslFieldBuilder IsExported(bool exportFlag)
        {
            _member.SetExportFlag(exportFlag);
            return this;
        }

        public BslFieldBuilder SetAnnotations(IEnumerable<object> annotations)
        {
            _member.SetAnnotations(annotations);
            return this;
        }
        
        public BslFieldBuilder SetDispatchingIndex(int index)
        {
            _member.SetDispatchIndex(index);
            return this;
        }
        
        public BslFieldInfo Build()
        {
            var scriptField = _member as BslScriptFieldInfo;
            Debug.Assert(scriptField != null);
            if (scriptField.Name == default)
            {
                throw new InvalidOperationException("No name specified");
            }

            return scriptField;
        }
        
        public static BslFieldBuilder Create() => new BslFieldBuilder();
    }
}