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
			var result = ArrayImpl.Constructor() as ArrayImpl;

			foreach (var type in _types)
			{
				result.Add(type);
			}

			return result;
		}

		[ContextMethod("СодержитТип", "ContainsType")]
		public bool ContainsType(IValue type)
		{
			if (type is TypeTypeValue)
				return _types.IndexOf(type as TypeTypeValue) != -1;
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
			var _types = new List<TypeTypeValue>();
			if (types == null)
				return _types;

			types = types.GetRawValue();
			if (types.DataType == DataType.String)
			{
				var typeNames = types.AsString().Split(',');
				foreach (var typeName in typeNames)
				{
					_types.Add(new TypeTypeValue(typeName.Trim()));
				}
			} else if (types is ArrayImpl)
			{
				foreach (var type in (types as ArrayImpl))
				{
					var rawType = type.GetRawValue() as TypeTypeValue;
					if (rawType == null)
						return null;

					_types.Add(rawType);
				}
			} else
			{
				return null;
			}
			return _types;
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

		[ScriptConstructor]
		public static TypeDescription Constructor(
			IValue source = null,
			IValue p1 = null,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null)
		{
			var rawSource = source?.GetRawValue();

			if (rawSource == null || rawSource.DataType == DataType.Undefined)
			{
				// первый параметр имеет право быть не задан только в таком конструкторе
				return ConstructByOtherDescription(null, p1, p2, p3, p4, p5, p6);
			}

			if (rawSource is TypeDescription)
			{
				return ConstructByOtherDescription(rawSource, p1, p2, p3, p4, p5, p6);
			}

			if (rawSource.DataType == DataType.String || rawSource is ArrayImpl)
			{
				// TODO: проверить, что p5 и p6 не заданы
				return ConstructByQualifiers(rawSource, p1, p2, p3, p4);
			}

			throw RuntimeException.InvalidArgumentValue();
		}

		public static TypeDescription ConstructByQualifiers(
			IValue types,
			IValue numberQualifiers = null,
			IValue stringQualifiers = null,
			IValue dateQualifiers = null,
			IValue binaryDataQualifiers = null)
		{
			var _types = ConstructTypeList(types);
			if (_types == null)
				throw RuntimeException.InvalidArgumentType(nameof(types));

			var paramNumberQ = numberQualifiers?.GetRawValue() as NumberQualifiers;
			var paramStringQ = stringQualifiers?.GetRawValue() as StringQualifiers;
			var paramDateQ = dateQualifiers?.GetRawValue() as DateQualifiers;
			var paramBinaryDataQ = binaryDataQualifiers?.GetRawValue() as BinaryDataQualifiers;

			return new TypeDescription(_types, paramNumberQ, paramStringQ, paramDateQ, paramBinaryDataQ);
		}

		public static TypeDescription ConstructByOtherDescription(
			IValue typeDescription = null,
			IValue addTypes = null,
			IValue removeTypes = null,
			IValue numberQualifiers = null,
			IValue stringQualifiers = null,
			IValue dateQualifiers = null,
			IValue binaryDataQualifiers = null)
		{
			var td = typeDescription as TypeDescription;

			var removeTypesList = ConstructTypeList(removeTypes);
			if (removeTypesList == null)
				throw RuntimeException.InvalidArgumentType(nameof(removeTypes));


			var _types = new List<TypeTypeValue>();
			if (td != null)
			{
				foreach (var ivType in td.Types())
				{
					var type = ivType as TypeTypeValue;
					if (removeTypesList.IndexOf(type) == -1)
					{
						_types.Add(type);
					}
				}
			}

			var addTypesList = ConstructTypeList(addTypes);
			if (addTypesList == null)
				throw RuntimeException.InvalidArgumentType(nameof(addTypes));
			_types.AddRange(addTypesList);

			var paramNumberQ = numberQualifiers?.AsObject() as NumberQualifiers ?? td?.NumberQualifiers;
			var paramStringQ = stringQualifiers?.AsObject() as StringQualifiers ?? td?.StringQualifiers;
			var paramDateQ = dateQualifiers?.AsObject() as DateQualifiers ?? td?.DateQualifiers;
			var paramBinaryDataQ = binaryDataQualifiers?.AsObject() as BinaryDataQualifiers ?? td?.BinaryDataQualifiers;

			return new TypeDescription(_types, paramNumberQ, paramStringQ, paramDateQ, paramBinaryDataQ);
		}
	}
}
