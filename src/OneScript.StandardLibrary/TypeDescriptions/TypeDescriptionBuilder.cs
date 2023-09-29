/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Types;
using OneScript.Values;

namespace OneScript.StandardLibrary.TypeDescriptions
{
    internal class TypeDescriptionBuilder
    {
        private NumberQualifiers _numberQualifiers;
        private StringQualifiers _stringQualifiers;
        private DateQualifiers _dateQualifiers;
        private BinaryDataQualifiers _binaryDataQualifiers;
        
        private const string TYPE_BINARYDATA_NAME = "ДвоичныеДанные";

        private List<BslTypeValue> _types = new List<BslTypeValue>();
        
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

        public TypeDescriptionBuilder AddTypes(IEnumerable<BslTypeValue> types)
        {
            _types.AddRange(types);
            return this;
        }

        public TypeDescriptionBuilder RemoveTypes(IEnumerable<BslTypeValue> types)
        {
            _types.RemoveAll(types.Contains);
            return this;
        }

        public TypeDescriptionBuilder SetNumberQualifiers(NumberQualifiers nq)
        {
            _numberQualifiers = nq;
            return this;
        }

        public TypeDescriptionBuilder SetStringQualifiers(StringQualifiers sq)
        {
            _stringQualifiers = sq;
            return this;
        }

        public TypeDescriptionBuilder SetDateQualifiers(DateQualifiers dq)
        {
            _dateQualifiers = dq;
            return this;
        }

        public TypeDescriptionBuilder SetBinaryDataQualifiers(BinaryDataQualifiers bq)
        {
            _binaryDataQualifiers = bq;
            return this;
        }

        public TypeDescription Build()
        {
            _types = new List<BslTypeValue>(_types.Distinct());
            _types.RemoveAll(type => type.TypeValue.ImplementingClass == typeof(BslUndefinedValue));
            _types.Sort(new TypeComparer());
            var hasNumber = _types.Any(type => type.TypeValue == BasicTypes.Number);
            var hasString =_types.Any(type => type.TypeValue == BasicTypes.String);
            var hasDate = _types.Any(type => type.TypeValue == BasicTypes.Date);
            var hasBinaryData = _types.Any(x => x.TypeValue.Name == TYPE_BINARYDATA_NAME);
            
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

        public static TypeDescriptionBuilder OfType(TypeDescriptor commonType)
        {
            var builder = new TypeDescriptionBuilder();
            builder.AddTypes(new[] { new BslTypeValue(commonType) });
            return builder;
        }
    }
}