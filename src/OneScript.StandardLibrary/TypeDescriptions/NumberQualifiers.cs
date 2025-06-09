/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Commons;
using OneScript.Contexts;
using OneScript.Exceptions;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.TypeDescriptions
{
	[ContextClass("КвалификаторыЧисла", "NumberQualifiers")]
	public sealed class NumberQualifiers : AutoContext<NumberQualifiers>, IValueAdjuster
	{
		public NumberQualifiers(int digits = 0,
		                            int fractionDigits = 0,
		                            AllowedSignEnum allowedSign = AllowedSignEnum.Any)
		{
			Digits = digits;
			FractionDigits = fractionDigits;
			AllowedSign = allowedSign;
		}

		[ContextProperty("ДопустимыйЗнак", "AllowedSign")]
		public AllowedSignEnum AllowedSign { get; }

		[ContextProperty("Разрядность", "Digits")]
		public int Digits { get; }

		[ContextProperty("РазрядностьДробнойЧасти", "FractionDigits")]
		public int FractionDigits { get; }

		public override bool Equals(object obj)
		{
			var asThis = obj as NumberQualifiers;
			if (asThis == null)
				return false;

			return Digits == asThis.Digits
			    && FractionDigits == asThis.FractionDigits
			    && AllowedSign == asThis.AllowedSign;
		}

		public override bool Equals(BslValue other)
		{
			return Equals((object)other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Digits, FractionDigits, AllowedSign);
		}

		public IValue Adjust(IValue value)
		{
			if (value == null)
			{
				// Значение по-умолчанию
				return ValueFactory.Create(0);
			}
			// TODO: обрезать по количеству знаков

			try
			{
				// TODO: Вменяемое преобразование без Попытки
				var numericValue = value.AsNumber();

				if (AllowedSign == AllowedSignEnum.Nonnegative && numericValue < 0)
				{
					numericValue = 0;
				}

				if (Digits > 0)
				{
					ValueFormatter.ApplyNumericSizeRestrictions(ref numericValue, Digits, FractionDigits);
				}
				return ValueFactory.Create(numericValue);

			} catch
			{
			}

			return ValueFactory.Create(0);
		}

		[ScriptConstructor(Name = "На основании описания числа")]
		public static NumberQualifiers Constructor(
			int digits = default,
			int fractionDigits = default,
			AllowedSignEnum allowedSign = default)
		{
			if (digits < 0 || fractionDigits < 0)
			{
				throw RuntimeException.InvalidArgumentValue();
			}
			
			return new NumberQualifiers(digits, fractionDigits, allowedSign);
		}
	}
}
