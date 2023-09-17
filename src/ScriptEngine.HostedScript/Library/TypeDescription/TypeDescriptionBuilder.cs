/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using ScriptEngine.HostedScript.Library.Binary;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.HostedScript.Library
{
    internal class TypeDescriptionBuilder
    {
        
        private static readonly TypeDescriptor BinaryDataType = TypeManager.GetTypeByFrameworkType(typeof(BinaryDataContext));
        private static readonly TypeDescriptor NullDataType = TypeManager.GetTypeByFrameworkType(typeof(NullValue)); 

        private static readonly TypeDescriptor[] Primitives = {
            TypeDescriptor.FromDataType(DataType.Boolean),
            BinaryDataType,
            TypeDescriptor.FromDataType(DataType.String),
            TypeDescriptor.FromDataType(DataType.Date),
            NullDataType,
            TypeDescriptor.FromDataType(DataType.Number),
            TypeDescriptor.FromDataType(DataType.Type)
        };
        
        private NumberQualifiers _numberQualifiers;
        private StringQualifiers _stringQualifiers;
        private DateQualifiers _dateQualifiers;
        private BinaryDataQualifiers _binaryDataQualifiers;

        private List<TypeTypeValue> _types = new List<TypeTypeValue>();
        
        internal TypeDescriptionBuilder()
        {
        }

        public TypeDescriptionBuilder SourceDescription(TypeDescription source)
        {
            _numberQualifiers = source.NumberQualifiers;
            _stringQualifiers = source.StringQualifiers;
            _dateQualifiers = source.DateQualifiers;
            _binaryDataQualifiers = source.BinaryDataQualifiers;
            return AddTypes(source.TypesInternal());
        }

        public TypeDescriptionBuilder AddTypes(IEnumerable<TypeTypeValue> types)
        {
            _types.AddRange(types);
            return this;
        }

        public TypeDescriptionBuilder RemoveTypes(IEnumerable<TypeTypeValue> types)
        {
            _types.RemoveAll(types.Contains);
            return this;
        }

        public TypeDescriptionBuilder AddQualifiers(IValue[] qualifiers, int nParam = 0)
        {
            foreach (var qualifier in qualifiers)
            {
                nParam++;
                AddQualifier(qualifier, nParam);
            }

            return this;
        }

        public TypeDescriptionBuilder AddQualifier(IValue qualifier, int nParam = 0)
        {
            if (qualifier != null && !qualifier.Equals(ValueFactory.Create()))
            {
                switch (qualifier.GetRawValue())
                {
                    case NumberQualifiers nq:
                        _numberQualifiers = nq;
                        break;

                    case StringQualifiers sq:
                        _stringQualifiers = sq;
                        break;

                    case DateQualifiers dq:
                        _dateQualifiers = dq;
                        break;

                    case BinaryDataQualifiers bdq:
                        _binaryDataQualifiers = bdq;
                        break;

                    default:
                        throw nParam == 0
                            ? RuntimeException.InvalidArgumentType()
                            : RuntimeException.InvalidNthArgumentType(nParam);
                }
            }

            return this;
        }

        public TypeDescription Build()
        {
            _types = new List<TypeTypeValue>(_types.Distinct());
            _types.RemoveAll(type => type.Value.Equals(UndefinedValue.Instance.SystemType));
            _types.Sort(new TypeComparer());
            var hasNumber = _types.Contains(TypeDescription.TypeNumber());
            var hasString =_types.Contains(TypeDescription.TypeString());
            var hasDate = _types.Contains(TypeDescription.TypeDate());
            var hasBinaryData = _types.Any(x => x.Value.Equals(BinaryDataType));
            
            if (!hasNumber || _numberQualifiers == null) _numberQualifiers = new NumberQualifiers();
            if (!hasString || _stringQualifiers == null) _stringQualifiers = new StringQualifiers();
            if (!hasDate || _dateQualifiers == null) _dateQualifiers = new DateQualifiers();
            if (!hasBinaryData || _binaryDataQualifiers == null) _binaryDataQualifiers = new BinaryDataQualifiers();

            return new TypeDescription(_types,
                _numberQualifiers,
                _stringQualifiers,
                _dateQualifiers,
                _binaryDataQualifiers
            );

        }

        public static TypeDescription Build(TypeTypeValue type, IValue qualifier = null)
        {
            var builder = new TypeDescriptionBuilder();
            return builder.AddTypes(new[] { type }).AddQualifier(qualifier).Build();
        }
        
        private class TypeComparer : IComparer<TypeTypeValue>
        {
            public int Compare(TypeTypeValue x, TypeTypeValue y)
            {
                if (x == null)
                {
                    return y == null ? 0 : -1;
                }

                if (y == null) return 1;

                var primitiveX = PrimitiveIndex(x);
                var primitiveY = PrimitiveIndex(y);
                
                if (primitiveX != -1)
                {
                    if (primitiveY != -1)
                        return primitiveX - primitiveY;

                    return -1;
                }

                if (primitiveY != -1)
                    return 1;

                return x.Value.ID.CompareTo(y.Value.ID);
            }

            private static int PrimitiveIndex(TypeTypeValue type)
            {
                var typeDescriptor = TypeManager.GetTypeDescriptorFor(type);
                for (var primitiveIndex = 0; primitiveIndex < Primitives.Length; primitiveIndex++)
                {
                    if (typeDescriptor.Equals(Primitives[primitiveIndex]))
                    {
                        return primitiveIndex;
                    }
                }
                
                return -1;
            }

        }
    }
}