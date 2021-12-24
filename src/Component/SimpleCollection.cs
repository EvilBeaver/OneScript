/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/

using System.Collections;
using System.Collections.Generic;

using ScriptEngine.Machine;
using ScriptEngine.Machine.Contexts;

namespace Component
{
	[ContextClass("ПростоКоллекция")]
	public sealed class SimpleCollection : AutoContext<SimpleCollection>, ICollectionContext, IEnumerable<SimpleClass>
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
		public int Count()
		{
			return _data.Count;
		}

		public CollectionEnumerator GetManagedIterator()
		{
			return new CollectionEnumerator(GetEnumerator());
		}

		public IEnumerator<SimpleClass> GetEnumerator()
		{
			return ((IEnumerable<SimpleClass>) _data).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<SimpleClass>) _data).GetEnumerator();
		}
	}
}