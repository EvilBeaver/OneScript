﻿/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScriptEngine.Machine.Contexts
{
    public class EnumerationContext : PropertyNameIndexAccessor
    {
        private readonly List<EnumerationValue> _values = new List<EnumerationValue>();

        readonly IndexedNamesCollection _nameIds = new IndexedNamesCollection();
        private readonly TypeDescriptor _valuesType;

        public EnumerationContext(TypeDescriptor typeRepresentation, TypeDescriptor valuesType) : base(typeRepresentation)
        {
            _valuesType = valuesType;
        }

        public void AddValue(string name, EnumerationValue val)
        {
            AddValue(name, null, val);
        }

        public void AddValue(string name, string alias, EnumerationValue val)
        {
            System.Diagnostics.Debug.Assert(name != null);
            System.Diagnostics.Debug.Assert(val != null);

            if (!ScriptEngine.Utils.IsValidIdentifier(name))
                throw new ArgumentException("Name must be a valid identifier", "name");

            if(alias != null && !ScriptEngine.Utils.IsValidIdentifier(alias))
                throw new ArgumentException("Name must be a valid identifier", "alias");

            _nameIds.RegisterName(name, alias);
            val.ValuePresentation = name;
            _values.Add(val);

        }

        public TypeDescriptor ValuesType
        {
            get
            {
                return _valuesType;
            }
        }

        public EnumerationValue this[string name]
        {
            get
            {
                int id = FindProperty(name);
                return _values[id];
            }
        }

        public int IndexOf(EnumerationValue enumVal)
        {
            return _values.IndexOf(enumVal);
        }

        public override int FindProperty(string name)
        {
            int id;
            if (_nameIds.TryGetIdOfName(name, out id))
                return id;
            else
                return base.FindProperty(name);
        }

        public override bool IsPropReadable(int propNum)
        {
            return true;
        }

        public override IValue GetPropValue(int propNum)
        {
            return _values[propNum];
        }

        protected IList<EnumerationValue> ValuesInternal
        {
            get
            {
                return _values;
            }
        }
    }
}
