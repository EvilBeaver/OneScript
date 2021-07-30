/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Commons;
using OneScript.Contexts;
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
			if (type is BslTypeValue)
				return _types.IndexOf(type as BslTypeValue) != -1;
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
			var _types = new List<BslTypeValue>();
			if (types == null)
				return _types;

			types = types.GetRawValue();
			if (types.SystemType == BasicTypes.String)
			{
				var typeNames = types.AsString().Split(',');
				foreach (var typeName in typeNames)
				{
					var type = typeManager.GetTypeByName(typeName.Trim());
					_types.Add(new BslTypeValue(type));
				}
			} else if (types is ArrayImpl arrayOfTypes)
			{
				foreach (var type in arrayOfTypes)
				{
					var rawType = type.GetRawValue() as BslTypeValue;
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

		[ScriptConstructor]
		public static TypeDescription Constructor(
			TypeActivationContext context,
			IValue source = null,
			IValue p1 = null,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null)
		{
			var rawSource = source?.GetRawValue();

			if (rawSource == null || rawSource.SystemType == BasicTypes.Undefined)
			{
				// первый параметр имеет право быть не задан только в таком конструкторе
				return ConstructByOtherDescription(context.TypeManager, null, p1, p2, p3, p4, p5, p6);
			}

			if (rawSource is TypeDescription)
			{
				return ConstructByOtherDescription(context.TypeManager, rawSource, p1, p2, p3, p4, p5, p6);
			}

			if (rawSource.SystemType == BasicTypes.String || rawSource is ArrayImpl)
			{
				// TODO: проверить, что p5 и p6 не заданы
				return ConstructByQualifiers(context.TypeManager, rawSource, p1, p2, p3, p4);
			}

			throw RuntimeException.InvalidArgumentValue();
		}

		public static TypeDescription ConstructByQualifiers(ITypeManager typeManager, IValue types,
			IValue numberQualifiers = null,
			IValue stringQualifiers = null,
			IValue dateQualifiers = null,
			IValue binaryDataQualifiers = null)
		{
			var _types = ConstructTypeList(typeManager, types);
			if (_types == null)
				throw RuntimeException.InvalidArgumentType(nameof(types));

			var paramNumberQ = numberQualifiers?.GetRawValue() as NumberQualifiers;
			var paramStringQ = stringQualifiers?.GetRawValue() as StringQualifiers;
			var paramDateQ = dateQualifiers?.GetRawValue() as DateQualifiers;
			var paramBinaryDataQ = binaryDataQualifiers?.GetRawValue() as BinaryDataQualifiers;

			return new TypeDescription(_types, paramNumberQ, paramStringQ, paramDateQ, paramBinaryDataQ);
		}

		public static TypeDescription ConstructByOtherDescription(ITypeManager typeManager,
			IValue typeDescription = null,
			IValue addTypes = null,
			IValue removeTypes = null,
			IValue numberQualifiers = null,
			IValue stringQualifiers = null,
			IValue dateQualifiers = null,
			IValue binaryDataQualifiers = null)
		{
			var td = typeDescription as TypeDescription;

			var removeTypesList = ConstructTypeList(typeManager, removeTypes);
			if (removeTypesList == null)
				throw RuntimeException.InvalidArgumentType(nameof(removeTypes));


			var _types = new List<BslTypeValue>();
			if (td != null)
			{
				foreach (var ivType in td.Types())
				{
					var type = ivType as BslTypeValue;
					if (removeTypesList.IndexOf(type) == -1)
					{
						_types.Add(type);
					}
				}
			}

			var addTypesList = ConstructTypeList(typeManager, addTypes);
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
