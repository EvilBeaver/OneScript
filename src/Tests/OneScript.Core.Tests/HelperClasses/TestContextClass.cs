/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using OneScript.Execution;
using OneScript.Types;
using OneScript.Values;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;
using ScriptEngine.Types;
using System.Collections.Generic;

namespace OneScript.Core.Tests
{
	[ContextClass("ТестовыйКласс", "TestClass")]
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

		[DeprecatedName("ObsoleteAlias", throwOnUse: false)]
		[DeprecatedName("VeryObsoleteAlias", throwOnUse: true)]
		[ContextMethod("ХорошийМетод", "GoodMethod")]
		public void GoodMethod()
		{
			// Do nothing
		}
		
		[DeprecatedName("OldBslProp")]
		[ContextProperty("СвойствоBsl","BslProp")]
		public string BslProp { get; set; }
		
		[ContextProperty("УстаревшееСвойство","ObsoleteProperty", IsDeprecated = true)]
		public string ObsoleteProp { get; set; }

		public override bool IsIndexed => true;

		public override IValue GetIndexedValue(IValue index)
		{
			return _indexedValues[(BslValue)index];
		}

		public override void SetIndexedValue(IValue index, IValue val)
		{
			_indexedValues[(BslValue)index] = (BslValue)val;
		}

        #region IBslProcessTests

        [ContextMethod("Процедура0")]
        public void Procedure0() { }

        [ContextMethod("Процедура0СПроцессом")]
        public void Procedure0WithProcess(IBslProcess process) { }

        [ContextMethod("Процедура1")]
        public void Procedure1(int intValue) { }

        [ContextMethod("Процедура1СПроцессом")]
        public void Procedure1WithProcess(IBslProcess process, int intValue) { }

        [ContextMethod("Процедура1СУмолчанием")]
        public void Procedure1WithDefault(int intValue = 0) { }

        [ContextMethod("Процедура1СУмолчаниемСПроцессом")]
        public void Procedure1WithDefaultWithProcess(IBslProcess process, int intValue = 0) { }

        [ContextMethod("Функция0")]
        public int Function0() { return 0; }

        [ContextMethod("Функция0СПроцессом")]
        public int Function0WithProcess(IBslProcess process) { return 0; }

        [ContextMethod("Функция1")]
        public int Function1(int intValue) { return intValue; }

        [ContextMethod("Функция1СПроцессом")]
        public int Function1WithProcess(IBslProcess process, int intValue) { return intValue; }

        [ContextMethod("Функция1СУмолчанием")]
        public int Function1WithDefault(int intValue = 0) { return intValue; }

        [ContextMethod("Функция1СУмолчаниемСПроцессом")]
        public int Function1WithDefaultWithProcess(IBslProcess process, int intValue = 0) { return intValue; }
        #endregion

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