/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;
using System.Globalization;
using System.Reflection;
using OneScript.Core;
using ScriptEngine.Machine.Contexts;

namespace ScriptEngine.Machine
{
	public sealed class ValueBinder : Binder
	{
		public override FieldInfo BindToField(BindingFlags bindingAttr, FieldInfo[] match, object value, CultureInfo culture)
		{
			throw new NotImplementedException(nameof(BindToField));
		}

		public override MethodBase BindToMethod(BindingFlags bindingAttr, MethodBase[] match, ref object[] args, ParameterModifier[] modifiers,
			CultureInfo culture, string[] names, out object state)
		{
			if (match.Length == 1)
			{
				// TODO: проверить количество параметров
				state = null;
				return match[0];
			}

			return Type.DefaultBinder.BindToMethod(bindingAttr, match, ref args, modifiers, culture, names, out state);
		}

		public override object ChangeType(object value, Type type, CultureInfo culture)
		{
			if (value is IValue)
			{
				return ContextValuesMarshaller.ConvertParam((IValue) value, type);
			}
			return Type.DefaultBinder.ChangeType(value, type, culture);
		}

		public override void ReorderArgumentArray(ref object[] args, object state)
		{
			throw new NotImplementedException(nameof(ReorderArgumentArray));
		}

		public override MethodBase SelectMethod(BindingFlags bindingAttr, MethodBase[] match, Type[] types, ParameterModifier[] modifiers)
		{
			throw new NotImplementedException(nameof(SelectMethod));
		}

		public override PropertyInfo SelectProperty(BindingFlags bindingAttr, PropertyInfo[] match, Type returnType, Type[] indexes,
			ParameterModifier[] modifiers)
		{
			throw new NotImplementedException(nameof(SelectProperty));
		}
	}
	}