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
	[ContextClass("КвалификаторыСтроки", "StringQualifiers")]
	public sealed class StringQualifiersImpl : AutoContext<StringQualifiersImpl>, IValueAdjuster
	{
		public StringQualifiersImpl(int length = 0,
		                            AllowedLengthEnum allowedLength = AllowedLengthEnum.Variable)
		{
			Length = length;
			AllowedLength = allowedLength;
		}

		[ContextProperty("Длина")]
		public int Length { get; }

		[ContextProperty("ДопустимаяДлина")]
		public AllowedLengthEnum AllowedLength { get; }

		public override bool Equals(object obj)
		{
			var asThis = obj as StringQualifiersImpl;
			if (asThis == null)
				return false;

			return Length == asThis.Length
			    && AllowedLength == asThis.AllowedLength;
		}

		public override bool Equals(IValue other)
		{
			return object.Equals(this, other?.GetRawValue());
		}

		public override int GetHashCode()
		{
			return Length.GetHashCode();
		}

		public string DefaultString()
		{
			if (AllowedLength == AllowedLengthEnum.Variable)
				return "";
			
			if (Length == 0)
				return "";

			return new string(' ', Length);
		}

		public IValue Adjust(IValue value)
		{
			var stringValue = value?.AsString() ?? "";

			if (Length != 0 && stringValue.Length > Length)
			{
				stringValue = stringValue.Substring(0, Length);
			}

			if (AllowedLength == AllowedLengthEnum.Fixed && stringValue.Length < Length)
			{
				var tail = new string(' ', Length - stringValue.Length);
				stringValue = string.Format("{0}{1}", stringValue, tail);
			}

			return ValueFactory.Create(stringValue);
		}

		[ScriptConstructor]
		public static IRuntimeContextInstance Constructor(IValue length = null,
		                                                  IValue allowedLength = null)
		{
			var paramLength        = ContextValuesMarshaller.ConvertParam<int>(length);
			var paramAllowedLength = ContextValuesMarshaller.ConvertParam<AllowedLengthEnum>(allowedLength);
			return new StringQualifiersImpl(paramLength, paramAllowedLength);
		}
	}
}
