/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using System.Linq;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.StandardLibrary.Binary;
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

		public TypeDescription(IEnumerable<BslTypeValue> types = null,
		                           NumberQualifiers numberQualifiers = null,
		                           StringQualifiers stringQualifiers = null,
		                           DateQualifiers   dateQualifiers = null,
		                           BinaryDataQualifiers binaryDataQualifiers = null)
		{
			if (types != null)
			{
				_types.AddRange(types);
			}

			NumberQualifiers = numberQualifiers != null && _types.Contains(TypeNumber()) ?
				numberQualifiers : new NumberQualifiers();
			StringQualifiers = stringQualifiers != null && _types.Contains(TypeString()) ?
				stringQualifiers : new StringQualifiers();
			DateQualifiers = dateQualifiers != null && _types.Contains(TypeDate()) ?
				dateQualifiers : new DateQualifiers();
			BinaryDataQualifiers = binaryDataQualifiers != null && _types.Any(x => x.TypeValue.Name == TYPE_BINARYDATA_NAME) ? 
				binaryDataQualifiers : new BinaryDataQualifiers();
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

		private static IList<BslTypeValue> ConstructTypeList(ITypeManager typeManager, IValue types)
		{
			var typesList = new List<BslTypeValue>();
			if (types == null)
				return typesList;

			types = types.GetRawValue();
			if (types.SystemType == BasicTypes.String)
			{
				var typeNames = types.AsString().Split(',');
				foreach (var typeName in typeNames)
				{
					if (string.IsNullOrWhiteSpace(typeName))
						continue;

					var typeValue = new BslTypeValue(typeManager.GetTypeByName(typeName.Trim()));
					if (!typesList.Contains(typeValue))
						typesList.Add(typeValue);
				}
			} else if (types is ArrayImpl arrayOfTypes)
			{
				foreach (var type in arrayOfTypes)
				{
					var rawType = type.GetRawValue() as BslTypeValue;
					if (rawType == null)
						continue;

					if (!typesList.Contains(rawType))
						typesList.Add(rawType);
				}
			} 
			else if (types.SystemType == BasicTypes.Undefined)
			{
				return null; // далее будет исключение
			}
			// для Неопределено возвращается пустой список

			return typesList;
		}

		static BslTypeValue TypeNumber()
		{
			return new BslTypeValue(BasicTypes.Number);
		}

		static BslTypeValue TypeBoolean()
		{
			return new BslTypeValue(BasicTypes.Boolean);
		}

		static BslTypeValue TypeString()
		{
			return new BslTypeValue(BasicTypes.String);
		}
		
		static BslTypeValue TypeDate()
		{
			return new BslTypeValue(BasicTypes.Date);
		}

		public static TypeDescription StringType(int length = 0,
		                                         AllowedLengthEnum allowedLength = AllowedLengthEnum.Variable)
		{
			var stringQualifier = new StringQualifiers(length, allowedLength);
			return new TypeDescription(new BslTypeValue[] { TypeString() }, null, stringQualifier);
		}

		public static TypeDescription IntegerType(int length = 10,
		                                          AllowedSignEnum allowedSign = AllowedSignEnum.Any)
		{
			var numberQualifier = new NumberQualifiers(length, 0, allowedSign);
			return new TypeDescription(new BslTypeValue[] { TypeNumber() }, numberQualifier);
		}

		public static TypeDescription BooleanType()
		{
			return new TypeDescription(new BslTypeValue[] { TypeBoolean() });
		}


		private class TypeQualifiersSet
		{
			public readonly NumberQualifiers numberQualifiers = null;
			public readonly StringQualifiers stringQualifiers = null;
			public readonly DateQualifiers dateQualifiers = null;
			public readonly BinaryDataQualifiers binaryDataQualifiers = null;

			public TypeQualifiersSet(IValue p2, IValue p3, IValue p4, IValue p5, IValue p6, IValue p7)
			{
				int nParam = 1;
				foreach (var qual in new[] { p2, p3, p4, p5, p6, p7 })
				{
					nParam++;

					if (qual == null)
						continue;

					var rawQual = qual.GetRawValue();

					if (rawQual is NumberQualifiers)
					{
						numberQualifiers = (NumberQualifiers)rawQual;
					}
					else if (rawQual is StringQualifiers)
					{
						stringQualifiers = (StringQualifiers)rawQual;
					}
					else if (rawQual is DateQualifiers)
					{
						dateQualifiers = (DateQualifiers)rawQual;
					}
					else if (rawQual is BinaryDataQualifiers)
					{
						binaryDataQualifiers = (BinaryDataQualifiers)rawQual;
					}
					else
					{
						throw RuntimeException.InvalidNthArgumentType(nParam);
					}
				}
			}
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

				// только для контроля типов
				var _ = new TypeQualifiersSet(p2, p3, p4, p5, p6, p7);

				return new TypeDescription();
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

		public static TypeDescription ConstructByQualifiers(ITypeManager typeManager, IValue types,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var typesList = ConstructTypeList(typeManager, types);
			if (typesList == null)
				throw RuntimeException.InvalidNthArgumentType(1);

			var qualSet = new TypeQualifiersSet(p2,p3,p4,p5,p6,p7);

			return new TypeDescription(typesList,
				qualSet.numberQualifiers,
				qualSet.stringQualifiers,
				qualSet.dateQualifiers,
				qualSet.binaryDataQualifiers);
		}

		public static TypeDescription ConstructByOtherDescription(ITypeManager typeManager,
			IValue typeDescription = null,
			IValue addTypes = null,
			IValue removeTypes = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var removeTypesList = ConstructTypeList(typeManager, removeTypes);
			if (removeTypesList == null)
				throw RuntimeException.InvalidNthArgumentType(3);

			var typesList = new List<BslTypeValue>();
			if (typeDescription is TypeDescription typeDesc)
			{
				foreach (var type in typeDesc._types)
				{
					if (!removeTypesList.Contains(type))
					{
						typesList.Add(type);
					}
				}
			}

			var addTypesList = ConstructTypeList(typeManager, addTypes);
			if (addTypesList == null)
				throw RuntimeException.InvalidNthArgumentType(2);
			typesList.AddRange(addTypesList);

			var qualSet = new TypeQualifiersSet(null, null, p4, p5, p6, p7);

			return new TypeDescription(typesList,
				qualSet.numberQualifiers,
				qualSet.stringQualifiers,
				qualSet.dateQualifiers,
				qualSet.binaryDataQualifiers);
		}
	}
}
