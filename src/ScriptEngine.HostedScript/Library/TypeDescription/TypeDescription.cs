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
using ScriptEngine.Machine.Values;

namespace ScriptEngine.HostedScript.Library
{
	[ContextClass("ОписаниеТипов", "TypeDescription")]
	public class TypeDescription : AutoContext<TypeDescription>
	{
		private readonly List<TypeTypeValue> _types = new List<TypeTypeValue>();

		public TypeDescription(IEnumerable<TypeTypeValue> types = null)
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

		internal TypeDescription(IEnumerable<TypeTypeValue> types,
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

        internal IEnumerable<TypeTypeValue> TypesInternal()
        {
	        return _types;
        }

		[ContextMethod("СодержитТип", "ContainsType")]
		public bool ContainsType(IValue type)
		{
			if (type is TypeTypeValue typeVal)
			{
				if (typeVal.Value.Equals(CommonTypes.Undefined))
				{
					// тип "Неопределено" содержится в любом явно определенном составном типе
					// и не содержится в типе Произвольный (когда явно не указан состав типов)
					//   или когда указан один конкретный тип
					return (_types.Count > 1);
				}
				return _types.Contains(typeVal);
			}

			throw RuntimeException.InvalidArgumentType(nameof(type));
		}

		IValueAdjuster GetAdjusterForType(TypeTypeValue type)
		{
			if (type.Value.Equals(CommonTypes.Number))
				return NumberQualifiers;

			if (type.Value.Equals(CommonTypes.String))
				return StringQualifiers;

			if (type.Value.Equals(CommonTypes.Date))
				return DateQualifiers;
			
			if (type.Value.Equals(CommonTypes.Boolean))
				return new BooleanTypeAdjuster();
			
			if (type.Value.Equals(CommonTypes.Undefined))
				return new UndefinedTypeAdjuster();

			return null;
		}

		[ContextMethod("ПривестиЗначение", "AdjustValue")]
		public IValue AdjustValue(IValue pValue = null)
		{
			var value = pValue?.GetRawValue();
			if (_types.Count == 0)
			{
				return value ?? ValueFactory.Create();
			}

			if (value != null)
			{
				var valueType = new TypeTypeValue(value.SystemType);
				if (ContainsType(valueType))
				{
					// Если такой тип у нас есть, возвращаем само значение применив квалификаторы, если они есть
					var adjuster = GetAdjusterForType(valueType);
					var adjustedValue = adjuster?.Adjust(value) ?? value;
					return adjustedValue;
				}
			}

			foreach (var type in _types)
			{
				var adjuster = GetAdjusterForType(type);
				var adjustedValue = adjuster?.Adjust(value);
				if (adjustedValue != null)
					return adjustedValue;
			}

			return ValueFactory.Create();
		}

		public static TypeDescription StringType(int length = 0,
		                                         AllowedLengthEnum allowedLength = AllowedLengthEnum.Variable)
		{
			return TypeDescriptionBuilder.OfType(CommonTypes.String)
				.SetStringQualifiers(new StringQualifiers(length, allowedLength))
				.Build();
		}

		public static TypeDescription IntegerType(int length = 10,
		                                          AllowedSignEnum allowedSign = AllowedSignEnum.Any)
		{
			return TypeDescriptionBuilder.OfType(CommonTypes.Number)
				.SetNumberQualifiers(new NumberQualifiers(length, 0, allowedSign))
				.Build();
		}

		public static TypeDescription BooleanType()
		{
			return TypeDescriptionBuilder.OfType(CommonTypes.Boolean).Build();
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

				// квалификакторы передаются только для контроля типов
				return ConstructByQualifiers(UndefinedValue.Instance, p2, p3, p4, p5, p6, p7);
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

		private static TypeDescription ConstructByQualifiers(IValue types,
			IValue p2 = null,
			IValue p3 = null,
			IValue p4 = null,
			IValue p5 = null,
			IValue p6 = null,
			IValue p7 = null)
		{
			var builder = new TypeDescriptionBuilder();
			var typesList = TypeList.Construct(types);
			CheckNotNull(typesList, 1);
			builder.AddTypes(typesList.List());

			CheckAndAddQualifiers(builder, new[] {null, p2, p3, p4, p5, p6, p7 });

			return builder.Build();
		}

		private static void CheckAndAddQualifiers(TypeDescriptionBuilder builder, IValue[] parameters)
		{
			for (var nParam = 0; nParam < parameters.Length; nParam++)
			{
				var rawQualifier = parameters[nParam]?.GetRawValue();
				if (rawQualifier != null && !rawQualifier.Equals(ValueFactory.Create()))
				{
					switch (rawQualifier)
					{
						case NumberQualifiers nq:
							builder.SetNumberQualifiers(nq);
							break;

						case StringQualifiers sq:
							builder.SetStringQualifiers(sq);
							break;

						case DateQualifiers dq:
							builder.SetDateQualifiers(dq);
							break;

						case BinaryDataQualifiers bdq:
							builder.SetBinaryDataQualifiers(bdq);
							break;

						default:
							throw RuntimeException.InvalidNthArgumentType(nParam + 1);
					}
				}
			}
		}

		private static TypeDescription ConstructByOtherDescription(
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
			
			var removeTypesList = TypeList.Construct(removeTypes);
			CheckNotNull(removeTypesList, 3);
			builder.RemoveTypes(removeTypesList.List());

			var addTypesList = TypeList.Construct(addTypes);
			CheckNotNull(addTypesList, 2);
			builder.AddTypes(addTypesList.List());
			
			CheckAndAddQualifiers(builder, new [] {null, null, null, p4, p5, p6, p7 });

			return builder.Build();
		}

		private static void CheckNotNull(object value, int nParam)
		{
			if (value == null)
			{
				throw RuntimeException.InvalidNthArgumentType(nParam);
			}
		}
	}

	internal class UndefinedTypeAdjuster : IValueAdjuster
	{
		public IValue Adjust(IValue value)
		{
			return ValueFactory.Create();
		}
	}
}
