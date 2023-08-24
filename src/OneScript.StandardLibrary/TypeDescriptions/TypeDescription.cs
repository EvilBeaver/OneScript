/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.StandardLibrary.Collections;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.TypeDescriptions
{
	[ContextClass("ОписаниеТипов", "TypeDescription")]
	public class TypeDescription : AutoContext<TypeDescription>
	{
		private readonly List<BslTypeValue> _types = new List<BslTypeValue>();

		private const string TYPE_BINARYDATA_NAME = "ДвоичныеДанные";

		public TypeDescription(IEnumerable<BslTypeValue> types = null)
		{
			if (types != null)
			{
				_types.AddRange(types);
			}
			
			NumberQualifiers = new NumberQualifiers();
			StringQualifiers = new StringQualifiers();
			DateQualifiers = new DateQualifiers();
			BinaryDataQualifiers = new BinaryDataQualifiers();
		}

		internal TypeDescription(IEnumerable<BslTypeValue> types,
		                           NumberQualifiers numberQualifiers,
		                           StringQualifiers stringQualifiers,
		                           DateQualifiers   dateQualifiers,
		                           BinaryDataQualifiers binaryDataQualifiers)
		{
			if (types != null)
			{
				_types.AddRange(types);
			}

			NumberQualifiers = numberQualifiers;
			StringQualifiers = stringQualifiers;
			DateQualifiers = dateQualifiers;
			BinaryDataQualifiers = binaryDataQualifiers;
		}

		[ContextProperty("КвалификаторыЧисла", "NumberQualifiers")]
		public NumberQualifiers NumberQualifiers { get; }

		[ContextProperty("КвалификаторыСтроки", "StringQualifiers")]
		public StringQualifiers StringQualifiers { get; }

		[ContextProperty("КвалификаторыДаты", "DateQualifiers")]
		public DateQualifiers DateQualifiers { get; }

		[ContextProperty("КвалификаторыДвоичныхДанных", "BinaryDataQualifiers")]
		public BinaryDataQualifiers BinaryDataQualifiers { get; }

		[ContextMethod("Типы", "Types")]
		public ArrayImpl Types()
		{
			var result = ArrayImpl.Constructor();

			foreach (var type in _types)
			{
				result.Add(type);
			}

			return result;
		}

        internal IEnumerable<BslTypeValue> TypesInternal()
        {
	        return _types;
        }

		[ContextMethod("СодержитТип", "ContainsType")]
		public bool ContainsType(IValue type)
		{
			if (type is BslTypeValue typeVal)
				return _types.Contains(typeVal);

			throw RuntimeException.InvalidArgumentType(nameof(type));
		}

		IValueAdjuster GetAdjusterForType(BslTypeValue type)
		{
			var value = type.TypeValue;
			
			if (value.Equals(BasicTypes.Number))
				return NumberQualifiers;

			if (value.Equals(BasicTypes.String))
				return StringQualifiers;

			if (value.Equals(BasicTypes.Date))
				return DateQualifiers;
			
			if (value.Equals(BasicTypes.Boolean))
				return new BooleanTypeAdjuster();

			return null;
		}

		[ContextMethod("ПривестиЗначение", "AdjustValue")]
		public IValue AdjustValue(IValue value = null)
		{

			if (_types.Count == 0)
			{
				return value ?? ValueFactory.Create();
			}

			BslTypeValue typeToCast = null;

			if (value != null && value.SystemType != BasicTypes.Undefined)
			{
				var valueType = new BslTypeValue(value.SystemType);
				if (_types.Contains(valueType))
				{
					// Если такой тип у нас есть
					typeToCast = valueType;
				}
			}

			if (typeToCast == null)
			{
				// Если типа нет, то нужно брать значение по-умолчанию
				if (_types.Count != 1)
				{
					// много типов - Неопределено
					return ValueFactory.Create();
				}

				typeToCast = _types[0];
			}

			var adjuster = GetAdjusterForType(typeToCast);

			return adjuster?.Adjust(value) ?? ValueFactory.Create();
		}

		internal static BslTypeValue TypeNumber()
		{
			return new BslTypeValue(BasicTypes.Number);
		}

		internal static BslTypeValue TypeBoolean()
		{
			return new BslTypeValue(BasicTypes.Boolean);
		}

		internal static BslTypeValue TypeString()
		{
			return new BslTypeValue(BasicTypes.String);
		}
		
		internal static BslTypeValue TypeDate()
		{
			return new BslTypeValue(BasicTypes.Date);
		}

		public static TypeDescription StringType(int length = 0,
		                                         AllowedLengthEnum allowedLength = AllowedLengthEnum.Variable)
		{
			return TypeDescriptionBuilder.Build(TypeString(), new StringQualifiers(length, allowedLength));
		}

		public static TypeDescription IntegerType(int length = 10,
		                                          AllowedSignEnum allowedSign = AllowedSignEnum.Any)
		{
			return TypeDescriptionBuilder.Build(TypeNumber(), new NumberQualifiers(length, 0, allowedSign));
		}

		public static TypeDescription BooleanType()
		{
			return TypeDescriptionBuilder.Build(TypeBoolean());
		}

		[ScriptConstructor]
		public static TypeDescription Constructor(
			TypeActivationContext context,
			IValue source = null,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var rawSource = source?.GetRawValue();

			if (rawSource == null || rawSource.SystemType == BasicTypes.Undefined)
			{
				// пустой первый параметр - нет объекта-основания
				// добавляемые/вычитаемые типы не допускаются, квалификаторы игнорируются

				// квалификакторы передаются только для контроля типов
				return ConstructByQualifiers(context.TypeManager, new TypeDescription(), p2, p3, p4, p5, p6, p7);
			}

			if (rawSource is TypeDescription)
			{
				return ConstructByOtherDescription(context.TypeManager, rawSource, p2, p3, p4, p5, p6, p7);
			}

			if (rawSource.SystemType == BasicTypes.String || rawSource is ArrayImpl)
			{
				return ConstructByQualifiers(context.TypeManager, rawSource, p2, p3, p4, p5, p6, p7);
			}

			throw RuntimeException.InvalidArgumentValue();
		}

		private static TypeDescription ConstructByQualifiers(ITypeManager typeManager, IValue types,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var builder = new TypeDescriptionBuilder();
			var typesList = TypeList.Construct(typeManager, types, 1);
			builder.AddTypes(typesList.List());

			builder.AddQualifiers(new[] { p2, p3, p4, p5, p6, p7 }, 1);

			return builder.Build();
		}

		private static TypeDescription ConstructByOtherDescription(ITypeManager typeManager,
			IValue typeDescription = null,
			IValue addTypes = null,
			IValue removeTypes = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var builder = new TypeDescriptionBuilder();

			if (typeDescription is TypeDescription typeDesc)
			{
				builder.SourceDescription(typeDesc);
			}
			
			var removeTypesList = TypeList.Construct(typeManager, removeTypes, 3);
			builder.RemoveTypes(removeTypesList.List());

			var addTypesList = TypeList.Construct(typeManager, addTypes, 2);
			builder.AddTypes(addTypesList.List());
			builder.AddQualifiers(new[] { p4, p5, p6, p7 }, 3);

			return builder.Build();
		}
	}
}
