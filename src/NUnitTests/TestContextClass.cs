/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using ScriptEngine.Machine.Contexts;

namespace NUnitTests
{
	[ContextClass("ТестовыйКласс", "TestClass")]
	public class TestContextClass : AutoContext<TestContextClass>
	{
		
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
		
		[ScriptConstructor]
		public static TestContextClass Constructor()
		{
			return new TestContextClass();
		}
		
	}
}