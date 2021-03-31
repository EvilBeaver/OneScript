/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Core;
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
			var asThis = obj as BinaryDataQualifiers;
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

		[ScriptConstructor]
		public static BinaryDataQualifiers Constructor(IValue length = null,
		                                                  IValue allowedLength = null)
		{
			var paramLength = ContextValuesMarshaller.ConvertParam<int>(length);
			var paramAllowedLength = ContextValuesMarshaller.ConvertParam<AllowedLengthEnum>(allowedLength);
			return new BinaryDataQualifiers(paramLength, paramAllowedLength);
		}
	}
}
