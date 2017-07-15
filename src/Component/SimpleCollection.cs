/*----------------------------------------------------------
This Source Code Form is subject to the terms of the 
Mozilla Public License, v.2.0. If a copy of the MPL 
was not distributed with this file, You can obtain one 
at http://mozilla.org/MPL/2.0/.
----------------------------------------------------------*/
using System;
using ScriptEngine.Machine.Contexts;
using System.Collections.Generic;
using ScriptEngine.Machine;
using System.Collections;

namespace Component
{
	[ContextClass("ПростоКоллекция")]
	public sealed class SimpleCollection : AutoContext<SimpleCollection>, ICollectionContext, IEnumerable<SimpleClass>
	{

		readonly List<SimpleClass> _data = new List<SimpleClass>();

		public SimpleCollection()
		{
		}

		[ContextMethod("Добавить")]
		public void Add(SimpleClass item)
		{
			_data.Add(item);
		}

		[ContextMethod("Количество")]
		public int Count()
		{
			return _data.Count;
		}

		public IEnumerator<SimpleClass> GetEnumerator()
		{
			return ((IEnumerable<SimpleClass>)_data).GetEnumerator();
		}

		public CollectionEnumerator GetManagedIterator()
		{
			return new CollectionEnumerator(GetEnumerator());
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<SimpleClass>)_data).GetEnumerator();
		}

		[ScriptConstructor]
		public static IRuntimeContextInstance Constructor()
		{
			return new SimpleCollection();
		}
	}
}
