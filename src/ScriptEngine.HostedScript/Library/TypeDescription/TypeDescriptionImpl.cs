/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;

namespace ScriptEngine.HostedScript.Library
{
	[ContextClass("ОписаниеТипов", "TypeDescription")]
	public class TypeDescriptionImpl : AutoContext<TypeDescriptionImpl>
	{
		private readonly List<TypeTypeValue> _types = new List<TypeTypeValue>();

		public TypeDescriptionImpl(IEnumerable<TypeTypeValue> types,
		                           NumberQualifiersImpl numberQualifiers = null,
		                           StringQualifiersImpl stringQualifiers = null,
		                           DateQualifiersImpl   dateQualifiers = null,
		                           BinaryDataQualifiersImpl binaryDataQualifiers = null)
		{
			_types.AddRange(types);
			NumberQualifiers = numberQualifiers ?? new NumberQualifiersImpl();
			StringQualifiers = stringQualifiers ?? new StringQualifiersImpl();
			DateQualifiers = dateQualifiers ?? new DateQualifiersImpl();
			BinaryDataQualifiers = binaryDataQualifiers ?? new BinaryDataQualifiersImpl();
		}

		[ContextProperty("КвалификаторыЧисла")]
		public NumberQualifiersImpl NumberQualifiers { get; }

		[ContextProperty("КвалификаторыСтроки")]
		public StringQualifiersImpl StringQualifiers { get; }

		[ContextProperty("КвалификаторыДаты")]
		public DateQualifiersImpl DateQualifiers { get; }

		[ContextProperty("КвалификаторыДвоичныхДанных")]
		public BinaryDataQualifiersImpl BinaryDataQualifiers { get; }

		[ContextMethod("Типы")]
		public ArrayImpl Types()
		{
			var result = ArrayImpl.Constructor() as ArrayImpl;

			foreach (var type in _types)
			{
				result.Add(type);
			}

			return result;
		}

		[ContextMethod("СодержитТип")]
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

		[ContextMethod("ПривестиЗначение")]
		public IValue AdjustValue(IValue value = null)
		{

			if (_types.Count == 0)
			{
				// нет типов - только Неопределено
				return ValueFactory.Create();
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

		[ScriptConstructor]
		public static IRuntimeContextInstance Constructor(
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

			if (rawSource is TypeDescriptionImpl)
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

		public static IRuntimeContextInstance ConstructByQualifiers(
			IValue types,
			IValue numberQualifiers = null,
			IValue stringQualifiers = null,
			IValue dateQualifiers = null,
			IValue binaryDataQualifiers = null)
		{
			var _types = ConstructTypeList(types);
			if (_types == null)
				throw RuntimeException.InvalidArgumentType(nameof(types));

			var paramNumberQ = numberQualifiers?.GetRawValue() as NumberQualifiersImpl;
			var paramStringQ = stringQualifiers?.GetRawValue() as StringQualifiersImpl;
			var paramDateQ = dateQualifiers?.GetRawValue() as DateQualifiersImpl;
			var paramBinaryDataQ = binaryDataQualifiers?.GetRawValue() as BinaryDataQualifiersImpl;

			return new TypeDescriptionImpl(_types, paramNumberQ, paramStringQ, paramDateQ, paramBinaryDataQ);
		}

		public static IRuntimeContextInstance ConstructByOtherDescription(
			IValue typeDescription = null,
			IValue addTypes = null,
			IValue removeTypes = null,
			IValue numberQualifiers = null,
			IValue stringQualifiers = null,
			IValue dateQualifiers = null,
			IValue binaryDataQualifiers = null)
		{
			var td = typeDescription as TypeDescriptionImpl;

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

			var paramNumberQ = numberQualifiers?.GetRawValue() as NumberQualifiersImpl ?? td?.NumberQualifiers;
			var paramStringQ = stringQualifiers?.GetRawValue() as StringQualifiersImpl ?? td?.StringQualifiers;
			var paramDateQ = dateQualifiers?.GetRawValue() as DateQualifiersImpl ?? td?.DateQualifiers;
			var paramBinaryDataQ = binaryDataQualifiers?.GetRawValue() as BinaryDataQualifiersImpl ?? td?.BinaryDataQualifiers;

			return new TypeDescriptionImpl(_types, paramNumberQ, paramStringQ, paramDateQ, paramBinaryDataQ);
		}
	}
}
