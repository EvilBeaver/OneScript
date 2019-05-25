/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace Component
{
	[ContextClass("ПростоКласс")]
	public sealed class SimpleClass : AutoContext<SimpleClass>, ISimple
	{
		[ContextProperty("СвойствоПеречисление")]
		public SimpleEnum EnumProperty { get; set; }

		[ContextProperty("ЦелочисленноеСвойство")]
		public int IntProperty { get; set; }

		[ContextProperty("СвойствоСПроизвольнымЗначением")]
		public IValue AnyValueProperty { get; set; }

		[ScriptConstructor]
		public static SimpleClass Constructor()
		{
			return new SimpleClass();
		}

		[ScriptConstructor]
		public static SimpleClass Constructor(IValue initialProperty)
		{
			var result = new SimpleClass();
			result.IntProperty = ContextValuesMarshaller.ConvertParam<int>(initialProperty);
			return result;
		}
	}
}