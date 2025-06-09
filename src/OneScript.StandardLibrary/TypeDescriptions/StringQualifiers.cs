/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.TypeDescriptions
{
	[ContextClass("КвалификаторыСтроки", "StringQualifiers")]
	public sealed class StringQualifiers : AutoContext<StringQualifiers>, IValueAdjuster
	{
		public StringQualifiers(int length = 0,
		                            AllowedLengthEnum allowedLength = AllowedLengthEnum.Variable)
		{
			Length = length;
			AllowedLength = allowedLength;
		}

		[ContextProperty("Длина", "Length")]
		public int Length { get; }

		[ContextProperty("ДопустимаяДлина", "AllowedLength")]
		public AllowedLengthEnum AllowedLength { get; }

		public override bool Equals(object obj)
		{
			var asThis = obj as StringQualifiers;
			if (asThis == null)
				return false;

			return Length == asThis.Length
			    && AllowedLength == asThis.AllowedLength;
		}

		public override bool Equals(BslValue other)
		{
			return Equals((object)other);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Length, AllowedLength);
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
			// FIXME: не пробрасывается процесс в приведение значения
			// Кастомизированные представления из UserScriptContextInstance при присваивании в строковые
			// колонки ТаблицыЗначений будут получать стандартное приведение, а не кастомное.
			var stringValue = value?.ToString() ?? "";

			if (Length != 0 && stringValue.Length > Length)
			{
				stringValue = stringValue.Substring(0, Length);
			}

			if (AllowedLength == AllowedLengthEnum.Fixed && stringValue.Length < Length)
			{
				var tail = new string(' ', Length - stringValue.Length);
				stringValue += tail;
			}

			return ValueFactory.Create(stringValue);
		}

		[ScriptConstructor(Name = "На основании описания строки")]
		public static StringQualifiers Constructor(int length = default,
		                                           AllowedLengthEnum allowedLength = default)
		{
			return new StringQualifiers(length, allowedLength);
		}
	}
}
