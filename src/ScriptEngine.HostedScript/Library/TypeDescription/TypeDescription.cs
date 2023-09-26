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
			var builder = new TypeDescriptionBuilder();
			
			// параметры, которые заведомо не квалификаторы, заменяем на null, но оставляем,
			// чтобы указать номер параметра при выводе ошибки несоответствия типа
			var qualifiers = new[] { null, p2, p3, p4, p5, p6, p7 };
			
			var rawSource = source?.GetRawValue();
			if (rawSource == null || rawSource.DataType == DataType.Undefined)
			{
				// пустой первый параметр - нет объекта-основания
				// добавляемые/вычитаемые типы не допускаются, квалификаторы игнорируются

				// квалификакторы передаются только для контроля типов
			}
			else
			if (rawSource is TypeDescription typeDesc)
			{
				// Если 1 парарметр - ОписаниеТипов, то 2 - добавляемые типы, 3 - убираемые типы,
				builder.SourceDescription(typeDesc);
				
				var typesToAdd = CheckAndParseTypeList(p2, 2);
				var typesToRemove = CheckAndParseTypeList(p3, 3);
				
				builder.RemoveTypes(typesToRemove);
				builder.AddTypes(typesToAdd);

				qualifiers[1] = null; // эти параметры не квалификаторы
				qualifiers[2] = null; // эти параметры не квалификаторы

			}
			else
			if (rawSource.DataType == DataType.String || rawSource is ArrayImpl)
			{
				// Если 1 парарметр - Массив или строка, то это набор конкретных типов
				// остальные параметры (2 и далее) - клвалификаторы в произвольном порядке
				var typesList = CheckAndParseTypeList(rawSource, 1);
				builder.AddTypes(typesList);
			} else
				throw RuntimeException.InvalidArgumentValue();
			
			CheckAndAddQualifiers(builder, qualifiers);
			return builder.Build();
		}

		/// <summary>
		/// Преобразует входящий параметр в список типов.
		/// </summary>
		/// <param name="types">В качестве типов могут быть переданы Строка или Массив Типов</param>
		/// <param name="nParam">Номер параметра, который будет указан в исключении, если параметр typeList задан неверно</param>
		/// <exception cref="RuntimeException">Если typeList не может быть разобран как набор типов</exception>
		/// <returns>Список переданных типов, приведенный к конкретным TypeTypeValue</returns>
		private static List<TypeTypeValue> CheckAndParseTypeList(IValue types, int nParam)
		{
			types = types?.GetRawValue();
			if (types == null || types.DataType == DataType.Undefined)
				return new List<TypeTypeValue>();
			
			if (types.DataType == DataType.String)
			{
				return FromTypeNames(types.AsString());
			}
			if (types is ArrayImpl arrayOfTypes)
			{
				return FromArrayOfTypes(arrayOfTypes);
			}

			throw RuntimeException.InvalidNthArgumentType(nParam);
		}

		private static List<TypeTypeValue> FromTypeNames(string types)
		{
			var typeNames = types.Split(',');
			var typesList = new List<TypeTypeValue>();
			foreach (var typeName in typeNames)
			{
				if (string.IsNullOrWhiteSpace(typeName))
					continue;

				var typeValue = new TypeTypeValue(typeName.Trim());
				if (!typesList.Contains(typeValue))
					typesList.Add(typeValue);
			}

			return typesList;
		}

		private static List<TypeTypeValue> FromArrayOfTypes(ArrayImpl arrayOfTypes)
		{
			var typesList = new List<TypeTypeValue>();
			foreach (var type in arrayOfTypes)
			{
				if (type.GetRawValue() is TypeTypeValue rawType)
				{
					typesList.Add(rawType);
				}
			}
			return typesList;
		}

		private static void CheckAndAddQualifiers(TypeDescriptionBuilder builder, IValue[] parameters)
		{
			for (var i = 0; i < parameters.Length; i++)
			{
				var rawQualifier = parameters[i]?.GetRawValue();
				if (rawQualifier != null && !rawQualifier.Equals(ValueFactory.Create()))
				{
					CheckAndAddOneQualifier(builder, rawQualifier, i + 1);
				}
			}
		}

		/// <summary>
		/// Проверяет, что переданный параметр является квалификатором типа.
		/// Если тип параметра не является квалификатором, бросает исключение с указанием номера параметра.
		/// </summary>
		/// <param name="builder">Построитель описания типов, которому будет присвоен квалификатор</param>
		/// <param name="qualifier">Проверяемый входящий параметр</param>
		/// <param name="nParam">Порядковый номер параметра для выброса исключения</param>
		/// <exception cref="RuntimeException">Если qualifier не является квалификатором типа</exception>
		private static void CheckAndAddOneQualifier(TypeDescriptionBuilder builder, IValue qualifier, int nParam)
		{
			switch (qualifier)
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
