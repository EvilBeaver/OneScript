/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections.Generic;
using OneScript.Contexts;
using OneScript.Types;
using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace Component
{
	[ContextClass("ПростоКоллекция")]
	public sealed class SimpleCollection : AutoCollectionContext<SimpleCollection, SimpleClass>
	{
		private readonly List<SimpleClass> _data = new List<SimpleClass>();

		[ContextMethod("Добавить")]
		public void Add(SimpleClass item)
		{
			_data.Add(item);
		}

		[ScriptConstructor]
		public static IRuntimeContextInstance Constructor()
		{
			return new SimpleCollection();
		}

		[ContextMethod("Количество")]
		public override int Count()
		{
			return _data.Count;
		}

		public override IEnumerator<SimpleClass> GetEnumerator()
		{
			return ((IEnumerable<SimpleClass>) _data).GetEnumerator();
		}

	}
}