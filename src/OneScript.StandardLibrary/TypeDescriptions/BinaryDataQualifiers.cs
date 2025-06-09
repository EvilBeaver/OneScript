/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using OneScript.Contexts;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace OneScript.StandardLibrary.TypeDescriptions
{
	[ContextClass("КвалификаторыДвоичныхДанных", "BinaryDataQualifiers")]
	public sealed class BinaryDataQualifiers : AutoContext<BinaryDataQualifiers>
	{
		public BinaryDataQualifiers(int length = 0,
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
			if (!(obj is BinaryDataQualifiers asThis))
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

		[ScriptConstructor]
		public static BinaryDataQualifiers Constructor(int length = default, AllowedLengthEnum allowedLength = default)
		{
			return new BinaryDataQualifiers(length, allowedLength);
		}
	}
}
