/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.HostedScript.Library
{
	[ContextClass("КвалификаторыЧисла", "NumberQualifiers")]
	public sealed class NumberQualifiersImpl : AutoContext<NumberQualifiersImpl>, IValueAdjuster
	{
		public NumberQualifiersImpl(int digits = 0,
		                            int fractionDigits = 0,
		                            AllowedSignEnum allowedSign = AllowedSignEnum.Any)
		{
			Digits = digits;
			FractionDigits = fractionDigits;
			AllowedSign = allowedSign;
		}

		[ContextProperty("ДопустимыйЗнак")]
		public AllowedSignEnum AllowedSign { get; }

		[ContextProperty("Разрядность")]
		public int Digits { get; }

		[ContextProperty("РазрядностьДробнойЧасти")]
		public int FractionDigits { get; }

		public override bool Equals(object obj)
		{
			var asThis = obj as NumberQualifiersImpl;
			if (asThis == null)
				return false;

			return Digits == asThis.Digits
			    && FractionDigits == asThis.FractionDigits
			    && AllowedSign == asThis.AllowedSign;
		}

		public override bool Equals(IValue other)
		{
			return object.Equals(this, other?.GetRawValue());
		}

		public override int GetHashCode()
		{
			return Digits.GetHashCode();
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
				return ValueFactory.Create(value.AsNumber());

			} catch
			{
			}

			return ValueFactory.Create(0);
		}

		[ScriptConstructor]
		public static IRuntimeContextInstance Constructor(IValue digits = null,
		                                                  IValue fractionDigits = null,
		                                                  IValue allowedSign = null)
		{
			var paramDigits         = ContextValuesMarshaller.ConvertParam<int>(digits);
			var paramFractionDigits = ContextValuesMarshaller.ConvertParam<int>(fractionDigits);
			var paramAllowedSign    = ContextValuesMarshaller.ConvertParam<AllowedSignEnum>(allowedSign);
			return new NumberQualifiersImpl(paramDigits, paramFractionDigits, paramAllowedSign);
		}
	}
}
