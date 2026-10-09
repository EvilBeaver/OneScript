/*----------------------------------------------------------
This Source Code Form is subject to the terms of the
Mozilla Public License, v.2.0. If a copy of the MPL
was not distributed with this file, You can obtain one
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using OneScript.Contexts;
using ScriptEngine.Machine.Contexts;

namespace Component
{
	// Компонента добавляет свой глобальный контекст, как в #344 и #911
	[GlobalContext(Category = "Тестовая компонента")]
	public class SimpleGlobalContext : GlobalContextBase<SimpleGlobalContext>
	{
		[ContextMethod("ГлобальныйМетодКомпоненты")]
		public string GlobalMethod() => "метод компоненты";

		[ContextProperty("ГлобальноеСвойствоКомпоненты")]
		public string GlobalProperty => "свойство компоненты";

		public static IAttachableContext CreateInstance() => new SimpleGlobalContext();
	}
}
