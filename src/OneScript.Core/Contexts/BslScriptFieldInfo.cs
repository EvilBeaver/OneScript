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
using OneScript.Contexts.Internal;
using OneScript.Values;

namespace OneScript.Contexts
{
    public class BslScriptFieldInfo : BslFieldInfo, IBuildableMember
    {
        private Type _declaringType;
        private bool _isExported;
        private string _name;
        private Type _dataType = typeof(BslValue);
        private int _dispId = -1;
        
        internal BslScriptFieldInfo(string name)
        {
            _name = name;
        }

        public override Type DeclaringType => _declaringType;
        public override string Name => _name;
        public override Type ReflectedType => _declaringType;

        public int DispatchId => _dispId;
        
        public override object GetValue(object obj)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public override FieldAttributes Attributes => _isExported ? FieldAttributes.Public : FieldAttributes.Private;
        public override RuntimeFieldHandle FieldHandle => default;
        public override Type FieldType => _dataType;
        
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
            _isExported = isExport;
        }

        void IBuildableMember.SetDataType(Type type)
        {
            _dataType = type;
        }

        void IBuildableMember.SetAnnotations(IEnumerable<object> annotations)
        {
            SetAnnotations(new AnnotationHolder(annotations.ToArray()));
        }

        void IBuildableMember.SetDispatchIndex(int index)
        {
            _dispId = index;
        }
    }
}