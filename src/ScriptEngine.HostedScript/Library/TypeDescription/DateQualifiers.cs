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
	[ContextClass("КвалификаторыДаты", "DateQualifiers")]
	public sealed class DateQualifiers : AutoContext<DateQualifiers>, IValueAdjuster
	{
		public DateQualifiers(DateFractionsEnum dateFractions = DateFractionsEnum.DateTime)
		{
			DateFractions = dateFractions;
		}

		[ContextProperty("ЧастиДаты", "DateFractions")]
		public DateFractionsEnum DateFractions { get; }

		public override bool Equals(object obj)
		{
			var asThis = obj as DateQualifiers;
			if (asThis == null)
				return false;

			return DateFractions == asThis.DateFractions;
		}

		public override bool Equals(IValue other)
		{
			return object.Equals(this, other?.GetRawValue());
		}

		public override int GetHashCode()
		{
			return DateFractions.GetHashCode();
		}

		public IValue Adjust(IValue value)
		{
			if (value == null || value.DataType == DataType.Undefined)
				return ValueFactory.Create(new DateTime(1, 1, 1));

			try
			{
				// TODO: вменяемое приведение без Попытки
				var dateToAdjust = value.AsDate();

				switch (DateFractions)
				{
					case DateFractionsEnum.Date: return ValueFactory.Create(dateToAdjust.Date);
					case DateFractionsEnum.Time: return ValueFactory.Create(new DateTime(dateToAdjust.TimeOfDay.Ticks));
					default: return ValueFactory.Create(dateToAdjust);
				}
			}
			catch
			{
				return ValueFactory.Create(new DateTime(1, 1, 1));
			}

		}

		[ScriptConstructor(Name = "На основании описания даты")]
		public static DateQualifiers Constructor(IValue dateFractions = null)
		{
			var paramDateFractions = ContextValuesMarshaller.ConvertParam(dateFractions, DateFractionsEnum.DateTime);
			return new DateQualifiers(paramDateFractions);
		}
	}
}
