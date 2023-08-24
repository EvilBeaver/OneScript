using System.Collections.Generic;
using System.Linq;
using OneScript.Exceptions;
using OneScript.Values;
using ScriptEngine.Machine;

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
            if (qualifier != null && !qualifier.Equals(BslUndefinedValue.Instance))
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
            _types = new List<BslTypeValue>(_types.Distinct());
            var hasNumber = _types.Contains(TypeDescription.TypeNumber());
            var hasString =_types.Contains(TypeDescription.TypeString());
            var hasDate = _types.Contains(TypeDescription.TypeDate());
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

        public static TypeDescription Build(BslTypeValue type, IValue qualifier = null)
        {
            var builder = new TypeDescriptionBuilder();
            return builder.AddTypes(new[] { type }).AddQualifier(qualifier).Build();
        }
    }
}