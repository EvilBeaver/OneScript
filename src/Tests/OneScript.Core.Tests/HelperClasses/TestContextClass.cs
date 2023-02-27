/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;

namespace OneScript.Core.Tests
{
	[ContextClass("ТестовыйКласс", "TestClass", TypeUUID = "65E99482-F711-4FBC-AAC7-4BF7E2A124A5")]
	public class TestContextClass : AutoContext<TestContextClass>
	{
		private IDictionary<BslValue, BslValue> _indexedValues = new Dictionary<BslValue, BslValue>();

		public TestContextClass()
		{
			DefineType(GetType().GetTypeFromClassMarkup());
		}
		
		public string CreatedViaMethod { get; private set; }
		
		[ContextMethod("УстаревшийМетод", "ObsoleteMethod", IsDeprecated = true, ThrowOnUse = false)]
		public void ObsoleteMethod()
		{
			// Do nothing
		}

		[ContextMethod("ХорошийМетод", "GoodMethod")]
		[ContextMethod("ObsoleteAlias", IsDeprecated = true, ThrowOnUse = false)]
		[ContextMethod("VeryObsoleteAlias", IsDeprecated = true, ThrowOnUse = true)]
		public void GoodMethod()
		{
			// Do nothing
		}
		
		[ContextProperty("СвойствоBsl","BslProp")]
		public string BslProp { get; set; }

		public override bool IsIndexed => true;

		public override IValue GetIndexedValue(IValue index)
		{
			return _indexedValues[(BslValue)index];
		}

		public override void SetIndexedValue(IValue index, IValue val)
		{
			_indexedValues[(BslValue)index] = (BslValue)val;
		}

		[ScriptConstructor]
		public static TestContextClass Constructor()
		{
			return new TestContextClass
			{
				CreatedViaMethod = "Constructor0"
			};
		}
		
		[ScriptConstructor]
		public static TestContextClass Constructor(TypeActivationContext context, IValue ctorParam1, IValue ctorParam2)
		{
			return new TestContextClass
			{
				CreatedViaMethod = $"Constructor2-{context.TypeName}"
			};
		}
	}
}