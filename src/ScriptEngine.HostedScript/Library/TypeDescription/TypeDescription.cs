/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using ScriptEngine.Machine.Values;

namespace ScriptEngine.HostedScript.Library
{
	[ContextClass("ОписаниеТипов", "TypeDescription")]
	public class TypeDescription : AutoContext<TypeDescription>
	{
		private readonly List<TypeTypeValue> _types = new List<TypeTypeValue>();

		public TypeDescription(IEnumerable<TypeTypeValue> types = null,
		                           NumberQualifiers numberQualifiers = null,
		                           StringQualifiers stringQualifiers = null,
		                           DateQualifiers   dateQualifiers = null,
		                           BinaryDataQualifiers binaryDataQualifiers = null)
		{
			if (types != null)
			{
				_types.AddRange(types);
			}

			NumberQualifiers = numberQualifiers ?? new NumberQualifiers();
			StringQualifiers = stringQualifiers ?? new StringQualifiers();
			DateQualifiers = dateQualifiers ?? new DateQualifiers();
			BinaryDataQualifiers = binaryDataQualifiers ?? new BinaryDataQualifiers();
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
			if (type is TypeTypeValue typeVal)
				return _types.Contains(typeVal);

			throw RuntimeException.InvalidArgumentType(nameof(type));
		}

		IValueAdjuster GetAdjusterForType(TypeTypeValue type)
		{
			if (type.Value.Equals(TypeDescriptor.FromDataType(DataType.Number)))
				return NumberQualifiers;

			if (type.Value.Equals(TypeDescriptor.FromDataType(DataType.String)))
				return StringQualifiers;

			if (type.Value.Equals(TypeDescriptor.FromDataType(DataType.Date)))
				return DateQualifiers;
			
			if (type.Value.Equals(TypeDescriptor.FromDataType(DataType.Boolean)))
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

			TypeTypeValue typeToCast = null;

			if (value != null && value.DataType != DataType.Undefined)
			{
				var valueType = new TypeTypeValue(value.SystemType);
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

		private static IList<TypeTypeValue> ConstructTypeList(IValue types)
		{
			var typesList = new List<TypeTypeValue>();
			if (types == null)
				return typesList;

			types = types.GetRawValue();
			if (types.DataType == DataType.String)
			{
				var typeNames = types.AsString().Split(',');
				foreach (var typeName in typeNames)
				{
					var typeValue = new TypeTypeValue(typeName.Trim());
					if (!typesList.Contains(typeValue))
						typesList.Add(typeValue);
				}
			}
			else if (types is ArrayImpl)
			{
				foreach (var type in (types as ArrayImpl))
				{
					var rawType = type.GetRawValue() as TypeTypeValue;
					if (rawType == null)
						continue;

					if (!typesList.Contains(rawType))
						typesList.Add(rawType);
				}
			} else
			{
				return null;
			}
			return typesList;
		}

		static TypeTypeValue TypeNumber()
		{
			return new TypeTypeValue(TypeManager.GetTypeById((int)DataType.Number));
		}

		static TypeTypeValue TypeBoolean()
		{
			return new TypeTypeValue(TypeManager.GetTypeById((int)DataType.Boolean));
		}

		static TypeTypeValue TypeString()
		{
			return new TypeTypeValue(TypeManager.GetTypeById((int)DataType.String));
		}

		public static TypeDescription StringType(int length = 0,
		                                         AllowedLengthEnum allowedLength = AllowedLengthEnum.Variable)
		{
			var stringQualifier = new StringQualifiers(length, allowedLength);
			return new TypeDescription(new TypeTypeValue[] { TypeString() }, null, stringQualifier);
		}

		public static TypeDescription IntegerType(int length = 10,
		                                          AllowedSignEnum allowedSign = AllowedSignEnum.Any)
		{
			var numberQualifier = new NumberQualifiers(length, 0, allowedSign);
			return new TypeDescription(new TypeTypeValue[] { TypeNumber() }, numberQualifier);
		}

		public static TypeDescription BooleanType()
		{
			return new TypeDescription(new TypeTypeValue[] { TypeBoolean() });
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
			IValue source = null,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var rawSource = source?.GetRawValue();

			if (rawSource == null || rawSource.DataType == DataType.Undefined)
			{
				// пустой первый параметр - нет объекта-основания
				// добавляемые/вычитаемые типы не допускаются, квалификаторы игнорируются

				// только для контроля типов
				var _ = new TypeQualifiersSet(p2, p3, p4, p5, p6, p7);

				return new TypeDescription();
			}

			if (rawSource is TypeDescription)
			{
				return ConstructByOtherDescription(rawSource, p2, p3, p4, p5, p6, p7);
			}

			if (rawSource.DataType == DataType.String || rawSource is ArrayImpl)
			{
				return ConstructByQualifiers(rawSource, p2, p3, p4, p5, p6, p7);
			}

			throw RuntimeException.InvalidArgumentValue();
		}

		public static TypeDescription ConstructByQualifiers(
			IValue types,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var typesList = ConstructTypeList(types);
			if (typesList == null)
				throw RuntimeException.InvalidNthArgumentType(1);

			var qualSet = new TypeQualifiersSet(p2,p3,p4,p5,p6,p7);

			return new TypeDescription(typesList,
				qualSet.numberQualifiers,
				qualSet.stringQualifiers,
				qualSet.dateQualifiers,
				qualSet.binaryDataQualifiers);
		}

		public static TypeDescription ConstructByOtherDescription(
			IValue typeDescription = null,
			IValue addTypes = null,
			IValue removeTypes = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var removeTypesList = ConstructTypeList(removeTypes);
			if (removeTypesList == null)
				throw RuntimeException.InvalidNthArgumentType(3);

			var typesList = new List<TypeTypeValue>();
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

			var addTypesList = ConstructTypeList(addTypes);
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
